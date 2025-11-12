namespace CC.Domain.Contracts
{
    /// <summary>
    /// Resultado de consulta de exámenes de NilRead
    /// </summary>
    public record NilReadExamsResult(
        bool Success,
        string PatientId,
        int Offset,
        int Limit,
        int Count,
        int Total,
        int? NextOffset,
        List<NilReadExamInfo>? Exams,
        string? ErrorMessage);

    /// <summary>
    /// Información de un examen
    /// </summary>
    public record NilReadExamInfo(
        string? Accession,
        string? Description,
        string? DateTime,
        bool ReportAvailable,
        string? ReportUrl,
        bool ImagesAvailable);

    /// <summary>
    /// Resultado de obtención de informe PDF
    /// </summary>
    public record NilReadReportResult(
        bool Success,
        byte[]? PdfContent,
        string? FileName,
        string? ErrorMessage);

    /// <summary>
    /// Resultado de generación de enlace del visor
    /// </summary>
    public record NilReadViewerLinkResult(
        bool Success,
        string? Accession,
        string? PatientId,
        string? DateTime,
        string? Provider,
        List<string>? AccNumbersUsed,
        string? ViewerUrl,
        string? ResultCode,
        string? ResultDescription,
        string? ErrorMessage);
}
