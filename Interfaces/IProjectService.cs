
using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDto> Create(CreateProjectDto dto, string username);
        Task<PagedResult<ProjectDto>> GetPaged(int page, int pageSize, string? search);
        Task<ProjectDto?> GetById(long id);
        Task<bool> Update(long id, UpdateProjectDto dto, string username);
        Task<bool> Delete(long id);
    }
}
