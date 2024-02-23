
export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE';

export type RequestBody = string | FormData | Blob | ArrayBuffer | ArrayBufferView | URLSearchParams | ReadableStream<Uint8Array> | null;

export type HttpHeaders = { [key: string]: string };

export interface HttpRequest {
    method: HttpMethod;
    url: string;
    headers: { [key: string]: string };
    body: RequestBody;
}

export interface HttpResponse {
    status: number;
    headers: { [key: string]: string };
    body: string | ReadableStream<Uint8Array> | null;
}

export interface HttpClient {
    send(request: HttpRequest): Promise<HttpResponse>;
}
