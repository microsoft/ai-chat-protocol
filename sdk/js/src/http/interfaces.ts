export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

export interface HttpRequestBody {
  type: "object" | "string";
  body: object | string;
}

export type HttpHeaders = { [key: string]: string };

export interface HttpRequest {
  method: HttpMethod;
  url: URL;
  headers: { [key: string]: string };
  body: HttpRequestBody;
}

export interface HttpResponse {
  status: number;
  headers: { [key: string]: string };
  body: ReadableStream<Uint8Array>;
}

export interface HttpMiddleware {
  (request: HttpRequest): Promise<HttpRequest>;
}

export interface HttpClient {
  send(request: HttpRequest): Promise<HttpResponse>;
  send(
    request: HttpRequest,
    middleware?: HttpMiddleware,
  ): Promise<HttpResponse>;
}
