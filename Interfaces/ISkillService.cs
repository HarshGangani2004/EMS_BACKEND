using EmployeeManagement.Api.DTOs;
using System.Security.Claims;

namespace EmployeeManagement.Api.Interfaces
{
    public interface ISkillService

    {
        Task<List<SkillDto>> GetAll();
        Task<SkillDto?> GetById(long id);
        Task<SkillDto> Create(CreateSkillDto dto, string username, ClaimsPrincipal user);
        Task<bool> Update(long id, CreateSkillDto dto);
        Task<bool> Delete(long id);
        Task<PagedResult<SkillDto>> GetPaged(int page, int pageSize, string? search, ClaimsPrincipal user);
    }
}
