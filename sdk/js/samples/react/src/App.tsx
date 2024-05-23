import { FluentProvider, webLightTheme } from '@fluentui/react-components'
import Chat from './Chat.tsx';
import Readme from './Readme.tsx';

function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <div style={{ display: 'flex', height: '100vh' }}>
        <Chat style={{ flex: 1 }} />
        <Readme style={{ flex: 1 }} />
      </div>
    </FluentProvider>
  );
}

export default App;
