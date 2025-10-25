using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 [Authorize] // agregar política de Admin cuando esté disponible
 public class ReportsController : ControllerBase
 {
 private readonly IReportsService _service;

 public ReportsController(IReportsService service)
 {
 _service = service;
 }

 [HttpGet("logins/effective")]
 public async Task<IActionResult> Effective([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
 {
 var (f, t) = Normalize(from, to);
 var count = await _service.GetEffectiveLoginsAsync(f, t);
 return Ok(new { count, from = f, to = t });
 }

 [HttpGet("logins/ineffective")]
 public async Task<IActionResult> Ineffective([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? reason = null)
 {
 var (f, t) = Normalize(from, to);
 var count = await _service.GetIneffectiveLoginsAsync(f, t, reason);
 return Ok(new { count, reason, from = f, to = t });
 }

 [HttpGet("sessions/avg-duration")]
 public async Task<IActionResult> AvgSession([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
 {
 var (f, t) = Normalize(from, to);
 var avg = await _service.GetAvgSessionMinutesAsync(f, t);
 return Ok(new { averageMinutes = avg, from = f, to = t });
 }

 [HttpGet("logins/by-timeslice")]
 public async Task<IActionResult> ByTimeSlice([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? timezone = null)
 {
 var (f, t) = Normalize(from, to);
 var tz = string.IsNullOrWhiteSpace(timezone) ? TimeZoneInfo.Utc : TimeZoneInfo.FindSystemTimeZoneById(timezone);
 var kpi = await _service.GetLoginsByTimeSlicesAsync(f, t, tz);
 return Ok(new { kpi, timezone = tz.Id, from = f, to = t });
 }

 private static (DateTime fromUtc, DateTime toUtc) Normalize(DateTime? from, DateTime? to)
 {
 var toUtc = (to ?? DateTime.UtcNow);
 var fromUtc = (from ?? toUtc.AddDays(-30));
 return (fromUtc, toUtc);
 }
 }
}
