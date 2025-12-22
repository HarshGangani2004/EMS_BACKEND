 using EmployeeManagement.Api.DTOs;
namespace EmployeeManagement.Api.Interfaces
{
    public interface IAuthService
    {
        Task<string> Login(LoginDto dto);
        Task<string> Register(RegisterDto dto);
 
    }
}
