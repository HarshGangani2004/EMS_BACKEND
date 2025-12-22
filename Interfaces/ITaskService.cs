
using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> Create(CreateTaskDto dto, string username);
        Task<PagedResult<TaskDto>> GetPaged(int page, int pageSize, string? search, long? assignedTo);
        Task<TaskDto?> GetById(long id);
        Task<bool> Update(long id, UpdateTaskDto dto, string username);
        Task<bool> Delete(long id);
        Task<bool> UpdateStatus(long id, string status, string username);
    }
}
