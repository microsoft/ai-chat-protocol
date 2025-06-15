# GitHub Copilot Instructions

This file provides guidance to GitHub Copilot when working with code in this repository.

## Overview

The Microsoft AI Chat Protocol is a standardized API specification for AI chat applications with SDK implementations. The project consists of:

- TypeSpec API specification defining the protocol
- TypeScript/JavaScript client SDK (@microsoft/ai-chat-protocol)
- Sample implementations in multiple languages (C#, JavaScript/TypeScript, Python)

## Key Commands

### SDK Development (TypeScript/JavaScript)

```bash
# Navigate to SDK directory
cd sdk/js/packages/client

# Install dependencies
npm install

# Build the SDK
npm run build

# Run tests
npm test

# Run tests with coverage
npm run test:coverage

# Lint code
npm run lint

# Format code
npm run format

# Check formatting
npm run check-format

# Clean build artifacts
npm run clean
```

### Sample Backend Development

#### Express.js Backend

```bash
cd samples/backend/js/expressjs
npm install
npm run build
npm run dev  # Development server with hot reload
npm start    # Production server
```

#### Python Quart Backend

```bash
cd samples/backend/python/quart
pip install -r requirements.txt
python -m app  # Run the server
```

#### C# ASP.NET Core Backend

```bash
cd samples/backend/csharp
dotnet restore
dotnet build
dotnet run
```

### Sample Frontend Development (React)

```bash
cd samples/frontend/js/react
npm install
npm run dev      # Development server
npm run build    # Production build
npm run lint     # Lint code
npm run format   # Format code
```

### TypeSpec API Specification

```bash
cd spec
npm install
npm run compile  # Compile TypeSpec to OpenAPI
```

## Architecture

### Protocol Structure

The AI Chat Protocol defines a standardized HTTP API with:

- **POST /chat** - Synchronous chat completions
- **POST /chat/stream** - Server-sent events streaming responses

Request format includes:

- `messages`: Array of chat messages with role and content
- `context`: Optional request context (temperature, model settings, etc.)
- `sessionState`: Optional session memory/state

Response format includes:

- `message`: Assistant's response
- `context`: Response metadata (citations, intent, etc.)
- `sessionState`: Updated session state
- `error`: Error information if applicable

### SDK Client Architecture

The TypeScript SDK (`@microsoft/ai-chat-protocol`) provides:

- `AIChatProtocolClient` - Main client class for API interactions
- Streaming support with async iterators
- Error handling with typed error responses
- TypeScript models matching the API specification
- Both browser and Node.js support through multiple build targets

### Testing

- SDK uses Vitest with Playwright for browser testing
- Tests require Playwright Chromium: `npm run prepare-tests`
- Run a single test: `npm test -- <test-name>`
- Test coverage: `npm run test:coverage` (80% threshold for all metrics)
- Pre-commit hooks run linting, formatting, and tests automatically
- Coverage reports are uploaded to Codecov in CI

### Code Style

- TypeScript/JavaScript: Prettier formatting, ESLint linting
- Python: Black formatting (120 char line length), Ruff linting
- Follow existing patterns in each language/framework
