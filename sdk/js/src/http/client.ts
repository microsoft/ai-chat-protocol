import * as http from "node:http";
import * as https from "node:https";
import * as zlib from "node:zlib";

import { HttpHeaders, HttpRequest, HttpResponse } from "./interfaces.js";
import { Readable } from "node:stream";

export function _sendRequest(request: HttpRequest): Promise<HttpResponse> {
    const url = new URL(request.url);
    return new Promise<HttpResponse>((resolve, reject) => {
        const options: https.RequestOptions = {
            method: request.method,
            headers: request.headers,
            hostname: url.hostname,
            port: url.port,
            path: url.pathname + url.search + url.hash,
            // agent: request.agent,
            // timeout: request.timeout
        };
        const client = url.protocol === "https" ? https : http;
        const req = client.request(options, (res) => {
            const response: HttpResponse = {
                status: res.statusCode,
                headers: getResponseHeaders(res),
                body: getDecodedResponseStream(res)
            };
            resolve(response);
        });
        req.on('error', reject);
        req.write(request.body);
        req.end();
    });
}

function getDecodedResponseStream(stream: http.IncomingMessage): ReadableStream<Uint8Array> {
    const encoding = stream.headers['content-encoding'];
    const decodedStream = encoding === 'gzip' ? stream.pipe(zlib.createGunzip()) :
        encoding === 'deflate' ? stream.pipe(zlib.createInflate()) :
            stream;
    return Readable.toWeb(decodedStream);
}

function getResponseHeaders(res: http.IncomingMessage): HttpHeaders {
    // const headers = new Map<string, string>();
    const headers: HttpHeaders = {};
    for (const header of Object.keys(res.headers)) {
      const value = res.headers[header];
      if (Array.isArray(value)) {
        if (value.length > 0) {
          headers[header] = value[0];
        }
      } else if (value) {
        headers[header] = value;
      }
    }
    return headers;
  }

_sendRequest({ method: "POST", headers: { "Content-Type": "application/json" }, url: "http://test-server:8080/chat", body: "{\"messages\": [{\"role\":\"system\",\"content\":\"You are an AI assistant that helps people find information.\"},{\"role\":\"user\",\"content\":\"Give me ten reasons to regularly exercise.\"}],\"stream\": false}"})
    .then(response => {
      console.log(`Status: ${response.status}`)
      console.log(`Headers: ${JSON.stringify(response.headers)}`)
      let b = (response.body as ReadableStream<Uint8Array>);
        let reader = b.getReader();
        // let exit = false;
        reader.read().then(({ value, done }): void => {
          let text = new TextDecoder().decode(value);
              console.log(text);
              if (done) {
                // exit = true;
              }
          });

    });
