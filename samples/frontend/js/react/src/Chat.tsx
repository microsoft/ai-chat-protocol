// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Button, ToggleButton } from "@fluentui/react-components";
import {
  AIChatMessage,
  AIChatProtocolClient,
  AIChatError,
} from "@microsoft/ai-chat-protocol";
import { useEffect, useId, useRef, useState } from "react";
import ReactMarkdown from "react-markdown";
import TextareaAutosize from "react-textarea-autosize";
import styles from "./Chat.module.css";
import gfm from "remark-gfm";

type ChatEntry = AIChatMessage | AIChatError;

function isChatError(entry: unknown): entry is AIChatError {
  return (entry as AIChatError).code !== undefined;
}

export default function Chat({ style }: { style: React.CSSProperties }) {
  const client = new AIChatProtocolClient("/api/chat/");

  const [messages, setMessages] = useState<ChatEntry[]>([]);
  const [input, setInput] = useState<string>("");
  const [streaming, setStreaming] = useState<boolean>(false);
  const inputId = useId();
  const [sessionState, setSessionState] = useState<unknown>(undefined);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };
  useEffect(scrollToBottom, [messages]);

  const sendMessage = async () => {
    const message: AIChatMessage = {
      role: "user",
      content: input,
    };
    const updatedMessages = [...messages, message];
    setMessages(updatedMessages);
    setInput("");
    try {
      if (streaming) {
        const result = await client.getStreamedCompletion([message], {
          sessionState: sessionState,
        });
        const latestMessage: AIChatMessage = { content: "", role: "assistant" };
        for await (const response of result) {
          if (response.sessionState) {
            setSessionState(response.sessionState);
          }
          if (!response.delta) {
            continue;
          }
          if (response.delta.role) {
            latestMessage.role = response.delta.role;
          }
          if (response.delta.content) {
            latestMessage.content += response.delta.content;
            setMessages([...updatedMessages, latestMessage]);
          }
        }
      } else {
        const result = await client.getCompletion([message], {
          sessionState: sessionState,
        });
        setSessionState(result.sessionState);
        setMessages([...updatedMessages, result.message]);
      }
    } catch (e) {
      if (isChatError(e)) {
        setMessages([...updatedMessages, e]);
      }
    }
  };

  const getClassName = (message: ChatEntry) => {
    if (isChatError(message)) {
      return styles.caution;
    }
    return message.role === "user"
      ? styles.userMessage
      : styles.assistantMessage;
  };

  const getErrorMessage = (message: AIChatError) => {
    return `${message.code}: ${message.message}`;
  };

  return (
    <div className={styles.chatWindow} style={style}>
      <div className={styles.messages}>
        {messages.map((message) => (
          <div key={crypto.randomUUID()} className={getClassName(message)}>
            {isChatError(message) ? (
              <>{getErrorMessage(message)}</>
            ) : (
              <div className={styles.messageBubble}>
                <ReactMarkdown remarkPlugins={[gfm]}>
                  {message.content}
                </ReactMarkdown>
              </div>
            )}
          </div>
        ))}
        <div ref={messagesEndRef} />
      </div>
      <div className={styles.inputArea}>
        <TextareaAutosize
          id={inputId}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Enter" && e.shiftKey) {
              e.preventDefault();
              sendMessage();
            }
          }}
          minRows={1}
          maxRows={4}
        />
        <Button onClick={sendMessage}>Send</Button>
        <ToggleButton
          checked={streaming}
          onClick={() => setStreaming(!streaming)}
        >
          Streaming
        </ToggleButton>
      </div>
    </div>
  );
}
