import "@typespec/http";

import "./models.tsp";

using TypeSpec.Http;

namespace AI.Chat;

@post
op getCompletion(...AIChatCompletionRequest): AIChatCompletion;

@post
@route("/stream")
op getStreamedCompletion(...AIChatCompletionRequest): AIChatCompletionDelta;
