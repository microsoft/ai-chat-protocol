import "@typespec/http";

import "./models.tsp";

using TypeSpec.Http;

namespace AI.Chat;

alias Response<T> = T | AIChatErrorResponse;

@post
op getCompletion(...AIChatCompletionRequest): Response<AIChatCompletion>;

@post
@route("/stream")
op getStreamedCompletion(...AIChatCompletionRequest): Response<AIChatCompletionDelta>;
