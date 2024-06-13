# Microsoft AI Chat Protocol Samples

> This directory contains basic starters for using the AI Chat Protocol. If you're interested in more in-depth, end-to-end samples hosted on Azure, visit this [link](https://aka.ms/aichat/templates).

If you'd like to run the samples, follow these steps:

## Frontend

1. Clone the repository to your machine.
1. In one terminal, navigate to the `frontend` directory.
1. In the `frontend` directory, run `npm install` to install your dependencies, including [`@microsoft/ai-chat-protocol`](https://www.npmjs.com/package/@microsoft/ai-chat-protocol).
1. Next, run `npm run dev` to start your web application.

## Backend

The backend directory has both a .NET and a JavaScript (Express) backend sample. Follow the steps below for the sample you'd like to run.

### .NET (with Semantic Kernel)

1. In one terminal, navigate to the `backend/csharp` directory.
2. Set the following environment variables:
    1. UseAzureOpenAI - either `true` or `false`
        1. If using Azure OpenAI, set your `AzureDeployment` and `AzureEndpoint` according to this [guide](https://learn.microsoft.com/en-us/azure/ai-services/openai/quickstart?tabs=command-line%2Cpython-new&pivots=programming-language-python#retrieve-key-and-endpoint).
        2. Sign into Azure using the Azure CLI (`az login`) or Azure Developer CLI (`azd auth login`).
    2. If you're using OpenAI (*not* Azure OpenAI), set the environment variables `APIKey` and `Model`.
3. Next, run `dotnet restore` to restore your dependencies and `dotnet run` to run the backend.

### JavaScript (Express)

1. In one terminal, navigate to the `backend/js` directory.
2. In the `.env` file, update `AZURE_OPENAI_ENDPOINT` and `AZURE_OPENAI_DEPLOYMENT` according to this [guide](https://learn.microsoft.com/en-us/azure/ai-services/openai/quickstart?tabs=command-line%2Cpython-new&pivots=programming-language-python#retrieve-key-and-endpoint).
3. Run `npm install` to install your dependencies.
4. Run `npm run dev` to run the backend.
