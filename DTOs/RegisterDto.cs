
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.DTOs
{
    public class RegisterDto
    {
        [Required,MinLength(3),MaxLength(30)]
        public string FullName { get; set; } = null!;
        [Required,EmailAddress]
        public string Email { get; set; } = null!;
        [Required,MinLength(6)]
        public string Password { get; set; } = null!;
    
        public string? Role { get; set; }
    }
}
