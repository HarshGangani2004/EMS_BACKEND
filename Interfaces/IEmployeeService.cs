using EmployeeManagement.Api.DTOs;
using System.Security.Claims;

namespace EmployeeManagement.Api.Interfaces
{
    public interface IEmployeeService
    {
 
        Task<PagedResult<EmployeeDto>> GetPaged(int page, int pageSize, string? search, ClaimsPrincipal user);
        Task<EmployeeDto?> GetById(long id);

        Task<EmployeeDto> Create(CreateEmployeeDto dto, string username, ClaimsPrincipal user);
        Task<bool> Update(long id, UpdateEmployeeDto dto);
        Task<bool> Delete(long id);
        
    }
}
