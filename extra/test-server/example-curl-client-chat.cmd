REM
REM This script runs an example cURL command to talk to the ChatProtocolSample local-host service.
REM
REM Usage (in a Windows command prompt):
REM
REM    example-curl-client-chat.cmd <port-number>
REM
REM Where:
REM    <port-number> is the port the local-host service is listening to.
REM
REM Example:
REM    example-curl-client-chat.cmd 59741
REM

IF "%~1"=="" (
    ECHO ERROR: No port number provided.
    ECHO Usage: example-curl-client-chat.cmd ^<port-number^>
    EXIT /B 1
)

REM Good for testing non-streaming:
curl -v "https://localhost:%1/chat" -N -H "Content-Type: application/json" --data-ascii "{\"messages\": [{\"role\":\"system\",\"content\":\"You are an AI assistant that helps people find information.\"},{\"role\":\"user\",\"content\":\"How many feet in a mile??\"}]}"
REM curl -v "https://localhost:%1/chat" -H "Content-Type: application/json" --data-ascii "{\"messages\": [{\"role\":\"system\",\"content\":\"You are a helpful assistant. You will talk like a pirate.\"},{\"role\":\"user\",\"content\":\"Can you help me?\"},{\"role\":\"assistant\",\"content\":\"Arrrr! Of course, me hearty! What can I do for ye?\"},{\"role\":\"user\",\"content\":\"What's the best way to train a parrot?\"}]}"


REM Good for testing streaming (long response):
REM curl -v "https://localhost:%1/chat/stream" -N -H "Content-Type: application/json" --data-ascii "{\"messages\": [{\"role\":\"system\",\"content\":\"You are an AI assistant that helps people find information.\"},{\"role\":\"user\",\"content\":\"Give me ten reasons to regularly exercise.\"}]}"
