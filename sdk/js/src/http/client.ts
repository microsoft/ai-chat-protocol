import * as http from "node:http";
import * as https from "node:https";
import * as zlib from "node:zlib";

import { HttpClient, HttpRequest, HttpResponse } from "./interfaces";
import { Readable } from "node:stream";

export _sendRequest(request: HttpRequest): Promise<HttpResponse> {

}

function getDecodedResponseStream(stream: http.IncomingMessage): ReadableStream<Uint8Array> {
    const encoding = stream.headers['content-encoding'];
    const decodedStream = encoding === 'gzip' ? stream.pipe(zlib.createGunzip()) :
        encoding === 'deflate' ? stream.pipe(zlib.createInflate()) :
            stream;
    return Readable.toWeb(decodedStream);
}
