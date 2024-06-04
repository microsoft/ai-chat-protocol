// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ClientOptions, OperationOptions } from "@typespec/ts-http-runtime";

export type AIChatRole = "user" | "assistant" | "system";

export interface AIChatMessage {
  role: AIChatRole;
  content: string;
  context?: object;
}

export interface AIChatMessageDelta {
  role?: AIChatRole;
  content?: string;
  context?: object;
}

export interface AIChatCompletion {
  message: AIChatMessage;
  sessionState?: unknown;
  context?: object;
}

export interface AIChatCompletionDelta {
  delta: AIChatMessageDelta;
  sessionState?: unknown;
  context?: object;
}

export interface AIChatCompletionOptions {
  context?: object;
  sessionState?: unknown;
}

export type AIChatCompletionRequest = {
  messages: AIChatMessage[];
} & AIChatCompletionOptions;

export interface AIChatClientOptions extends ClientOptions {}

export type AIChatCompletionOperationOptions = AIChatCompletionOptions &
  OperationOptions;

export interface AIChatError {
  code: string;
  message: string;
}

export interface AIChatErrorResponse {
  error: AIChatError;
}
