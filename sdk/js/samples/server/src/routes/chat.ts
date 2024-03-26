import { DefaultAzureCredential } from "@azure/identity";
import { ChatRequestMessage, OpenAIClient } from "@azure/openai";
import express, { Router, Request } from "express";
import { ConfigParameter, getConfig } from "../config";
import {
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionRequest,
  AIChatFinishReason,
  AIChatMessage,
  AIChatRole,
} from "@microsoft/ai-chat-protocol-model";
import { StateStore } from "../state-store";

const chat = Router();

chat.use(express.json());

const client = new OpenAIClient(
  getConfig(ConfigParameter.azureOpenAIEndpoint),
  new DefaultAzureCredential(),
);

const stateStore = new StateStore<AIChatMessage[]>();
stateStore.connect();

async function getMessageHistory(
  request: AIChatCompletionRequest,
): Promise<[AIChatMessage[], string | undefined]> {
  const key = request.sessionState as string;
  if (key) {
    try {
      return [await stateStore.read(key), key];
    } catch (error) {
      console.error(`Failed to read state for key ${key}: ${error}`);
    }
  }
  return [
    [
      {
        role: "system",
        content: getConfig(ConfigParameter.systemPrompt),
      },
    ],
    undefined,
  ];
}

chat.post(
  "/",
  async (req: Request<{}, {}, AIChatCompletionRequest>, res, next) => {
    try {
      const [history, key] = await getMessageHistory(req.body);
      const updatedHistory = [
        ...history,
        req.body.messages.pop() as AIChatMessage,
      ];
      const response = await client.getChatCompletions(
        getConfig(ConfigParameter.azureOpenAIDeployment),
        updatedHistory as ChatRequestMessage[],
      );
      const choice = response.choices[0];
      const responseMessage: AIChatMessage = {
        role: (choice?.message?.role ?? undefined) as AIChatRole,
        content: choice?.message?.content ?? "",
      };

      const completion: AIChatCompletion = {
        message: responseMessage,
        finishReason: choice.finishReason as AIChatFinishReason,
        sessionState: stateStore.save(key, [
          ...updatedHistory,
          responseMessage,
        ]),
      };
      res.json(completion);
    } catch (error) {
      return next(error);
    }
  },
);

chat.post(
  "/stream",
  async (req: Request<{}, {}, AIChatCompletionRequest>, res, next) => {
    try {
      const history = (await getMessageHistory(
        req.body,
      )) as ChatRequestMessage[];
      const response = await client.streamChatCompletions(
        getConfig(ConfigParameter.azureOpenAIDeployment),
        [...history, req.body.messages.pop() as ChatRequestMessage],
      );
      res.contentType("application/x-ndjson");
      for await (const event of response) {
        const choice = event.choices[0];
        if (choice) {
          const completion: AIChatCompletionDelta = {
            delta: {
              content: choice?.delta?.content ?? undefined,
              role: (choice?.delta?.role ?? undefined) as AIChatRole,
            },
            finishReason: choice.finishReason as AIChatFinishReason,
          };
          res.write(JSON.stringify(completion) + "\r\n");
        }
      }
      res.end();
    } catch (error) {
      return next(error);
    }
  },
);

export default chat;
