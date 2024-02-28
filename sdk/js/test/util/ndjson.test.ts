import { expect, test, vi } from "vitest";
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
