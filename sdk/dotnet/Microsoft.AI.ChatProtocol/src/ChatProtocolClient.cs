namespace Microsoft.AI.ChatProtocol
{
    public class ChatProtocolClient
    {
        private readonly Uri _endpoint;
        private readonly ChatProtocolClientOptions? _clientOptions;

        /// <summary> Initializes a new instance of ChatProtocolClient. </summary>
        /// <param name="endpoint"> The Uri to use. </param>
        /// <param name="options"> Additional client options. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="endpoint"/></exception>
        public ChatProtocolClient(Uri endpoint, ChatProtocolClientOptions? clientOptions = null)
        {
            Argument.AssertNotNull(endpoint, nameof(endpoint));
            Argument.AssertNotNull(clientOptions, nameof(clientOptions));

            _endpoint = endpoint;
            _clientOptions = clientOptions;
        }

        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        public /* virtual */ void /*Response<ChatCompletion> */ Create(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(chatCompletionOptions, nameof(chatCompletionOptions));
/*
            RequestContext context = FromCancellationToken(cancellationToken);
            using RequestContent requestContent = chatCompletionOptions.ToRequestContent();
            using HttpMessage message = CreateCreateRequest(_chatRoute, requestContent, context);

            Response response = ProcessMessage("ChatProtocolClient.Create", message, context);
            return Response.FromValue(ChatCompletion.FromResponse(response), response);
*/
        }
    }
}
