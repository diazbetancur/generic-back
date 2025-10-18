using System.Net;
using System.Net.Http;

namespace CC.Infrastructure.External
{
    public static class HttpClientConfiguration
    {
        // Optionally configure handler to ignore invalid SSL in DEV/QA (use with caution)
        public static HttpMessageHandler CreateHandler(bool allowInvalidCerts)
        {
            if (!allowInvalidCerts) return new HttpClientHandler();

            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
        }
    }
}
