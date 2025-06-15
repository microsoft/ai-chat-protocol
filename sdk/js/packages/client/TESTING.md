# Testing Guide

## Overview

This project uses Vitest for testing with comprehensive coverage reporting and pre-commit hooks to ensure code quality.

## Running Tests

### Basic Test Commands

```bash
# Run all tests
npm test

# Run tests with coverage
npm run test:coverage

# Run tests in watch mode (during development)
npx vitest watch
```

### Test Structure

Tests are organized by environment:
- `*.node.test.ts` - Tests that run in Node.js environment
- `*.browser.test.ts` - Tests that run in browser environment (using Playwright)
- `*.shared.test.ts` - Tests that run in both environments

## Coverage Requirements

The project enforces the following coverage thresholds:
- Lines: 80%
- Functions: 80%
- Branches: 80%
- Statements: 80%

Coverage reports are generated in the `coverage/` directory and uploaded to Codecov for tracking.

## Pre-commit Hooks

The project uses Husky to run automated checks before commits and pushes:

### Pre-commit
- Runs lint-staged to format and lint changed files
- Runs the test suite to ensure no regressions

### Pre-push
- Runs full test suite with coverage
- Ensures coverage thresholds are met

## Writing Tests

### Test File Structure

```typescript
import { describe, expect, test, vi } from "vitest";

describe("Component/Function Name", () => {
  test("should do something specific", () => {
    // Arrange
    const input = "test";
    
    // Act
    const result = myFunction(input);
    
    // Assert
    expect(result).toBe("expected");
  });
});
```

### Mocking

Use Vitest's `vi` utilities for mocking:

```typescript
// Mock an entire module
vi.mock("./module", () => ({
  myFunction: vi.fn(),
}));

// Mock a specific function
const mockFn = vi.fn();
mockFn.mockReturnValue("mocked value");
```

## Continuous Integration

Tests run automatically on every pull request via GitHub Actions:
1. Linting and formatting checks
2. Full test suite execution
3. Coverage reporting to Codecov
4. Build verification

## Troubleshooting

### Tests Failing Locally

1. Ensure dependencies are installed: `npm install`
2. For browser tests, ensure Playwright is set up: `npm run prepare-tests`
3. Clear any test caches: `npx vitest --clearCache`

### Coverage Not Meeting Thresholds

1. Run `npm run test:coverage` to see detailed coverage report
2. Open `coverage/index.html` in a browser for line-by-line coverage
3. Focus on uncovered branches and edge cases

### Pre-commit Hooks Not Running

1. Ensure Husky is installed: `npm run prepare`
2. Check that `.husky/` directory exists
3. Verify hooks are executable: `ls -la .husky/`