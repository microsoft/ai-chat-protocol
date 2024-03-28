import "@typespec/http";

using TypeSpec.Http;

model ContextProp {
  context?: Record<unknown>;
}

model SessionStateProp {
  sessionState?: unknown;
}

enum AIChatRole {
  assistant,
  user,
  system,
}

enum AIChatFinishReason {
  stop,
  length,
}

model AIChatMessage {
  role: AIChatRole;
  content: string;
  ...ContextProp;
}

model AIChatMessageDelta {
  role?: AIChatRole;
  content?: string;
  ...ContextProp;
}

model AIChatCompletion {
  message: AIChatMessage;
  finishReason: AIChatFinishReason;
  ...ContextProp;
  ...SessionStateProp;
}

model AIChatCompletionDelta {
  @header
  contentType: "application/x-ndjson";

  delta: AIChatMessageDelta;
  finisReason?: AIChatFinishReason;
  ...ContextProp;
  ...SessionStateProp;
}

model AIChatCompletionRequest {
  messages: AIChatMessage[];
  ...ContextProp;
  ...SessionStateProp;
}
