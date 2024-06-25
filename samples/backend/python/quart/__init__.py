import base64
import json
import os
import re
from dotenv import load_dotenv
from typing import Optional, Tuple
from openai import AsyncAzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from quart import Quart, request, jsonify, stream_with_context
from model import (
    AIChatRequest,
    AIChatCompletion,
    AIChatMessage,
    AIChatRole,
    AIChatFile,
    AIChatError,
    AIChatCompletionDelta,
    AIChatMessageDelta,
)

load_dotenv()

PORT = os.getenv("PORT", 3000)
AZURE_OPENAI_ENDPOINT = os.getenv("AZURE_OPENAI_ENDPOINT")
AZURE_OPENAI_DEPLOYMENT = os.getenv("AZURE_OPENAI_DEPLOYMENT")
AZURE_OPENAI_API_VERSION = os.getenv("AZURE_OPENAI_API_VERSION")

token_provider = get_bearer_token_provider(
    DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default"
)
client = AsyncAzureOpenAI(
    api_version=AZURE_OPENAI_API_VERSION,
    azure_endpoint=AZURE_OPENAI_ENDPOINT,
    azure_ad_token_provider=token_provider,
)

app = Quart(__name__)


def run() -> None:
    app.run(port=PORT)


def get_file_position(file_key: str) -> Optional[Tuple[int, int, str]]:
    match = re.match(r"messages\[(\d+)\]\.files\[(\d+)\]", file_key)
    if match:
        message_index, file_index = map(int, match.groups())
        return message_index, file_index, file_key
    raise ValueError(f"Invalid file name: {file_key}")


def reconstruct_multipart_request(form: dict, files: dict):
    json_content = form["json"]
    json_dict = json.loads(json_content)
    chat_request = AIChatRequest.from_dict(json_dict)

    file_positions = sorted([get_file_position(file_key) for file_key in files])
    for message_index, file_index, file_key in file_positions:
        file = files[file_key]

        if len(chat_request.messages) <= message_index:
            raise ValueError(f"Invalid message index: {file_key}")

        if chat_request.messages[message_index].files is None:
            chat_request.messages[message_index].files = []

        if len(chat_request.messages[message_index].files) != file_index:
            raise ValueError(f"Invalid file index: {file_key}")

        chat_request.messages[message_index].files.append(
            AIChatFile(content_type=file.content_type, data=file.read())
        )
    return chat_request


def to_openai_message(chat_message: AIChatMessage):
    if chat_message.files is None:
        return {
            "role": chat_message.role.value,
            "content": chat_message.content,
        }

    def encode_file_to_data_url(file: AIChatFile):
        base64_data = base64.b64encode(file.data).decode("utf-8")
        return f"data:{file.content_type};base64,{base64_data}"

    images = [
        {"type": "image_url", "image_url": {"url": encode_file_to_data_url(file)}}
        for file in chat_message.files
        if file.content_type.startswith("image/")
    ]
    return {
        "role": chat_message.role.value,
        "content": [{"type": "text", "text": chat_message.content}] + images,
    }


@app.route("/api/chat/", methods=["POST"])
async def process_message():
    try:
        if request.content_type.startswith("multipart/form-data"):
            form = await request.form
            files = await request.files
            chat_request = reconstruct_multipart_request(form, files)
        elif request.content_type.startswith("application/json"):
            chat_request_dict = await request.get_json()
            chat_request = AIChatRequest.from_dict(chat_request_dict)
        else:
            return jsonify({"error": "Unsupported Media Type"}), 415
        completion = await client.chat.completions.create(
            model=AZURE_OPENAI_DEPLOYMENT,
            messages=[to_openai_message(message) for message in chat_request.messages],
        )

        message = completion.choices[0].message
        response = AIChatCompletion(
            message=AIChatMessage(
                role=AIChatRole(message.role),
                content=message.content,
            )
        )
        return jsonify(response.to_dict())
    except Exception as e:
        return (
            jsonify(AIChatError(code="internal_error", message=str(e)).to_dict()),
            500,
        )


def object_to_json_line(obj):
    return f"{json.dumps(obj.to_dict())}\r\n"


@app.route("/api/chat/stream", methods=["POST"])
async def process_message_stream():
    @stream_with_context
    async def async_generator():
        try:
            if request.content_type.startswith("multipart/form-data"):
                form = await request.form
                files = await request.files
                chat_request = reconstruct_multipart_request(form, files)
            elif request.content_type.startswith("application/json"):
                chat_request = await request.get_json()
            else:
                yield object_to_json_line(
                    AIChatError(
                        code="unsupported_media_type", message="Unsupported Media Type"
                    )
                )
                return

            stream = await client.chat.completions.create(
                model=AZURE_OPENAI_DEPLOYMENT,
                stream=True,
                messages=[
                    to_openai_message(message) for message in chat_request.messages
                ],
            )

            async for chunk in stream:
                if len(chunk.choices) == 0:
                    continue
                delta = chunk.choices[0].delta
                response_chunk = AIChatCompletionDelta(
                    delta=AIChatMessageDelta(
                        content=delta.content,
                        role=delta.role,
                    )
                )
                yield object_to_json_line(response_chunk)

        except Exception as e:
            error = AIChatError(code="internal_error", message=str(e))
            yield object_to_json_line(error)

    return async_generator(), 200, {"Content-Type": "application/jsonl"}


if __name__ == "__main__":
    run()
