using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface ILeaveService
    {
        Task ApplyLeaveAsync(ApplyLeaveDto dto,string user);

        Task<PagedResult<LeaveListDto>> GetMyLeavesPagedAsync(
        long userId, int page, int pageSize, LeaveFilterDto filter);


        Task<PagedResult<LeaveListDto>> GetAllLeavesPagedAsync(
         int page, int pageSize, LeaveFilterDto filter);

        Task<LeaveListDto?> GetByIdAsync(long id);
        Task UpdateStatusAsync(long id, string status, string actionBy);
        Task<List<LeaveListDto>> GetPendingLeavesAsync();
        Task DeleteLeaveAsync(long id, long userId);
        Task UpdateLeaveAsync(long id, long userId, UpdateLeaveDto dto);
    }
}
