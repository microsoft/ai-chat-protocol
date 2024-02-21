REM
REM Use the cURL command in this script to control which back-end service
REM the sample local-host proxy service will talk to.
REM
REM Usage (in a Windows command prompt):
REM 
REM    example-curl-service-configuration.cmd <port-number> <back-end-service-id>
REM
REM Where
REM    <port-number> is the port the local-host service is listening to.
REM    <back-end-service-id> is one of these single digit intergers:
REM         0 - Azure OpenAI endpoint
REM         1 - MaaS endpoint
REM         2 - Llama2 PaaS endpoint
REM
REM Eample:
REM     example-curl-service-configuration.cmd 59741 1
REM

REM Good for testing non-streaming:
curl -v "https://localhost:%1/config?backend=%2" -H "Content-Type: application/json" --data-ascii "{}"
