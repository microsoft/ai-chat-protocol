import dotenv from "dotenv";

dotenv.config();

export enum ConfigParameter {
  port,
  azureOpenAIEndpoint,
  azureOpenAIDeployment,
  redisUrl,
  systemPrompt,
  stateTTL,
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
      case ConfigParameter.port: {
        return process.env.PORT;
      }
      case ConfigParameter.redisUrl: {
        return process.env.REDIS_URL;
      }
      case ConfigParameter.systemPrompt: {
        return process.env.SYSTEM_PROMPT;
      }
      case ConfigParameter.stateTTL: {
        return process.env.STATE_TTL;
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
