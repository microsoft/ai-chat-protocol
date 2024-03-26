import { createClient, RedisClientType } from "redis";
import { v4 as uuid } from "uuid";

import { ConfigParameter, getConfig } from "./config";

export class StateStore<T> {
  private client: RedisClientType;

  constructor() {
    this.client = createClient({
      url: getConfig(ConfigParameter.redisUrl),
    });
  }

  public async connect(): Promise<void> {
    await this.client.connect();
  }

  public async disconnect(): Promise<void> {
    await this.client.disconnect();
  }

  public async read(key: string): Promise<T> {
    const state = await this.client.get(key);
    if (!state) {
      throw new Error("Not found.");
    }
    return JSON.parse(state);
  }

  public async save(key: string | undefined, state: T): Promise<string> {
    if (!key) {
      key = uuid();
    }
    await this.client.set(key, JSON.stringify(state));
    return key;
  }
}
