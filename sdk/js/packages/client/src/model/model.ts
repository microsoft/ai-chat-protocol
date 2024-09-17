// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ClientOptions, OperationOptions } from "@typespec/ts-http-runtime";

export type AIChatRole = "user" | "assistant" | "system";

type GenericContext = Record<string, unknown>;

export interface AIChatFile {
  contentType: string;
  data: Uint8Array | File | Buffer;
}

export interface AIChatMessage<ContextType extends GenericContext = GenericContext> {
  role: AIChatRole;
  content: string;
  context?: ContextType;
  files?: AIChatFile[];
}

export interface AIChatMessageDelta<ContextType extends GenericContext = GenericContext> {
  role?: AIChatRole;
  content?: string;
  context?: ContextType;
}

export interface AIChatCompletion<ContextType extends GenericContext = GenericContext> {
  message: AIChatMessage;
  sessionState?: unknown;
  context?: ContextType;
}

export interface AIChatCompletionDelta<ContextType extends GenericContext = GenericContext> {
  delta: AIChatMessageDelta;
  sessionState?: unknown;
  context?: ContextType;
}

export interface AIChatCompletionOptions<ContextType extends GenericContext = GenericContext> {
  context?: ContextType;
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
