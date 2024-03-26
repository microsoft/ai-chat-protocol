import express, { Express } from "express";
import dotenv from "dotenv";

dotenv.config();

import chat from "./routes/chat";
import { ConfigParameter, getConfig } from "./config";

const app: Express = express();

const port = getConfig(ConfigParameter.port);

app.use("/api/chat", chat);

app.listen(port, () => {
  console.log(`[server]: Server is running at http://localhost:${port}`);
});
