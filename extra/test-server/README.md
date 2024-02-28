# Chat Protocol Sample Service

This sample runs as a local-host service, translating a REST chat protocol call from a local client, to one of the supported back-end chat model endpoints.
Three types of chat endpoints are supported at the moment:

* Azure OpenAI endpoint
* Model-as-a-Service (MaaS, aka "Pay-as-you-go") endpoint, hosting any model
* Model-as-a-Platform (MaaP, aka "Real-time endpoint") endpoint, hosting a Llama-2 model

For the sample to work, you need a REST client that implements the chat protocol. This can be, for example, cURL or PostMan.
See [example-curl-client-chat.cmd](./example-curl-client-chat.cmd) for some examples of cURL command you can use.

You need to define 2 or 3 environment variables, per each one of the backend services you would like to use, as specified in the sections below.

## How to run the Sample Service

Instructions here are for running on Windows, but can be adapted for Linux and macOS:

* Open a command prompt window in this folder
* If this is your first time:
  * Edit the local batch script [SetEnvVariables.cmd](./SetEnvVariables.cmd) and populate your values. Run it to define the variables.
  * Reopen the command prompt window so the environment variables take effect
* Optional: edit the assignment `GlobalSettings.backendChatService =` at top of the source file [Program.cs](./Program.cs) based
on the desired default backend service. You can change this selection at runtime. See section "How to configure the Sample Service" below.

* Build and run the service:
    ```cmd
    dotnet build
    dotnet run
    ```
* You should see console printout similar to the following:
    ```
    info: Microsoft.Hosting.Lifetime[14]
          Now listening on: https://localhost:59741
    info: Microsoft.Hosting.Lifetime[14]
          Now listening on: http://localhost:59742
    info: Microsoft.Hosting.Lifetime[0]
          Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
          Hosting environment: Development
    info: Microsoft.Hosting.Lifetime[0]
          Content root path: e:\src\carbon.dev\hacks\ChatProtocolSample
    ```
* Notice the port number (59741 in the above example)
* At any point, you can stop the service from running by pressing `CTRL-C`.
* Open a second command prompt in the current folder
* Edit the script [example-curl-client-chat.cmd](./example-curl-client-chat.cmd) as needed, and run it with a single argument -- the port number you saw above:
    ```
    example-curl-client-chat.cmd <port-number>
    ```
* This will make a REST call to the local-host service, which will be adapted and forwarded to the selected backend service.

## How to configure the Sample Service

The default selection of which back-end chat service to contact is hard-code in the source code. See `GlobalSettings.backendChatService = ...` statement at the top of the [Program.cs](./Program.cs) file.

You can change this default while the service is running, by sending an HTTP Post request to the service with path `/config` and query parameter `backend=<backend-service-id>`, where `<backend-service-id>` is an integer value 0, 1 or 2, representing the supported back-end services. See `BackendChatService` enum defined in [GlobalSettings.cs](./GlobalSettings.cs).

You can use the provided batch script [example-curl-service-configuration.cmd](./example-curl-service-configuration.cmd) to run a cURL command to update the back-end service:
```
example-curl-service-configuration.cmd <port-number> <backend-service-id>
```

## To use Azure OpenAI endpoint

### Environment variables

| Environment Variable | Description |
|----------------------|-------------|
| SAMPLE_CHAT_SERVICE_AZURE_OPENAI_KEY | Your Azure OpenAI key, a 32-character Hexadecimal number |
| SAMPLE_CHAT_SERVICE_AZURE_OPENAI_ENDPOINT | Your Azure OpenAI endpoint, in the form `https://<your-endpoint-name>.openai.azure.com` |
| SAMPLE_CHAT_SERVICE_AZURE_OPENAI_DEPLOYMENT | The deployment to use, for example `gpt-4` |

## To use MaaS endpoint

This is the "Pay-as-you-go" deployment option in Azure AI Studio.

For any model (e.g. Llama-2) deployed as MaaS, the JSON format of the HTTP request & response body is similar to what you would use when communicating with an Azure OpenAI model.

For more information, see the [Chat API](https://learn.microsoft.com/azure/ai-studio/how-to/deploy-models-llama#chat-api) section 
under "Reference for Llama 2 models deployed as a service". Look only at the "Chat API" section, since "Completions API" is likley to get deprecated.

### Environment variables

| Environment Variable | Description |
|----------------------|-------------|
| SAMPLE_CHAT_SERVICE_MAAS_KEY | The `Primary key` value from your Azure AI Studio `Deployments` page. A 32-character alphanumeric string. |
| SAMPLE_CHAT_SERVICE_MAAS_ENDPOINT | The `Endpoint - URL` value from your Azure AI Studio `Deployments` page. It has the form `https://<your-endpoint-name>.<your-azure-region>.inference.ai.azure.com/v1/chat/completions`. |

## To use Llama-2 MaaP endpoint

### Overview

This is the "Real-time endpoint" deployment option in Azure AI Studio.

When the Llama-2 model is deployed as MaaP, the JSON format of the HTTP request & response body is a bit different than what you would use when communicating with OpenAI model.

HTTP request headers:
```
Authorization: Bearer <key>
azureml-model-deployment: <deployment>
```

Example HTTP request body:
```json
{"input_data":{"input_string":[{"content":"You are a helpful assistant. You will talk like a pirate.","role":"system"},{"content":"Can you help me?","role":"user"},{"content":"Arrrr! Of course, me hearty! What can I do for ye?","role":"assistant"},{"content":"What's the best way to train a parrot?","role":"user"}]}}
```

Example HTTP response body:
```json
{"output": "Arrrr, shiver me timbers! Trainin' a parrot"}
```
For more information, see:

* Sample inputs and outputs (for real-time inference), in the Azure AI Studio [Llama-2-7b-chat model details](https://int.ai.azure.com/explore/models/Llama-2-7b-chat/version/17/registry/azureml-meta?tid=72f988bf-86f1-41af-91ab-2d7cd011db47)
* [Deploy Llama 2 models to real-time endpoints](https://learn.microsoft.com/azure/ai-studio/how-to/deploy-models-llama#deploy-llama-2-models-to-real-time-endpoints)

### Environment variables

| Environment Variable | Description |
|----------------------|-------------|
| SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_KEY | The `Primary key` value from your Azure AI Studio `Deployments` page. A 32-character alphanumeric string. |
| SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_ENDPOINT | The `Endpoint - URL` value from your Azure AI Studio `Deployments` page. It has the form `https://<your-endpoint-name>.<your-azure-region>.inference.ml.azure.com/score`. |
| SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_DEPLOYMENT | The `Deployment attributes - Name` value from your Azure AI Studio `Depolyments` page. For example `llama-2-7b-chat-17`. |
