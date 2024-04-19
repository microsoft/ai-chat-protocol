// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

function isAbsoluteUrl(url: string): boolean {
  let parsed: URL;
  try {
    parsed = new URL(url);
  } catch (e) {
    return false;
  }
  return parsed.protocol === 'http:' || parsed.protocol === 'https:';
}

export function toAbsoluteUrl(url: string): string {
  if (isAbsoluteUrl(url)) {
    return url;
  }
  const base = window.location.origin;
  const basePath = window.location.pathname;
  return new URL(url, `${base}${basePath}`).href;
}
