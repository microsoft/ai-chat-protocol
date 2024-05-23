// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Button, ToggleButton } from "@fluentui/react-components";
import { AIChatMessage, AIChatProtocolClient } from "@microsoft/ai-chat-protocol";
import { useId, useState } from "react";
import ReactMarkdown from "react-markdown";
import TextareaAutosize from 'react-textarea-autosize';
import styles from './Chat.module.css';


export default function Chat({ style }: { style: React.CSSProperties }) {
  const client = new AIChatProtocolClient('http://localhost:3000/api/chat', {
    allowInsecureConnection: true
  });

  const [messages, setMessages] = useState<AIChatMessage[]>([]);
  const [input, setInput] = useState<string>('');
  const [streaming, setStreaming] = useState<boolean>(false);
  const inputId = useId();
  const [sessionState, setSessionState] = useState<unknown>(undefined);

  const clear = () => {
    setMessages([]);
    setInput('');
  };

  const sendMessage = async () => {
    const message: AIChatMessage = {
      role: 'user',
      content: input
    };
    const updatedMessages = [...messages, message];
    setMessages(updatedMessages);
    setInput('');
    if (streaming) {
      const result = await client.getStreamedCompletion([message], { sessionState: sessionState });
      const latestMessage: AIChatMessage = { content: "", role: 'assistant' };
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
      const result = await client.getCompletion([message], { sessionState: sessionState });
      if (result.sessionState) {
        setSessionState(result.sessionState);
      }
      setMessages([...updatedMessages, result.message]);
    }
  };

  return (
    <div className={styles.chatWindow} style={style}>
      <div className={styles.messages}>
        {messages.map(message => (
          <div key={crypto.randomUUID()} className={message.role === 'user' ? styles.userMessage : styles.assistantMessage}>
            <div className={styles.messageBubble}>
              <ReactMarkdown>{message.content}</ReactMarkdown>
            </div>
          </div>
        ))}
      </div>
      <div className={styles.inputArea}>
        <TextareaAutosize
          id={inputId}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          minRows={1}
          maxRows={4}
        />
        <Button onClick={sendMessage}>Send</Button>
        <ToggleButton checked={streaming} onClick={() => setStreaming(!streaming)} >Streaming</ToggleButton>
      </div>
    </div>
      );
}
