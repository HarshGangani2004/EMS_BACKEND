using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface IAdminService
    {
        Task<PagedResult<UserDto>> GetUsersPaged(int page, int pageSize, string? search);
        Task<UserDto?> GetUser(string email);
        Task<string?> CreateUser(RegisterDto dto);
        Task<string?> ChangeRole(ChangeUserRoleDto dto);
        Task<string?> DeleteUser(string email);
    }
}
