import express, { Express, Request, Response } from "express";
import dotenv from "dotenv";

import chat from "./routes/chat";
import { ConfigParameter, getConfig } from "./config";

dotenv.config();

const app: Express = express();

const port = getConfig(ConfigParameter.port);

app.use("/api/chat", chat);

app.listen(port, () => {
  console.log(`[server]: Server is running at http://localhost:${port}`);
});
