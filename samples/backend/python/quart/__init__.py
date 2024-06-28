import base64
import os
import re
from typing import Optional

from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from dotenv import load_dotenv
from openai import AsyncAzureOpenAI
from pydantic import BaseModel

from model import (
    AIChatCompletion,
    AIChatCompletionDelta,
    AIChatError,
    AIChatFile,
    AIChatMessage,
    AIChatMessageDelta,
    AIChatRequest,
    AIChatRole,
)
from quart import Quart, jsonify, request, stream_with_context

load_dotenv()

PORT = os.getenv("PORT", 3000)
AZURE_OPENAI_ENDPOINT = os.getenv("AZURE_OPENAI_ENDPOINT")
AZURE_OPENAI_DEPLOYMENT = os.getenv("AZURE_OPENAI_DEPLOYMENT")
AZURE_OPENAI_API_VERSION = os.getenv("AZURE_OPENAI_API_VERSION")

token_provider = get_bearer_token_provider(DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default")
client = AsyncAzureOpenAI(
    api_version=AZURE_OPENAI_API_VERSION,
    azure_endpoint=AZURE_OPENAI_ENDPOINT,
    azure_ad_token_provider=token_provider,
)

app = Quart(__name__)


def run() -> None:
    app.run(port=PORT)


def get_file_position(file_key: str) -> tuple[int, int, str]:
    """
    Extracts the message and file indices from a given file key.

    The function expects file keys in the format "messages[<message_index>].files[<file_index>]",
    where <message_index> and <file_index> are integers. It parses these indices from the file key
    and returns them along with the original file key as a tuple.

    Args:
        file_key (str): The key representing a file's position in the message structure,
                        expected to follow the specific format mentioned above.

    Returns:
        tuple[int, int, str]: A tuple containing the message index, file index,
                              and the original file key if the key matches the expected format.

    Raises:
        ValueError: If the file key does not match the expected format.
    """
    match = re.match(r"messages\[(\d+)\]\.files\[(\d+)\]", file_key)
    if match:
        message_index, file_index = map(int, match.groups())
        return message_index, file_index, file_key
    raise ValueError(f"Invalid file name: {file_key}")


def reconstruct_multipart_request(form: dict, files: dict):
    """
    Reconstructs an AIChatRequest object from multipart form data.

    This function takes form data and a dictionary of files, then reconstructs
    the AIChatRequest object by parsing the JSON content from the form and attaching
    the files to their corresponding messages based on their keys.

    Args:
        form (dict): A dictionary containing the form data, expected to have a "json" key
                     with the JSON representation of the AIChatRequest.
        files (dict): A dictionary where keys are file keys in the format
                      "messages[<message_index>].files[<file_index>]" and values are the file objects.

    Returns:
        AIChatRequest: The reconstructed AIChatRequest object with files attached to the appropriate messages.

    Raises:
        ValueError: If any file key does not match the expected format, or if the indices in the file keys
                    do not correspond to valid positions in the reconstructed AIChatRequest object.
    """
    json_content = form["json"]
    chat_request = AIChatRequest.model_validate_json(json_content)

    file_positions = sorted([get_file_position(file_key) for file_key in files])
    for message_index, file_index, file_key in file_positions:
        file = files[file_key]

        if len(chat_request.messages) <= message_index:
            raise ValueError(f"Invalid message index: {file_key}")

        if chat_request.messages[message_index].files is None:
            chat_request.messages[message_index].files = []

        if len(chat_request.messages[message_index].files) != file_index:
            raise ValueError(f"Invalid file index: {file_key}")

        chat_request.messages[message_index].files.append(AIChatFile(content_type=file.content_type, data=file.read()))
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
            chat_request_data = await request.data
            chat_request = AIChatRequest.model_validate_json(chat_request_data)
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
            ),
        )
        return jsonify(response.model_dump())
    except Exception as e:
        return (
            jsonify(AIChatError(code="internal_error", message=str(e)).model_dump()),
            500,
        )


def object_to_json_line(obj: BaseModel):
    return f"{obj.model_dump_json()}\r\n"


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
                chat_request_data = await request.data
                chat_request = AIChatRequest.model_validate_json(chat_request_data)
            else:
                yield object_to_json_line(AIChatError(code="unsupported_media_type", message="Unsupported Media Type"))
                return

            stream = await client.chat.completions.create(
                model=AZURE_OPENAI_DEPLOYMENT,
                stream=True,
                messages=[to_openai_message(message) for message in chat_request.messages],
            )

            async for chunk in stream:
                if len(chunk.choices) == 0:
                    continue
                delta = chunk.choices[0].delta
                response_chunk = AIChatCompletionDelta(
                    delta=AIChatMessageDelta(
                        content=delta.content,
                        role=delta.role,
                    ),
                )
                yield object_to_json_line(response_chunk)

        except Exception as e:
            error = AIChatError(code="internal_error", message=str(e))
            yield object_to_json_line(error)

    return async_generator(), 200, {"Content-Type": "application/jsonl"}


if __name__ == "__main__":
    run()
