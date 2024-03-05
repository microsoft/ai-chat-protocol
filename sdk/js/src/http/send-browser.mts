import { HttpRequest, HttpRequestBody, HttpResponse } from "./interfaces.js";

export async function _sendRequest(
  request: HttpRequest,
  abortSignal?: AbortSignal,
): Promise<HttpResponse> {
  const _request = new Request(request.url, {
    method: request.method,
    headers: request.headers,
    body: toRequestBody(request.body),
    signal: abortSignal,
  });
  const response = await fetch(_request);
  return {
    status: response.status,
    headers: getResponseHeaders(response),
    body: response.body,
  };
}

function toRequestBody(body: HttpRequestBody): BodyInit {
  if (body.type === "object") {
    return JSON.stringify(body.body);
  }
  if (body.type === "string") {
    return body.body as string;
  }
  throw new Error(`Invalid body type: ${body.type}`);
}

function getResponseHeaders(response: Response): { [key: string]: string } {
  const headers: { [key: string]: string } = {};
  response.headers.forEach((value, key) => {
    headers[key] = value;
  });
  return headers;
}
