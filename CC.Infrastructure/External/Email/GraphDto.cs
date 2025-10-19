using System.Text.Json.Serialization;

namespace CC.Infrastructure.External.Email
{
    #region Send Email DTOs

    /// <summary>
    /// DTO para envío de email a través de Microsoft Graph
    /// </summary>
    internal sealed class GraphSendMailRequest
    {
        [JsonPropertyName("message")]
        public required GraphMessage Message { get; set; }

        [JsonPropertyName("saveToSentItems")]
        public bool SaveToSentItems { get; set; } = true;
    }

    /// <summary>
    /// Mensaje de correo
    /// </summary>
    internal sealed class GraphMessage
    {
        [JsonPropertyName("subject")]
        public required string Subject { get; set; }

        [JsonPropertyName("body")]
        public required GraphBody Body { get; set; }

        [JsonPropertyName("toRecipients")]
        public required List<GraphRecipient> ToRecipients { get; set; }

        [JsonPropertyName("ccRecipients")]
        public List<GraphRecipient>? CcRecipients { get; set; }

        [JsonPropertyName("bccRecipients")]
        public List<GraphRecipient>? BccRecipients { get; set; }

        [JsonPropertyName("from")]
        public GraphRecipient? From { get; set; }

        [JsonPropertyName("replyTo")]
        public List<GraphRecipient>? ReplyTo { get; set; }

        [JsonPropertyName("importance")]
        public string? Importance { get; set; } // "low", "normal", "high"

        [JsonPropertyName("attachments")]
        public List<GraphAttachment>? Attachments { get; set; }
    }

    /// <summary>
    /// Cuerpo del mensaje
    /// </summary>
    internal sealed class GraphBody
    {
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = "HTML"; // "Text" o "HTML"

        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }

    /// <summary>
    /// Destinatario de email
    /// </summary>
    internal sealed class GraphRecipient
    {
        [JsonPropertyName("emailAddress")]
        public required GraphEmailAddress EmailAddress { get; set; }
    }

    /// <summary>
    /// Dirección de email
    /// </summary>
    internal sealed class GraphEmailAddress
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    /// <summary>
    /// Archivo adjunto
    /// </summary>
    internal sealed class GraphAttachment
    {
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; } = "#microsoft.graph.fileAttachment";

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("contentBytes")]
        public required string ContentBytes { get; set; } // Base64
    }

    #endregion
}
