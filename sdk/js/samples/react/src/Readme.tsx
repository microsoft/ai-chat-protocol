// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from "react";
import ReactMarkdown from "react-markdown";
import gfm from "remark-gfm";
import { Prism } from "react-syntax-highlighter";
import readmeContent from "../README.md";

const components = {
  code(props: any) {
    const { children, className, node, ...rest } = props;
    const match = /language-(\w+)/.exec(className || "");
    return match ? (
      <Prism
        {...rest}
        PreTag="div"
        children={String(children).replace(/\n$/, "")}
        language={match[1]}
      />
    ) : (
      <code {...rest} className={className}>
        {children}
      </code>
    );
  },
};

function Readme({ style }: { style: React.CSSProperties }) {
  const [markdown, setMarkdown] = useState("");
  useEffect(() => {
    fetch(readmeContent)
      .then((response) => response.text())
      .then((text) => setMarkdown(text));
  }, []);
  return (
    <div style={{ overflowY: "auto", ...style }}>
      <ReactMarkdown components={components} remarkPlugins={[gfm]}>
        {markdown}
      </ReactMarkdown>
    </div>
  );
}

export default Readme;
