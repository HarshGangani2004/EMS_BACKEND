using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } 
    }
}
