// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export class StateStore<T> {
  private store: Record<string, T>;

  constructor() {
    this.store = {};
  }

  public read(key: string): T {
    const state = this.store[key];
    if (!state) {
      throw new Error("Not found.");
    }
    return this.store[key];
  }

  public async save(key: string, state: T) {
    this.store[key] = state;
  }
}
