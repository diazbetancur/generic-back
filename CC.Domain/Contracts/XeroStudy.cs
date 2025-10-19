using System.Text.Json.Serialization;

namespace CC.Domain.Contracts
{
    /// <summary>
    /// Estudio de imagen diagnóstica (Xero)
    /// </summary>
    public sealed class XeroStudy
    {
        /// <summary>
        /// UID único del estudio
        /// </summary>
        [JsonPropertyName("uid")]
        public string? Uid { get; set; }

        /// <summary>
        /// Descripción del estudio
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Número de acceso del estudio
        /// </summary>
        [JsonPropertyName("accession")]
        public string? Accession { get; set; }

        /// <summary>
        /// Fecha y hora del estudio
        /// </summary>
        [JsonPropertyName("date_time")]
        public string? DateTime { get; set; }
    }
}
