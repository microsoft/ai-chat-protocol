// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

export interface HttpRequestBody {
  type: "object" | "string";
  body: object | string;
}

export type HttpHeaders = { [key: string]: string };

export interface HttpRequest {
  method: HttpMethod;
  url: URL;
  headers: HttpHeaders;
  body: HttpRequestBody;
}

export interface HttpResponse {
  status: number;
  headers: HttpHeaders;
  body: ReadableStream<Uint8Array>;
}

export interface HttpMiddleware {
  (request: HttpRequest): Promise<HttpRequest>;
}
