// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { isErrorResponse } from "./error.js";

function makeAsyncIterable<T>(stream: ReadableStream<T>): AsyncIterable<T> {
  return {
    async *[Symbol.asyncIterator]() {
      const reader = stream.getReader();
      try {
        while (true) {
          const { value, done } = await reader.read();
          if (done) {
            return;
          }
          yield value;
        }
      } finally {
        const cancel = reader.cancel();
        reader.releaseLock();
        await cancel;
      }
    },
  };
}

async function* toLines(
  stream: ReadableStream<Uint8Array>,
): AsyncIterable<string> {
  enum ControlChar {
    LF = 10,
    CR = 13,
  }
  let buf: Uint8Array;
  let bufIndex = 0;
  const chunkIter = makeAsyncIterable(stream);
  for await (const chunk of chunkIter) {
    if (buf === undefined) {
      buf = chunk;
      bufIndex = 0;
    } else {
      buf = Uint8Array.from([...buf, ...chunk]);
    }
    const bufLen = buf.length;
    let start = 0;
    while (bufIndex < bufLen) {
      while (bufIndex < bufLen && buf[bufIndex] !== ControlChar.LF) {
        bufIndex++;
      }
      if (bufIndex === bufLen) {
        break;
      }
      if (bufIndex > 0 && buf[bufIndex - 1] === ControlChar.CR) {
        if (start < bufIndex - 1) {
          yield new TextDecoder().decode(buf.slice(start, bufIndex - 1));
        }
        start = bufIndex + 1;
      } else {
        if (start < bufIndex) {
          yield new TextDecoder().decode(buf.slice(start, bufIndex));
        }
        start = bufIndex + 1;
      }
      bufIndex++;
    }
    if (start < bufLen) {
      buf = buf.slice(start);
      bufIndex = buf.length;
    } else {
      buf = undefined;
    }
  }
}

async function* map<T, U>(
  iter: AsyncIterable<T>,
  fn: (value: T) => U,
): AsyncIterable<U> {
  for await (const value of iter) {
    yield fn(value);
  }
}

export function getAsyncIterable<T>(
  stream: ReadableStream<Uint8Array>,
): AsyncIterable<T> {
  return map(toLines(stream), (line) => {
    const parsed = JSON.parse(line);
    if (isErrorResponse(parsed)) {
      throw parsed.error;
    }
    return parsed as T;
  });
}
