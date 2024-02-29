using Microsoft.Extensions.Logging;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.AI.ChatProtocol
{
    public class ChatProtocolClient
    {
        private readonly Uri _endpoint;
        private readonly ChatProtocolClientOptions? _clientOptions;
        private ILogger _logger;

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

            if (_clientOptions != null && _clientOptions.LoggerFactory != null)
            {
                _logger = _clientOptions.LoggerFactory.CreateLogger<ChatProtocolClient>();

                _logger.LogInformation("Endpoint = {endpoint}\n\t  HTTP request headers = {options}", _endpoint, _clientOptions.HttpHeaders);
            }
        }


        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        public HttpResponseMessage Create(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(chatCompletionOptions, nameof(chatCompletionOptions));

            using (HttpClient client = new HttpClient())
            {
                string jsonBody = "{\"messages\": [{\"role\":\"system\",\"content\":\"You are an AI assistant that helps people find information.\"},{\"role\":\"user\",\"content\":\"How many feet in a mile??\"}],\"stream\": false}";
                HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                if (_clientOptions != null && _clientOptions.HttpHeaders != null)
                {
                    foreach (var header in _clientOptions.HttpHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpResponseMessage response = client.PostAsync(_endpoint, content, cancellationToken).Result;

                if (_logger != null) 
                {
                    _logger.LogInformation("Response = {response}", response);

                    if (response.Content != null && response.Content.Headers.GetValues("Content-Type").First() == "application/json")
                    {
                        _logger.LogInformation("Response body = {body}", response.Content.ReadAsStringAsync().Result);
                    }
                }

                return response;
            }
/*
            /// <summary> Creates a new chat completion. </summary>
            /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
            /// <param name="cancellationToken"> The cancellation token to use. </param>
            /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
            public async Task<HttpResponseMessage> Create(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(chatCompletionOptions, nameof(chatCompletionOptions));

            using (HttpClient client = new HttpClient())
            {
                string jsonBody = "{}";
                HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                if (_clientOptions != null && _clientOptions.HttpHeaders != null)
                {
                    foreach (var header in _clientOptions.HttpHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }   

                HttpResponseMessage response = await client.PostAsync(_endpoint, content, cancellationToken);
                return response;
            }
*/
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
