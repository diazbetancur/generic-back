using System.Text.Json.Serialization;

namespace CC.Infrastructure.External.Sms
{
    #region Authentication DTOs

    /// <summary>
    /// Request para autenticación en Liwa API
    /// </summary>
    internal sealed class LiwaAuthRequest
    {
        [JsonPropertyName("account")]
        public required string Account { get; set; }

        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }

    /// <summary>
    /// Response de autenticación en Liwa API
    /// </summary>
    internal sealed class LiwaAuthResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("expiresIn")]
        public int? ExpiresIn { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }

    #endregion

    #region Single SMS DTOs

    /// <summary>
    /// Request para envío de SMS individual
    /// </summary>
    internal sealed class LiwaSendSingleSmsRequest
    {
        [JsonPropertyName("number")]
        public required string Number { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; } = 1; // 1 = Nacional por defecto
    }

    /// <summary>
    /// Response de envío de SMS individual
    /// </summary>
    internal sealed class LiwaSendSmsResponse
    {
        [JsonPropertyName("messageId")]
        public string? MessageId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    #endregion

    #region Multiple SMS DTOs

    /// <summary>
    /// Mensaje individual para envío masivo
    /// </summary>
    internal sealed class LiwaSmsMessage
    {
        [JsonPropertyName("codeCountry")]
        public string CodeCountry { get; set; } = "57"; // Colombia por defecto

        [JsonPropertyName("number")]
        public required string Number { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; } = 1; // 1 = Nacional
    }

    /// <summary>
    /// Request para envío masivo de SMS
    /// </summary>
    internal sealed class LiwaSendMultipleSmsRequest
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("sendingDate")]
        public string? SendingDate { get; set; } // "YYYY-MM-DD HH:MM:SS" o null para inmediato

        [JsonPropertyName("messages")]
        public required List<LiwaSmsMessage> Messages { get; set; }
    }

    /// <summary>
    /// Response de envío masivo
    /// </summary>
    internal sealed class LiwaSendMultipleSmsResponse
    {
        [JsonPropertyName("campaignId")]
        public string? CampaignId { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("totalSent")]
        public int TotalSent { get; set; }
    }

    #endregion
}
