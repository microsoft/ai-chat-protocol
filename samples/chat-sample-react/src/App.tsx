import React, { FormEvent, useId, useState } from 'react';
import logo from './logo.svg';
// import './App.css';
import { AIChatProtocolClient, AIChatMessage } from '@microsoft/ai-chat-protocol';
import { JSX } from 'react/jsx-runtime';
import { Button, FluentProvider, Input, Text, webLightTheme } from '@fluentui/react-components';




function History(messages: AIChatMessage[]) {
  const messageList = messages.map((message) => {
    return (
      <li>
        Role: {message.role}, Message: {message.content}
      </li>
    );
  });
  return (
    <ul>
      {messageList}
    </ul>
  );
}

function Chat() {
  const client = new AIChatProtocolClient('http://test-server:8080/chat', {allowInsecureConnection: true});
  const history: AIChatMessage[] = [
    { role: 'system', content: 'You are an enthusiastic assistant that talks like a 5 year old.' },

  ];

  const [messages, setMessages] = useState<AIChatMessage[]>([]);
  const [input, setInput] = useState<string>('');
  const inputId = useId();

  const sendMessage = async () => {
    const message: AIChatMessage = {
      role: 'user',
      content: input
    };
    setMessages([...messages, message]);
    setInput('');
    // const result = await client.getCompletion(messages);
    // const response = result.choices[0].message;
    // setMessages([...messages, response]);
  };
  return (
    <div>
        {messages.map(message => (
          <Text key={crypto.randomUUID()}>{message.content}</Text>
        ))}
        <Input id={inputId} value={input} />
        <Button onClick={sendMessage}>Send</Button>
    </div>
  );
}

function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <Chat />
    </FluentProvider>
  );
}

export default App;
