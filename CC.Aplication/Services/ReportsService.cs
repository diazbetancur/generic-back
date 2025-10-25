using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CC.Aplication.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IQueryableUnitOfWork _uow;
        private readonly ISessionsRepository _sessionsRepo;
        private readonly ILoginAttemptRepository _attemptRepo;

        public ReportsService(IQueryableUnitOfWork uow, ISessionsRepository sessionsRepo, ILoginAttemptRepository attemptRepo)
        {
            _uow = uow;
            _sessionsRepo = sessionsRepo;
            _attemptRepo = attemptRepo;
        }

        public async Task<int> GetEffectiveLoginsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        {
            return await _uow.GetSet<Sessions>()
                .AsNoTracking()
                .Where(s => s.IssuedAt >= fromUtc && s.IssuedAt <= toUtc)
                .CountAsync(ct);
        }

        public async Task<int> GetIneffectiveLoginsAsync(DateTime fromUtc, DateTime toUtc, string? reason = null, CancellationToken ct = default)
        {
            var q = _uow.GetSet<LoginAttempt>()
                .AsNoTracking()
                .Where(a => !a.Success && a.DateCreated >= fromUtc && a.DateCreated <= toUtc);

            if (!string.IsNullOrWhiteSpace(reason))
            {
                q = q.Where(a => a.Reason == reason);
            }

            return await q.CountAsync(ct);
        }

        public async Task<double> GetAvgSessionMinutesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        {
            var data = await _uow.GetSet<Sessions>()
                .AsNoTracking()
                .Where(s => s.IssuedAt >= fromUtc && s.IssuedAt <= toUtc)
                .Select(s => new { s.IssuedAt, EndAt = (DateTime?)(s.LastSeenAt ?? (s.RevokedAt ?? s.ExpiresAt)) })
                .ToListAsync(ct);

            var durations = data
                .Where(x => x.EndAt.HasValue)
                .Select(x => (x.EndAt!.Value - x.IssuedAt).TotalMinutes)
                .ToList();

            if (durations.Count == 0)
            {
                return 0d;
            }

            return durations.Average();
        }

        public async Task<TimeSliceKpiDto> GetLoginsByTimeSlicesAsync(DateTime fromUtc, DateTime toUtc, TimeZoneInfo? tz = null, CancellationToken ct = default)
        {
            tz ??= TimeZoneInfo.Utc;

            var issuedList = await _uow.GetSet<Sessions>()
                .AsNoTracking()
                .Where(s => s.IssuedAt >= fromUtc && s.IssuedAt <= toUtc)
                .Select(s => s.IssuedAt)
                .ToListAsync(ct);

            int business = 0, off = 0, weekend = 0;
            foreach (var issuedUtc in issuedList)
            {
                var dt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(issuedUtc, DateTimeKind.Utc), tz);
                var day = dt.DayOfWeek;
                if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                {
                    weekend++;
                    continue;
                }

                var hour = dt.Hour;
                if (hour >= 7 && hour < 17)
                {
                    business++;
                }
                else
                {
                    off++;
                }
            }

            return new TimeSliceKpiDto(business, off, weekend);
        }
    }
}
