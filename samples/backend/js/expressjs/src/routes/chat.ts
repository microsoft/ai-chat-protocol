// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { DefaultAzureCredential } from "@azure/identity";
import { ChatRequestMessage, OpenAIClient } from "@azure/openai";
import express, { Router, Request } from "express";
import { v4 as uuid } from "uuid";

import { ConfigParameter, getConfig } from "../config";
import {
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionRequest,
  AIChatMessage,
  AIChatRole,
} from "@microsoft/ai-chat-protocol";
import { StateStore } from "../state-store";

declare global {
  namespace Express {
    interface Request {
      sessionState: string;
    }
  }
}

const chat = Router();

chat.use(express.json());

const client = new OpenAIClient(
  getConfig(ConfigParameter.azureOpenAIEndpoint),
  new DefaultAzureCredential(),
);

const stateStore = new StateStore<AIChatMessage[]>();

type ChatRequest = Request<{}, {}, AIChatCompletionRequest>;

chat.use((req: ChatRequest, res, next) => {
  const request = req.body;
  if (request.sessionState && typeof request.sessionState == "string") {
    req.sessionState = request.sessionState as string;
    try {
      const history = stateStore.read(request.sessionState);
      console.info(`Loaded history for session ${request.sessionState}`);
      req.body.messages = [...history, ...request.messages];
      return next();
    } catch (error) {
      console.error(
        `Failed to load history for session ${request.sessionState}: ${error}`,
      );
    }
  } else {
    req.sessionState = uuid();
  }

  request.messages = [
    {
      role: "system",
      content: getConfig(ConfigParameter.systemPrompt),
    },
    ...request.messages,
  ];
  return next();
});

chat.post("/", async (req: ChatRequest, res, next) => {
  try {
    const response = await client.getChatCompletions(
      getConfig(ConfigParameter.azureOpenAIDeployment),
      req.body.messages as ChatRequestMessage[],
    );
    const choice = response.choices[0];
    const responseMessage: AIChatMessage = {
      role: (choice?.message?.role ?? undefined) as AIChatRole,
      content: choice?.message?.content ?? "",
    };

    stateStore.save(req.sessionState, [...req.body.messages, responseMessage]);

    const completion: AIChatCompletion = {
      message: responseMessage,
      sessionState: req.sessionState,
    };
    res.json(completion);
  } catch (error) {
    return next(error);
  }
});

chat.post(
  "/stream",
  async (req: Request<{}, {}, AIChatCompletionRequest>, res, next) => {
    try {
      const response = await client.streamChatCompletions(
        getConfig(ConfigParameter.azureOpenAIDeployment),
        req.body.messages as ChatRequestMessage[],
      );
      res.contentType("application/jsonl");
      const responseMessage: AIChatMessage = {
        role: "assistant",
        content: "",
      };
      for await (const event of response) {
        const choice = event.choices[0];
        if (choice && choice.delta) {
          const delta = choice.delta;
          const completion: AIChatCompletionDelta = {
            delta: {
              content: delta.content ?? undefined,
              role: (delta.role ?? undefined) as AIChatRole,
            },
            sessionState: req.sessionState,
          };
          responseMessage.role = completion.delta.role ?? responseMessage.role;
          responseMessage.content += completion.delta.content ?? "";
          res.write(JSON.stringify(completion) + "\r\n");
        }
      }
      stateStore.save(req.sessionState, [
        ...req.body.messages,
        responseMessage,
      ]);
      res.end();
    } catch (error) {
      return next(error);
    }
  },
);

export default chat;
