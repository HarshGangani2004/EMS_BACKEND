using EmployeeManagement.Api.DTOs;
using System.Security.Claims;
namespace EmployeeManagement.Api.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAll();
        Task<PagedResult<DepartmentDto>> GetPaged(int page, int pageSize, string? search, ClaimsPrincipal user);

        Task<DepartmentDto?> GetById(long id);
        Task<DepartmentDto> Create(CreateDepartmentDto dto,string username, ClaimsPrincipal user);
        Task<bool> Update(long id, CreateDepartmentDto dto);
        Task<bool> Delete(long id);
    }
}

