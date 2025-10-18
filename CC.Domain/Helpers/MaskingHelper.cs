namespace CC.Domain.Helpers
{
    /// <summary>
    /// Helper para enmascarar información sensible
    /// </summary>
    public static class MaskingHelper
    {
        /// <summary>
        /// Enmascara un número de teléfono mostrando solo los últimos 2 dígitos
        /// </summary>
        /// <param name="phone">Número de teléfono a enmascarar</param>
        /// <returns>Número de teléfono enmascarado o null si la entrada es null/empty</returns>
        public static string? MaskPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var last2 = phone.Length >= 2 ? phone[^2..] : phone;
            return new string('*', Math.Max(0, phone.Length - last2.Length)) + last2;
        }

        /// <summary>
        /// Enmascara un email mostrando solo los últimos 2 caracteres del nombre local
        /// </summary>
        /// <param name="email">Email a enmascarar</param>
        /// <returns>Email enmascarado o null si la entrada es null/empty</returns>
        public static string? MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var at = email.IndexOf('@');
            if (at <= 0) return "********"; // fallback
            var local = email.Substring(0, at);
            var domain = email.Substring(at);
            var last2 = local.Length >= 2 ? local[^2..] : local;
            return new string('*', Math.Max(0, local.Length - last2.Length)) + last2 + domain;
        }

        /// <summary>
        /// Enmascara un número de documento mostrando solo los últimos 4 dígitos
        /// </summary>
        /// <param name="document">Número de documento a enmascarar</param>
        /// <returns>Documento enmascarado o null si la entrada es null/empty</returns>
        public static string? MaskDocument(string? document)
        {
            if (string.IsNullOrWhiteSpace(document)) return null;
            var last4 = document.Length >= 4 ? document[^4..] : document;
            return new string('*', Math.Max(0, document.Length - last4.Length)) + last4;
        }
    }
}
