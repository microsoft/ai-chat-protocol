// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

export interface AIChatCompletionOptions {
  context?: object;
  sessionState?: unknown;
}

export type AIChatCompletionRequest = {
  messages: AIChatMessage[];
} & AIChatCompletionOptions;
