using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class DailyTaskLogService : IDailyTaskLogService
    {
        private readonly AppDbContext _db;

        public DailyTaskLogService(AppDbContext db)
        {
            _db = db;
        }

        private static decimal ActualHours(DateTime s, DateTime e)
            => (decimal)(e - s).TotalHours;

        private static decimal Efficiency(decimal planned, decimal actual)
            => actual <= 0 ? 0 : Math.Round((planned / actual) * 100, 2);

        private static IQueryable<DailyTaskLog> ApplyFilters(
            IQueryable<DailyTaskLog> q,
            DailyTaskLogFilterDto f)
        {
            // ✅ YEAR + MONTH FILTER (CORRECT)
            if (f.Month.HasValue)
            {
                int month = f.Month.Value;
                int year = f.Year ?? DateTime.Now.Year;

                var fromDate = new DateTime(year, month, 1);
                var toDate = fromDate.AddMonths(1);

                q = q.Where(x =>
                    x.StartTime >= fromDate &&
                    x.StartTime < toDate
                );
            }
            else if (f.Year.HasValue)
            {
                var fromDate = new DateTime(f.Year.Value, 1, 1);
                var toDate = fromDate.AddYears(1);

                q = q.Where(x =>
                    x.StartTime >= fromDate &&
                    x.StartTime < toDate
                );
            }

            // ✅ PROJECT
            if (f.ProjectId.HasValue)
                q = q.Where(x => x.ProjectId == f.ProjectId);

            // ✅ STATUS
            if (!string.IsNullOrWhiteSpace(f.Status))
            {
                var status = f.Status.Replace(" ", "");
                q = q.Where(x => x.Status.Replace(" ", "") == status);
            }


            // ✅ SEARCH
            if (!string.IsNullOrWhiteSpace(f.Search))
                q = q.Where(x =>
                    x.Title.Contains(f.Search) ||
                    x.UserName.Contains(f.Search));

            return q;
        }



        // CREATE
        public async Task CreateAsync(CreateDailyTaskLogDto dto, long userId, string userName)
        {
            var entity = new DailyTaskLog
            {
                UserId = userId,
                UserName = userName,
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                PlannedHours = dto.PlannedHours,
                Status = dto.Status
            };

            _db.DailyTaskLogs.Add(entity);
            await _db.SaveChangesAsync();
        }

        // MY LIST
        public async Task<Interfaces.PagedResult<DailyTaskLogListDto>> GetMyLogsPagedAsync(
            long userId, int page, int pageSize, DailyTaskLogFilterDto filter)
        {
            var query = _db.DailyTaskLogs
                .Include(x => x.Project)
                .Where(x => x.UserId == userId);

            query = ApplyFilters(query, filter);
            return await BuildPaged(query, page, pageSize);
        }

        // ALL LIST
        public async Task<Interfaces.PagedResult<DailyTaskLogListDto>> GetAllLogsPagedAsync(
            int page, int pageSize, DailyTaskLogFilterDto filter)
        {
            IQueryable<DailyTaskLog> query = _db.DailyTaskLogs
         .Include(x => x.Project);
            query = ApplyFilters(query, filter);

            return await BuildPaged(query, page, pageSize);
        }

        private async Task<Interfaces.PagedResult<DailyTaskLogListDto>> BuildPaged(
            IQueryable<DailyTaskLog> q, int page, int pageSize)
        {
            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DailyTaskLogListDto
                {
                    Id = x.Id,
                    WorkDate = x.StartTime.Date,
                    ProjectName = x.Project!.Name,
                    WorkTitle = x.Title,
                    UserName = x.UserName,
                    PlannedHours = x.PlannedHours,
                    ActualHours = ActualHours(x.StartTime, x.EndTime),
                    Efficiency = Efficiency(
                        x.PlannedHours,
                        ActualHours(x.StartTime, x.EndTime)),
                    Status = x.Status
                })
                .ToListAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1; // 🔥 IMPORTANT

            return new Interfaces.PagedResult<DailyTaskLogListDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task UpdateAsync(long id, long userId, CreateDailyTaskLogDto dto)
        {
            var entity = await _db.DailyTaskLogs
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (entity == null)
                throw new Exception("Access denied");

            entity.ProjectId = dto.ProjectId;
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.PlannedHours = dto.PlannedHours;
            entity.Status = dto.Status;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await _db.DailyTaskLogs
                .FindAsync(id);

            if (entity == null)
                throw new Exception("Access denied");

            _db.DailyTaskLogs.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<DailyTaskLogDetailDto?> GetByIdAsync(long id)
        {
            return await _db.DailyTaskLogs
                .Include(x => x.Project)
                .Where(x => x.Id == id)
                .Select(x => new DailyTaskLogDetailDto
                {
                    Id = x.Id,
                    WorkDate = x.StartTime.Date,
                    ProjectId = x.ProjectId,
                    ProjectName = x.Project!.Name,
                    WorkTitle = x.Title,
                    StartTime=x.StartTime,
                    EndTime=x.EndTime,
                    Description = x.Description,
                    UserName = x.UserName,
                    PlannedHours = x.PlannedHours,
                    ActualHours = ActualHours(x.StartTime, x.EndTime),
                    Efficiency = Efficiency(
                        x.PlannedHours,
                        ActualHours(x.StartTime, x.EndTime)),
                    Status = x.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DailyTaskEfficiencySummaryDto> GetEfficiencySummaryAsync(
            long userId, bool canViewAll)
        {
            var q = _db.DailyTaskLogs.AsQueryable();

            if (!canViewAll)
                q = q.Where(x => x.UserId == userId);

            var planned = await q.SumAsync(x => x.PlannedHours);
            var actual = await q.SumAsync(x =>
                (decimal)(x.EndTime - x.StartTime).TotalHours);

            return new DailyTaskEfficiencySummaryDto
            {
                TotalPlannedHours = planned,
                TotalActualHours = actual,
                OverallEfficiency = Efficiency(planned, actual)
            };
        }
    }
}
