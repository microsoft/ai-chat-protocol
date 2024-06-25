from quart import Quart, request, jsonify

app = Quart(__name__)

def run() -> None:
    app.run()

@app.route('/api/chat', methods=['POST'])
async def process_message():
    if request.content_type.startswith('multipart/form-data'):
        files = await request.files()
        json_data = files['json']  # Assuming 'json' is the key for JSON part
        chat_request = AIChatRequest.from_json(json_data)
        # Process files if any
        for file_key in files:
            file = files[file_key]
            # Process file here, e.g., save or analyze
            # Add file data to chat_request if necessary
    elif request.content_type.startswith('application/json'):
        # Parse application/json
        data = await request.get_json()
        # chat_request = AIChatRequest.from_dict(data)
    else:
        return jsonify({'error': 'Unsupported Media Type'}), 415

    # Process the chat request
    # chat_response = await process_chat_request(chat_request)

    # Return the response
    return jsonify(chat_response.to_dict())
