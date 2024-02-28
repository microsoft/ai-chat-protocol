import { HttpRequest, HttpResponse } from "./interfaces.js";

export async function _sendRequest(
  request: HttpRequest,
): Promise<HttpResponse> {
  const _request = new Request(request.url, {
    method: request.method,
    headers: request.headers,
    body: request.body,
  });
  const response = await fetch(_request);
  return {
    status: response.status,
    headers: getResponseHeaders(response),
    body: response.body,
  };
}

function getResponseHeaders(response: Response): { [key: string]: string } {
  const headers: { [key: string]: string } = {};
  response.headers.forEach((value, key) => {
    headers[key] = value;
  });
  return headers;
}
