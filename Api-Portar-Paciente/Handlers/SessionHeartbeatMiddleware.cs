using System.IdentityModel.Tokens.Jwt;
using CC.Domain.Interfaces.Repositories;

namespace Api_Portar_Paciente.Handlers
{
 public class SessionHeartbeatMiddleware
 {
 private readonly RequestDelegate _next;

 public SessionHeartbeatMiddleware(RequestDelegate next)
 {
 _next = next;
 }

 public async Task Invoke(HttpContext context, ISessionsRepository sessionsRepository)
 {
 try
 {
 var auth = context.User?.Identity?.IsAuthenticated ?? false;
 if (auth)
 {
 var jti = context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
 if (!string.IsNullOrEmpty(jti))
 {
 var session = await sessionsRepository.FindByAlternateKeyAsync(s => s.Jti == jti);
 if (session != null && session.IsActive)
 {
 var now = DateTime.UtcNow;
 if (!session.LastSeenAt.HasValue || (now - session.LastSeenAt.Value).TotalSeconds >=60)
 {
 session.LastSeenAt = now;
 session.ClientIp = context.Connection.RemoteIpAddress?.ToString();
 await sessionsRepository.UpdateAsync(session);
 }
 }
 }
 }
 }
 catch { /* no-op */ }

 await _next(context);
 }
 }
}
