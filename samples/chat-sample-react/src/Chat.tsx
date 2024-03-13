import { Button, Input, makeStyles, Text, ToggleButton } from "@fluentui/react-components";
import { AIChatMessage, AIChatProtocolClient } from "@microsoft/ai-chat-protocol";
import { useId, useState } from "react";

const useStyles = makeStyles({
  messages: {
    top: "10px",
    left: "10px",
    right: "10px",
    bottom: "50px",
    position: 'absolute',
    overflowY: 'auto',
    paddingRight: '10px',
    paddingLeft: '10px',
    paddingTop: '10px',
    paddingBottom: '10px',

  },
  input: {
    position: 'absolute',
    bottom: '10px',
    left: '10px',
    right: '10px',
  },
});

const useMessageStyles = makeStyles({
  assistant: {
    color: 'blue',
    fontWeight: 'bold',
  },
  user: {
    color: 'green',
    fontWeight: 'bold',
  },
});

export default function Chat() {
  const client = new AIChatProtocolClient('http://localhost:8080/chat', {
    allowInsecureConnection: true
  });
  const configMessage: AIChatMessage = {
    role: 'system',
    content: 'You are an enthusiastic assistant that talks like a 5 year old.',
  };

  const [messages, setMessages] = useState<AIChatMessage[]>([]);
  const [input, setInput] = useState<string>('');
  const [streaming, setStreaming] = useState<boolean>(false);
  const inputId = useId();

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
      const result = await client.getStreamedCompletion([configMessage, ...updatedMessages]);
      const latestMessage: AIChatMessage = { content: "", role: 'assistant' };
      for await (const response of result) {
        const choice = response.choices[0];
        if (!choice.delta) {
          continue;
        }
        if (choice.delta.role) {
          latestMessage.role = choice.delta.role;
        }
        if (choice.delta.content) {
          latestMessage.content += choice.delta.content;
          setMessages([...updatedMessages, latestMessage]);
        }
      }
    } else {
      const result = await client.getCompletion([configMessage, ...updatedMessages]);
      setMessages([...updatedMessages, result.choices[0].message]);
    }
  };

  const styles = useStyles();
  const messageStyles = useMessageStyles();
  return (
    <div>
      <div className={styles.messages}>
        {messages.map(message => (
          <div key={crypto.randomUUID()}>
            <Text className={message.role === 'user' ? messageStyles.user : messageStyles.assistant}>{message.role}: {message.content}</Text>
          </div>
        ))}
      </div>
      <div className={styles.input}>
        <Input id={inputId} value={input} onChange={(e) => setInput(e.target.value)} />
        <Button onClick={sendMessage}>Send</Button>
        <Button onClick={clear}>Clear</Button>
        <ToggleButton checked={streaming} onClick={() => setStreaming(!streaming)} >Streaming</ToggleButton>
      </div>
    </div>
  );
}
