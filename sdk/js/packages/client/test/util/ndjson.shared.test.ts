// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { expect, test } from "vitest";
import { getAsyncIterable } from "../../src/util/ndjson.js";

function getReadableStream(lines: string[]) {
  return new ReadableStream({
    start(controller) {
      lines.forEach((line) => {
        controller.enqueue(new TextEncoder().encode(line));
      });
      controller.close();
    },
  });
}

function getReadableStreamFromObjects(objects: object[]) {
  return getReadableStream(objects.map((o) => JSON.stringify(o) + "\n"));
}

interface Foo {
  a: number;
}

test("Can parse file delimited with '\\n'", async () => {
  const stream = getReadableStream(['{"a":1}\n{', '"a":2}\n{"', 'a":3}\n']);
  const iterable = await getAsyncIterable<Foo>(stream);

  let values: Foo[] = [];
  for await (const value of iterable) {
    values.push(value);
  }
  expect(values).toEqual([{ a: 1 }, { a: 2 }, { a: 3 }]);
});

test("Can parse file delimited with '\\r\\n'", async () => {
  const stream = getReadableStream([
    '{"a":1}\r\n{',
    '"a":2}\r\n{"',
    'a":3}\r\n',
  ]);
  const iterable = await getAsyncIterable<Foo>(stream);

  let values: Foo[] = [];
  for await (const value of iterable) {
    values.push(value);
  }
  expect(values).toEqual([{ a: 1 }, { a: 2 }, { a: 3 }]);
});

test("Can parse file delimited with mixed '\\n' and '\\r\\n'", async () => {
  const stream = getReadableStream(['{"a":1}\n{', '"a":2}\r\n{"', 'a":3}\n']);
  const iterable = await getAsyncIterable<Foo>(stream);

  let values: Foo[] = [];
  for await (const value of iterable) {
    values.push(value);
  }
  expect(values).toEqual([{ a: 1 }, { a: 2 }, { a: 3 }]);
});

test("Can parse file with empty lines", async () => {
  const stream = getReadableStream(['{"a":1}\n\n{"a":2}\n\n{"a":3}\n']);
  const iterable = await getAsyncIterable<Foo>(stream);

  let values: Foo[] = [];
  for await (const value of iterable) {
    values.push(value);
  }
  expect(values).toEqual([{ a: 1 }, { a: 2 }, { a: 3 }]);
});

test("When received a malformed JSON, it throws an error", async () => {
  const stream = getReadableStream(['{"a":1}\n{', '"a":2}\n{"', 'a":3\n']);
  const iterable = await getAsyncIterable<Foo>(stream);

  let values: Foo[] = [];
  try {
    for await (const value of iterable) {
      values.push(value);
    }
    expect(false).toBe(true);
  } catch (e) {
    expect(e).toBeInstanceOf(SyntaxError);
  }
});

test("Throws an exception when the stream contains an AIChatErrorResponse", async () => {
  const stream = getReadableStreamFromObjects([
    { error: { code: "Some code", message: "Some error" } },
  ]);
  const iterable = getAsyncIterable(stream);

  try {
    for await (const _ of iterable) {
      expect.unreachable("Should have thrown an error");
    }
    expect.unreachable("Should have thrown an error");
  } catch (err) {
    expect(err).toEqual({ code: "Some code", message: "Some error" });
  }
});

test("Throws an exception as soon as an AIChatError is encountered", async () => {
  const stream = getReadableStreamFromObjects([
    { foo: "bar" },
    { error: { code: "Some code", message: "Some error" } },
    { foo: "baz" },
  ]);
  const iterable = getAsyncIterable(stream);

  let values: unknown[] = [];
  try {
    for await (const value of iterable) {
      values.push(value);
    }
    expect.unreachable("Should have thrown an error");
  } catch (err) {
    expect(err).toEqual({ code: "Some code", message: "Some error" });
  }
  expect(values).toEqual([{ foo: "bar" }]);
});
