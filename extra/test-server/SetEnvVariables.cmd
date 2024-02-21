REM
REM A windows batch script to set environment variables
REM for the supported back-end model endpoints.
REM
REM Replace all <values> with your own.
REM

REM Azure OpenAI endpoint
setx SAMPLE_CHAT_SERVICE_AZURE_OPENAI_KEY <your-key>
setx SAMPLE_CHAT_SERVICE_AZURE_OPENAI_ENDPOINT <your-endpoint>
setx SAMPLE_CHAT_SERVICE_AZURE_OPENAI_DEPLOYMENT <your-deployment>

REM Model-as-a-Service endpoint
setx SAMPLE_CHAT_SERVICE_MAAS_KEY <your-key>
setx SAMPLE_CHAT_SERVICE_MAAS_ENDPOINT <your-endpoint>

REM Llama2 Model-as-a-Platform endpoint
setx SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_KEY <your-key>
setx SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_ENDPOINT <your-endpoint>
setx SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_DEPLOYMENT <your-deployment>
