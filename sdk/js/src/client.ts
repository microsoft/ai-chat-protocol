import { HttpClient } from "./http/client.js";
import {AIChatMessage, AIChatCompletion, AIChatCompletionDelta} from "./models/index.js";

class StreamedCompletionTransform extends TransformStream<Uint8Array, AIChatCompletionDelta> {
    constructor() {
        super({
            transform(chunk, controller) {
                const serialized = new TextDecoder().decode(chunk);
                const completion = JSON.parse(serialized) as AIChatCompletionDelta;
                controller.enqueue(completion);
            }
        });
    }

}

export class AIChatProtocolClient {
    private endpoint: string;
    private httpClient: HttpClient;
    constructor(endpoint: string) {
        this.endpoint = endpoint;
        this.httpClient = new HttpClient();
    }

    async getCompletion(messages: AIChatMessage[]): Promise<AIChatCompletion> {
        const response = await this.httpClient.post(this.endpoint, {
            "Content-Type": "application/json"
        }, {
            messages: messages,
            stream: false
        });
        if (response.status !== 200) {
            throw new Error(`Request failed with status code ${response.status}`);
        }
        const reader = response.body.getReader();
        const payload = await reader.read();
        const serialized = new TextDecoder().decode(payload.value);
        return JSON.parse(serialized) as AIChatCompletion;
    }

    async getStreamedCompletion(messages: AIChatMessage[]): Promise<ReadableStream<AIChatCompletionDelta>> {
        const response = await this.httpClient.post(this.endpoint, {
            "Content-Type": "application/json"
        }, {
            messages: messages,
            stream: true
        });
        if (response.status !== 200) {
            throw new Error(`Request failed with status code ${response.status}`);
        }
        return response.body.pipeThrough(new StreamedCompletionTransform());
    }

}
