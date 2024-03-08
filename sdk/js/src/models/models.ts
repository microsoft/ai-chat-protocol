// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { OperationOptions } from "@typespec/ts-http-runtime";

export type AIChatRole = "user" | "assistant" | "system";

export type AIChatFinishReason = "stop" | "length";

export interface AIChatMessage {
  role: AIChatRole;
  content: string;
  sessionState?: unknown;
}

export interface AIChatChoice {
  index: number;
  message: AIChatMessage;
  sessionState?: unknown;
  context?: object;
  finishReason: AIChatFinishReason;
}

export interface AIChatCompletion {
  choices: AIChatChoice[];
}

export interface AIChatChoiceDelta {
  index: number;
  delta: AIChatMessage;
  sessionState?: unknown;
  context?: object;
  finishReason: AIChatFinishReason;
}

export interface AIChatCompletionDelta {
  choices: AIChatChoiceDelta[];
}

export interface AIChatCompletionOptions extends OperationOptions {
  context?: object;
  sessionState?: unknown;
}
