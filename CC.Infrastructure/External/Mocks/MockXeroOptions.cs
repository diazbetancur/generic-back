namespace CC.Infrastructure.External.Mocks
{
 /// <summary>
 /// Opciones para el comportamiento del servicio mock de XeroViewer
 /// </summary>
 public sealed class MockXeroOptions
 {
 /// <summary>
 /// Base de la URL del visor mock. Se concatenará el UID del estudio escapado.
 /// </summary>
 public string? ViewerUrlBase { get; set; } = "https://xero-mock-viewer.local/view?study=";

 /// <summary>
 /// Minutos de expiración del enlace generado.
 /// </summary>
 public int ExpiresMinutes { get; set; } =15;

 /// <summary>
 /// Token estático a usar. Si no se especifica se generará uno aleatorio con prefijo 'mock-token-'.
 /// </summary>
 public string? StaticToken { get; set; }
 }
}
