import { HttpMiddleware, HttpRequest, HttpResponse } from "./interfaces.js";
import { _sendRequest } from "./send.js";

export class HttpClient {
  send(request: HttpRequest): Promise<HttpResponse>;
  send(request: HttpRequest, middleware: HttpMiddleware): Promise<HttpResponse>;

  async send(
    request: HttpRequest,
    middleware?: HttpMiddleware,
  ): Promise<HttpResponse> {
    const actualRequest = middleware ? await middleware(request) : request;
    return _sendRequest(actualRequest);
  }
}
