import {
  Button,
  Input,
  makeStyles,
  Text,
  ToggleButton,
} from "@fluentui/react-components";
import {
  AIChatMessage,
  AIChatProtocolClient,
  AIChatError,
} from "@microsoft/ai-chat-protocol";
import { useId, useState } from "react";

const useStyles = makeStyles({
  messages: {
    top: "10px",
    left: "10px",
    right: "10px",
    bottom: "50px",
    position: "absolute",
    overflowY: "auto",
    paddingRight: "10px",
    paddingLeft: "10px",
    paddingTop: "10px",
    paddingBottom: "10px",
  },
  input: {
    position: "absolute",
    bottom: "10px",
    left: "10px",
    right: "10px",
  },
});

const useMessageStyles = makeStyles({
  assistant: {
    color: "blue",
    fontWeight: "bold",
  },
  user: {
    color: "green",
    fontWeight: "bold",
  },
});

type ChatEntry = AIChatMessage | AIChatError;

function isChatError(entry: unknown): entry is AIChatError {
  return (entry as AIChatError).code !== undefined;
}

export default function Chat() {
  const client = new AIChatProtocolClient("/api/chat");

  const [messages, setMessages] = useState<ChatEntry[]>([]);
  const [input, setInput] = useState<string>("");
  const [streaming, setStreaming] = useState<boolean>(false);
  const inputId = useId();

  const clear = () => {
    setMessages([]);
    setInput("");
  };

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
        const result = await client.getStreamedCompletion([message]);
        const latestMessage: AIChatMessage = { content: "", role: "assistant" };
        for await (const response of result) {
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
        const result = await client.getCompletion([message]);
        setMessages([...updatedMessages, result.message]);
      }
    } catch (e) {
      if (isChatError(e)) {
        setMessages([...updatedMessages, e]);
      }
    }
  };

  const styles = useStyles();
  const messageStyles = useMessageStyles();
  return (
    <div>
      <div className={styles.messages}>
        {messages.map((message) => (
          <div key={crypto.randomUUID()}>
            {isChatError(message) ? (
              <Text>
                {message.code}: {message.message}
              </Text>
            ) : (
              <Text
                className={
                  message.role === "user"
                    ? messageStyles.user
                    : messageStyles.assistant
                }
              >
                {message.role}: {message.content}
              </Text>
            )}
          </div>
        ))}
      </div>
      <div className={styles.input}>
        <Input
          id={inputId}
          value={input}
          onChange={(e) => setInput(e.target.value)}
        />
        <Button onClick={sendMessage}>Send</Button>
        <Button onClick={clear}>Clear</Button>
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
