export type AIChatRole = "user" | "assistant" | "system";

export type AIChatFinishReason = "stop" | "length";

export interface AIChatMessage {
  role: AIChatRole;
  content: string;
  sessionState?: any;
}

export interface AIChatChoice {
  index: number;
  message: AIChatMessage;
  sessionState: any;
  context: any;
  finishReason: AIChatFinishReason;
}

export interface AIChatCompletion {
  choices: AIChatChoice[];
}

export interface AIChatChoiceDelta {
  index: number;
  delta: AIChatMessage;
  sessionState: any;
  context: any;
  finishReason: AIChatFinishReason;
}

export interface AIChatCompletionDelta {
  choices: AIChatChoiceDelta[];
}
