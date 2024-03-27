import { createClient, RedisClientType } from "redis";

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

  public async save(key: string, state: T): Promise<void> {
    await this.client.set(key, JSON.stringify(state));
    await this.client.expire(
      key,
      parseInt(getConfig(ConfigParameter.stateTTL)),
    );
  }
}
