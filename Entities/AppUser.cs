using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.Api.Entities
{
    public class AppUser : IdentityUser<long>
    {
        public string FullName { get; set; }
        public long RoleId { get; internal set; }
    }

}
