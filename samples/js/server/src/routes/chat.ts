import { DefaultAzureCredential } from "@azure/identity";
import { ChatRequestMessage, OpenAIClient } from "@azure/openai";
import express, { Router, Request } from "express";
import { ConfigParameter, getConfig } from "../config";

const chat = Router();

chat.use(express.json());

interface ChatMessage {
  role: string;
  content: string;
}

interface ChatCompletionRequest {
  messages: ChatMessage[];
}

const client = new OpenAIClient(
  getConfig(ConfigParameter.azureOpenAIEndpoint),
  new DefaultAzureCredential(),
);
chat.post(
  "/",
  async (req: Request<{}, {}, ChatCompletionRequest>, res, next) => {
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
      const completion = {
        message: {
          role: choice?.message?.role,
          content: choice?.message?.content,
        },
      };
      res.json(completion);
    } catch (error) {
      return next(error);
    }
  },
);

chat.post(
  "/stream",
  async (req: Request<{}, {}, ChatCompletionRequest>, res, next) => {
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
          /* TODO: Add chat client library types */
          const completion = {
            delta: {
              content: choice?.delta?.content,
              role: choice?.delta?.role,
            },
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
