// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import Chat from "./Chat.tsx";
import Readme from "./Readme.tsx";
import styles from "./App.module.css";

function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <div className={styles.appContainer}>
        <Chat style={{ flex: 1 }} />
        <Readme style={{ flex: 1 }} />
      </div>
    </FluentProvider>
  );
}

export default App;
