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

type ChatEntry = (AIChatMessage & { dataUrl?: string }) | AIChatError;

function isChatError(entry: unknown): entry is AIChatError {
  return (entry as AIChatError).code !== undefined;
}

interface FileInput {
  data: Uint8Array;
  name: string;
  type: string;
}

function toBase64DataUrl(
  arr?: Uint8Array,
  contentType?: string,
): Promise<string | undefined> {
  return new Promise<string | undefined>((resolve, reject) => {
    if (!arr) {
      resolve(undefined);
      return;
    }
    const blob = new Blob([arr], { type: contentType });
    const reader = new FileReader();

    reader.onerror = reject;
    reader.onload = (event) => {
      resolve(event.target?.result as string);
    };
    reader.readAsDataURL(blob);
  });
}

export default function Chat({ style }: { style: React.CSSProperties }) {
  const client = new AIChatProtocolClient("/api/chat/");

  const [messages, setMessages] = useState<ChatEntry[]>([]);
  const [input, setInput] = useState<string>("");
  const [streaming, setStreaming] = useState<boolean>(false);
  const inputId = useId();
  const [sessionState, setSessionState] = useState<unknown>(undefined);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const [selectedFile, setSelectedFile] = useState<FileInput | undefined>(
    undefined,
  );
  const fileInputRef = useRef<HTMLInputElement>(null);

  function isArrayBuffer(value: unknown): value is ArrayBuffer {
    return value instanceof ArrayBuffer;
  }

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target?.files && e.target.files.length > 0) {
      const file = e.target.files[0];
      const reader = new FileReader();

      reader.onload = (loadEvent) => {
        const arrayBuffer = loadEvent!.target!.result;
        if (!isArrayBuffer(arrayBuffer)) {
          setSelectedFile(undefined);
          return;
        }
        const fileUint8Array = new Uint8Array(arrayBuffer);
        setSelectedFile({
          data: fileUint8Array,
          name: file.name,
          type: file.type,
        });
      };

      reader.readAsArrayBuffer(file);
    }
  };

  const clearSelectedFile = () => {
    setSelectedFile(undefined);
    if (fileInputRef.current) {
      fileInputRef.current.value = ""; // Reset file input
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };
  useEffect(scrollToBottom, [messages]);

  const sendMessage = async () => {
    const message: AIChatMessage = {
      role: "user",
      content: input,
      files: selectedFile
        ? [
            {
              data: selectedFile.data,
              contentType: selectedFile.type,
            },
          ]
        : undefined,
    };
    const dataUrl = await toBase64DataUrl(
      selectedFile?.data,
      selectedFile?.type,
    );
    const updatedMessages: ChatEntry[] = [
      ...messages,
      {
        ...message,
        files: undefined,
        dataUrl: dataUrl,
      },
    ];
    setMessages(updatedMessages);
    setInput("");
    setSelectedFile(undefined);
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
              <>
                <div className={styles.messageBubble}>
                  <ReactMarkdown remarkPlugins={[gfm]}>
                    {message.content}
                  </ReactMarkdown>
                  {message.dataUrl && (
                    <img
                      src={message.dataUrl}
                      style={{
                        maxHeight: "300px",
                        maxWidth: "100%",
                        height: "auto",
                        width: "auto",
                      }}
                      alt="Message Attachment"
                    />
                  )}
                </div>
              </>
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
        {selectedFile && (
          <div>
            <span>File: {selectedFile.name}</span>
            <button onClick={clearSelectedFile}>Clear</button>
          </div>
        )}
        <input
          type="file"
          accept="image/*"
          style={{ display: "none" }}
          ref={fileInputRef} // Create this ref using useRef in your component
          onChange={handleFileChange} // Implement this function to handle file selection
        />
        <Button onClick={() => fileInputRef?.current?.click()}>Attach</Button>
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
