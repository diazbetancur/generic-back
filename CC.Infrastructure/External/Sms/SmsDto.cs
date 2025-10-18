using System.Text.Json.Serialization;

namespace CC.Infrastructure.External.Sms
{
    /// <summary>
    /// Request DTO para envío de SMS
    /// </summary>
    internal sealed class SendSmsRequest
    {
        [JsonPropertyName("to")]
        public required string To { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("from")]
        public string? From { get; set; }
    }

    /// <summary>
    /// Response DTO para envío de SMS
    /// </summary>
    internal sealed class SendSmsResponse
    {
        [JsonPropertyName("messageId")]
        public string? MessageId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
