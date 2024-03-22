// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ClientOptions, OperationOptions } from "@typespec/ts-http-runtime";

export type AIChatRole = "user" | "assistant" | "system";

export type AIChatFinishReason = "stop" | "length";

export interface AIChatMessage {
  role: AIChatRole;
  content: string;
  sessionState?: unknown;
}

export interface AIChatCompletion {
  message: AIChatMessage;
  sessionState?: unknown;
  context?: object;
  finishReason: AIChatFinishReason;
}

export interface AIChatCompletionDelta {
  delta: AIChatMessage;
  sessionState?: unknown;
  context?: object;
  finishReason: AIChatFinishReason;
}

export interface AIChatClientOptions extends ClientOptions {}

export interface AIChatCompletionOptions extends OperationOptions {
  context?: object;
  sessionState?: unknown;
}
