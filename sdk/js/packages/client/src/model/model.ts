// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { AIChatCompletionOptions } from "@microsoft/ai-chat-protocol-model";
import { ClientOptions, OperationOptions } from "@typespec/ts-http-runtime";

export interface AIChatClientOptions extends ClientOptions {}

export type AIChatCompletionOperationOptions = AIChatCompletionOptions &
  OperationOptions;
