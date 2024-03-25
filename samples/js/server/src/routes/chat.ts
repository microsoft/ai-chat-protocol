import { DefaultAzureCredential } from "@azure/identity";
import { ChatRequestMessage, OpenAIClient } from "@azure/openai";
import express, { Router, Request } from "express";
import { ConfigParameter, getConfig } from "../config";
import {
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionRequest,
  AIChatFinishReason,
  AIChatRole,
} from "@microsoft/ai-chat-protocol-model";

const chat = Router();

chat.use(express.json());

const client = new OpenAIClient(
  getConfig(ConfigParameter.azureOpenAIEndpoint),
  new DefaultAzureCredential(),
);
chat.post(
  "/",
  async (req: Request<{}, {}, AIChatCompletionRequest>, res, next) => {
    try {
      const response = await client.getChatCompletions(
        getConfig(ConfigParameter.azureOpenAIDeployment),
        req.body.messages.map((message) => {
          return {
            role: message.role,
            content: message.content,
          } as ChatRequestMessage;
        }),
      );
      const choice = response.choices[0];
      const completion: AIChatCompletion = {
        message: {
          role: (choice?.message?.role ?? undefined) as AIChatRole,
          content: choice?.message?.content ?? "",
        },
        finishReason: choice.finishReason as AIChatFinishReason,
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
      const response = await client.streamChatCompletions(
        getConfig(ConfigParameter.azureOpenAIDeployment),
        req.body.messages.map((message) => {
          return {
            role: message.role,
            content: message.content,
          } as ChatRequestMessage;
        }),
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
