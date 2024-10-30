# 🤖 AI Chat UI: Reusable Chat Web Component

As part of the [AI Chat Protocol SDK](/sdk), we provide a Chat UI [Web Component](https://developer.mozilla.org/docs/Web/Web_Components) that can be easily integrated into your application and can be used with any modern web framework or plain HTML.

## Usage

### Importing directly in the browser

In your HTML file, you can import the web component directly from the CDN:

```html
<script type="module" src="https://unpkg.com/@microsoft/ai-chat-protocol"></script>
```

This will make the `mai-chat` web component available in your HTML code.

```html
<mai-chat options="{ apiUrl: '<MY_API_URL' }"></mai-chat>
```

### Using a build system

Once the package is installed, you can use the web component in your HTML code or template like this:

```html
<mai-chat options="{ apiUrl: '<MY_API_URL' }"></mai-chat>
```

Depending of the framework and build system you're using, you'll have to import the web component in your JS code in different ways. You can have a look at the various integrations examples here:

- [Vanilla HTML](../samples/frontend/js/wc-html)
- [Angular](../wc-angular)
- [React](../wc-react)
- [Vue](../wc-vue)
- [Svelte](../wc-svelte)

### Configuration

### `mai-chat` web component

This web component is used to display the chat interface. It can be used with or without the `mai-auth` component.

#### Attributes

- `options`: JSON options to configure the chat component. See [ChatComponentOptions](../sdk/js/packages/client/src/ui/chat.ts#L28) for more details.
- `question`: Initial question to display in the chat.
- `messages`: Array of [messages](https://github.com/microsoft/ai-chat-protocol) to display in the chat.

By default, the component expect the [Chat API implementation](https://github.com/microsoft/ai-chat-protocol) to be available at `/api/chat`. You can change this URL by setting the `options.apiUrl` property.
