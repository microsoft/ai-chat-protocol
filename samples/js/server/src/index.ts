import express, { Express, Request, Response } from 'express';

import chat from './routes/chat';

const app: Express = express();
const port = 3000;

app.get('/', (req: Request, res: Response) => {
  res.send('Hello root World!');
});

app.use('/api/chat', chat);

app.listen(port, () => {
    console.log(`[server]: Server is running at http://localhost:${port}`);
  });