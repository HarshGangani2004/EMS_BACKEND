using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface IDailyTaskLogService
    {
        Task CreateAsync(CreateDailyTaskLogDto dto, long userId, string userName);

        Task<PagedResult<DailyTaskLogListDto>> GetMyLogsPagedAsync(
            long userId, int page, int pageSize, DailyTaskLogFilterDto filter);

        Task<PagedResult<DailyTaskLogListDto>> GetAllLogsPagedAsync(
            int page, int pageSize, DailyTaskLogFilterDto filter);

        Task<DailyTaskLogDetailDto?> GetByIdAsync(long id);

        Task UpdateAsync(long id, long userId, CreateDailyTaskLogDto dto);
        Task DeleteAsync(long id);

        Task<DailyTaskEfficiencySummaryDto> GetEfficiencySummaryAsync(
            long userId, bool canViewAll);
    }
}
