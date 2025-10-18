namespace CC.Domain.Dtos
{
    public record StartAuthRequest(string DocTypeCode, string DocNumber, string ClientIp);
    public record StartAuthResponse(Guid ChallengeId, string? MaskedPhone, string? MaskedEmail);

    public record ResendOtpRequest(Guid ChallengeId, string ClientIp);
    public record ResendOtpResponse(Guid ChallengeId, string? MaskedPhone, string? MaskedEmail);

    public record VerifyOtpRequest(string DocTypeCode, string DocNumber, Guid ChallengeId, string Otp, string ClientIp);
    public record VerifyOtpResponse(string Token, DateTime ExpiresAt);

    // Validate: valida usuario, genera OTP y retorna datos enmascarados
    public record ValidateAuthRequest(string DocTypeCode, string DocNumber);
    public record ValidateAuthResponse(
        Guid ChallengeId, 
        string? MaskedPhone, 
        string? MaskedEmail, 
        string? FullName, 
        string? History);
}