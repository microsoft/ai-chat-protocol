// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ClientOptions, OperationOptions } from "@typespec/ts-http-runtime";

export type AIChatRole = "user" | "assistant" | "system";

export interface AIChatFile {
  contentType: string;
  data: Uint8Array | File | Buffer;
}

export interface AIChatMessage<TCtx extends object = object> {
  role: AIChatRole;
  content: string;
  context?: TCtx;
  files?: AIChatFile[];
}

export interface AIChatMessageDelta<TCtx extends object = object> {
  role?: AIChatRole;
  content?: string;
  context?: TCtx;
}

export interface AIChatCompletion<TCtx extends object = object> {
  message: AIChatMessage;
  sessionState?: unknown;
  context?: TCtx;
}

export interface AIChatCompletionDelta<TCtx extends object = object> {
  delta: AIChatMessageDelta;
  sessionState?: unknown;
  context?: TCtx;
}

export interface AIChatCompletionOptions<TCtx extends object = object> {
  context?: TCtx;
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
