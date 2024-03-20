import { FluentProvider, webLightTheme } from '@fluentui/react-components'
import Chat from './Chat.tsx';

function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <Chat />
    </FluentProvider>
  );
}

export default App;
