using System.Text.Json.Serialization;

namespace CC.Infrastructure.External.Dtos
{
    public sealed class ExternalPatientDto
    {
        [JsonPropertyName("num_ide")] public string? NumIde { get; set; }
        [JsonPropertyName("tipo_ide")] public string? TipoIde { get; set; }
        [JsonPropertyName("celular")] public string? Celular { get; set; }
        [JsonPropertyName("historia")] public string? Historia { get; set; }
        [JsonPropertyName("apellido")] public string? Apellido { get; set; }
        [JsonPropertyName("apellido2")] public string? Apellido2 { get; set; }
        [JsonPropertyName("nombre")] public string? Nombre { get; set; }
        [JsonPropertyName("correo")] public string? Correo { get; set; }
    }
}