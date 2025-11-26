namespace CC.Domain.Dtos.ExternalApis
{
    /// <summary>
    /// DTO para respuesta de descarga de PDFs
    /// </summary>
    public class PdfResponseDto
    {
        /// <summary>
        /// Nombre del archivo PDF
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de contenido (siempre application/pdf)
        /// </summary>
        public string ContentType { get; set; } = "application/pdf";

        /// <summary>
        /// Contenido del PDF codificado en base64
        /// </summary>
        public string Base64Content { get; set; } = string.Empty;

        /// <summary>
        /// Tamaño del archivo en bytes
        /// </summary>
        public long Size { get; set; }
    }
}
