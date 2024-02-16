namespace Microsoft.AI.ChatProtocol
{
    public class ChatProtocolClientOptions
    {
        public Dictionary<string, string>? HttpHeaders { get; set; }

        public ChatProtocolClientOptions(Dictionary<string, string> httpHeaders)
        {
            HttpHeaders = httpHeaders;
        }
    }
}
