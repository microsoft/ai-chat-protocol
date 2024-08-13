// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  DefaultAzureCredential,
  getBearerTokenProvider,
} from "@azure/identity";
import express, { Router, Request } from "express";
import { v4 as uuid } from "uuid";
import formidable from "formidable";
import _ from "lodash";
import fs from "fs";

import { ConfigParameter, getConfig } from "../config";
import {
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionRequest,
  AIChatMessage,
  AIChatRole,
} from "@microsoft/ai-chat-protocol";
import { StateStore } from "../state-store";
import { AzureOpenAI } from "openai";
import {
  ChatCompletionContentPartImage,
  ChatCompletionMessageParam,
} from "openai/resources";

declare global {
  namespace Express {
    interface Request {
      sessionState: string;
    }
  }
}

const chat = Router();

const client = new AzureOpenAI({
  apiVersion: "2024-05-01-preview",
  endpoint: getConfig(ConfigParameter.azureOpenAIEndpoint),
  azureADTokenProvider: getBearerTokenProvider(
    new DefaultAzureCredential(),
    "https://cognitiveservices.azure.com/.default",
  ),
});

const stateStore = new StateStore<AIChatMessage[]>();

type UnknownRequest = Request<{}, {}, unknown>;
type ChatRequest = Request<{}, {}, AIChatCompletionRequest>;

async function readFile(filepath: string): Promise<Buffer> {
  const data = await fs.promises.readFile(filepath);
  await fs.promises.unlink(filepath);
  return data;
}

async function readJson(filepath: string): Promise<ChatRequest> {
  const buffer = await readFile(filepath);
  const data = buffer.toString("utf-8");
  return JSON.parse(data);
}

const jsonMiddleware = express.json();

chat.use(async (req: UnknownRequest, res, next) => {
  if (req.is("multipart/form-data")) {
    try {
      const form = formidable();
      const [, files] = await form.parse(req);

      const [jsonFile] = files.json as formidable.File[];
      const json = await readJson(jsonFile.filepath);

      for (let key of Object.keys(files)) {
        if (key === "json") {
          continue;
        }
        const [file] = files[key] as formidable.File[];
        const data = await readFile(file.filepath);
        _.set(json, `${key}.data`, data);
        _.set(json, `${key}.contentType`, file.mimetype);
      }
      req.body = json;
      req.headers["content-type"] = "application/json";

      return next();
    } catch (error) {
      return next(error);
    }
  } else if (req.is("application/json")) {
    return jsonMiddleware(req, res, next);
  }

  return next();
});

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

function toOpenAIMessage(message: AIChatMessage): ChatCompletionMessageParam {
  if (message.files && message.files.length > 0 && message.role === "user") {
    const fileContent: ChatCompletionContentPartImage[] = message.files.map(
      (file) => ({
        type: "image_url",
        image_url: {
          url: `data:${file.contentType};base64,${file.data.toString("base64")}`,
        },
      }),
    );
    return {
      role: message.role,
      content: [
        ...fileContent,
        {
          type: "text",
          text: message.content,
        },
      ],
    };
  }
  return {
    role: message.role,
    content: message.content,
  };
}

chat.post("/", async (req: ChatRequest, res, next) => {
  try {
    const response = await client.chat.completions.create({
      model: getConfig(ConfigParameter.azureOpenAIDeployment),
      messages: req.body.messages.map(toOpenAIMessage),
    });

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
      const response = await client.chat.completions.create({
        stream: true,
        model: getConfig(ConfigParameter.azureOpenAIDeployment),
        messages: req.body.messages.map(toOpenAIMessage),
      });
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
