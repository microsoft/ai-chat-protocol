import "@typespec/http";

import "./models.tsp";

using TypeSpec.Http;

namespace AI.Chat;

alias Response<T> = T | AIChatErrorResponse;

op getCompletion(@header contentType: "application/json" | "multipart/form-data", body: AIChatCompletionRequest | AIChatCompletionRequestMultipart): Response<AIChatCompletion>;

@overload(getCompletion)
@post
op getCompletionJSON(@header contentType: "application/json", @body body: AIChatCompletionRequest): Response<AIChatCompletion>;

@overload(getCompletion)
@post
op getCompletionMultipart(@header contentType: "multipart/form-data", @multipartBody body: AIChatCompletionRequestMultipart): Response<AIChatCompletion>;

@post
@route("/stream")
op getStreamedCompletion(@header contentType: "application/json" | "multipart/form-data", body: AIChatCompletionRequest | AIChatCompletionRequestMultipart): Response<AIChatCompletionDelta>;

@overload(getStreamedCompletion)
@post
@route("/stream")
op getStreamedCompletionJSON(@header contentType: "application/json", @body body: AIChatCompletionRequest): Response<AIChatCompletionDelta>;

@overload(getStreamedCompletion)
@post
@route("/stream")
op getStreamedCompletionMultipart(@header contentType: "multipart/form-data", @multipartBody body: AIChatCompletionRequestMultipart): Response<AIChatCompletionDelta>;
