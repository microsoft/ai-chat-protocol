import { HttpMiddleware, HttpRequest, HttpResponse } from "./interfaces.js";
import { _sendRequest } from "./send.js";

export class HttpClient {
  async send(
    request: HttpRequest,
    abortSignal?: AbortSignal,
    middleware?: HttpMiddleware,
  ): Promise<HttpResponse> {
    const actualRequest = middleware ? await middleware(request) : request;
    return _sendRequest(actualRequest, abortSignal);
  }
}
