// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import express, { Express } from "express";
import * as cors from "cors";

import chat from "./routes/chat";
import { ConfigParameter, getConfig } from "./config";

const app: Express = express();

const port = getConfig(ConfigParameter.port);

app.use(cors.default());

app.use("/api/chat", chat);

app.listen(port, () => {
  console.log(`[server]: Server is running at http://localhost:${port}`);
});
