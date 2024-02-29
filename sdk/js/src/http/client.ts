import { HttpResponse } from "./interfaces.js";
import { _sendRequest } from "./send.js";

export class HttpClient {
  get(url: URL, headers: { [key: string]: string }): Promise<HttpResponse> {
    return _sendRequest({ method: "GET", url, headers, body: null });
  }

  post(
    url: URL,
    headers: { [key: string]: string },
    body: object,
  ): Promise<HttpResponse> {
    return _sendRequest({
      method: "POST",
      url,
      headers,
      body: { type: "object", body },
    });
  }
}
