namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para transferencia de datos de preguntas frecuentes
    /// </summary>
    public class FrequentQuestionsDto : BaseDto<Guid>
    {
        /// <summary>
        /// Texto de la pregunta frecuente
        /// </summary>
        public required string Question { get; set; }

        /// <summary>
        /// Respuesta a la pregunta frecuente
        /// </summary>
        public required string Response { get; set; }
    }
}