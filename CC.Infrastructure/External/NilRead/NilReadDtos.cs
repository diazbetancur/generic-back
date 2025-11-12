using System.Text.Json.Serialization;

namespace CC.Infrastructure.External.NilRead
{
    /// <summary>
    /// Respuesta de health check de NilRead
    /// </summary>
    public class NilReadHealthResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }
    }

    /// <summary>
    /// Respuesta de listado de exámenes
    /// </summary>
    public class NilReadExamsResponse
    {
        [JsonPropertyName("patient_id")]
        public string PatientId { get; set; } = string.Empty;

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("exams")]
        public List<NilReadExam> Exams { get; set; } = new();
    }

    /// <summary>
    /// Información de un examen
    /// </summary>
    public class NilReadExam
    {
        [JsonPropertyName("accession")]
        public string? Accession { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("date_time")]
        public string? DateTime { get; set; }

        [JsonPropertyName("report_available")]
        public bool ReportAvailable { get; set; }

        [JsonPropertyName("report_url")]
        public string? ReportUrl { get; set; }

        [JsonPropertyName("images_available")]
        public bool ImagesAvailable { get; set; }
    }

    /// <summary>
    /// Request para generar enlace del visor
    /// </summary>
    public class NilReadViewerLinkRequest
    {
        [JsonPropertyName("patientID")]
        public string PatientID { get; set; } = string.Empty;

        [JsonPropertyName("dateTime")]
        public string? DateTime { get; set; }

        [JsonPropertyName("accNumbers")]
        public List<string> AccNumbers { get; set; } = new();
    }

    /// <summary>
    /// Respuesta de generación de enlace del visor
    /// </summary>
    public class NilReadViewerLinkResponse
    {
        [JsonPropertyName("accession")]
        public string? Accession { get; set; }

        [JsonPropertyName("patient_id")]
        public string? PatientId { get; set; }

        [JsonPropertyName("date_time")]
        public string? DateTime { get; set; }

        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        [JsonPropertyName("acc_numbers_used")]
        public List<string>? AccNumbersUsed { get; set; }

        [JsonPropertyName("result")]
        public NilReadResult? Result { get; set; }

        [JsonPropertyName("viewer_url")]
        public string? ViewerUrl { get; set; }
    }

    /// <summary>
    /// Resultado interno de NilRead
    /// </summary>
    public class NilReadResult
    {
        [JsonPropertyName("Code")]
        public string? Code { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("URLFinal")]
        public string? URLFinal { get; set; }
    }
}
