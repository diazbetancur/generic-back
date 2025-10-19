using System.Text.Json.Serialization;
using CC.Domain.Contracts;

namespace CC.Infrastructure.External.Xero
{
    #region Health Check DTOs

    /// <summary>
    /// Response de health check de Xero API
    /// </summary>
    internal sealed class XeroHealthResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("server")]
        public string? Server { get; set; }
    }

    #endregion

    #region Studies DTOs

    /// <summary>
    /// Response de lista de estudios
    /// </summary>
    internal sealed class XeroStudiesResponse
    {
        [JsonPropertyName("patient_id")]
        public string? PatientId { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("next_offset")]
        public int? NextOffset { get; set; }

        [JsonPropertyName("studies")]
        public List<XeroStudy>? Studies { get; set; }
    }

    #endregion

    #region Viewer Link DTOs

    /// <summary>
    /// Request para generar enlace del visor
    /// </summary>
    internal sealed class XeroViewerLinkRequest
    {
        [JsonPropertyName("patient_id")]
        public string? PatientId { get; set; }
    }

    /// <summary>
    /// Response de enlace del visor
    /// </summary>
    internal sealed class XeroViewerLinkResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("viewer_url")]
        public string? ViewerUrl { get; set; }

        [JsonPropertyName("expires_at")]
        public string? ExpiresAt { get; set; }
    }

    #endregion

    #region Error DTOs

    /// <summary>
    /// Response de error de Xero API
    /// </summary>
    internal sealed class XeroErrorResponse
    {
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }
    }

    #endregion
}
