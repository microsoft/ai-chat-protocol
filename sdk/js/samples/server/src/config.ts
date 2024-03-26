export enum ConfigParameter {
  port,
  azureOpenAIEndpoint,
  azureOpenAIDeployment,
}

export function getConfig(parameter: ConfigParameter): string {
  const getValue = () => {
    switch (parameter) {
      case ConfigParameter.azureOpenAIEndpoint: {
        return process.env.AZURE_OPENAI_ENDPOINT;
      }
      case ConfigParameter.azureOpenAIDeployment: {
        return process.env.AZURE_OPENAI_DEPLOYMENT;
      }
      default: {
        throw new Error("Unsupported config parameter.");
      }
    }
  };
  const value = getValue();
  if (!value) {
    throw new Error("Not found.");
  }
  return value;
}
