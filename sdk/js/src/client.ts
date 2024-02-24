import {AIChatMessage, AIChatCompletion, AIChatCompletionDelta, AIChatEventStream} from "./models/index.js";

export class AIChatProtocolClient {
    constructor(endpoint: string) {
        endpoint;
        throw new Error("Not implemented");
    }

    getCompletion(messages: AIChatMessage[]): Promise<AIChatCompletion> {
        messages;
        throw new Error("Not implemented");
    }

    getStreamedCompletion(messages: AIChatMessage[]): AIChatEventStream<AIChatCompletionDelta> {
        messages;
        throw new Error("Not implemented");
    }

}
