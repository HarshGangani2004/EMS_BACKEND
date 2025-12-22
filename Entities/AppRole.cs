using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity;


namespace EmployeeManagement.Api.Entities
{
    public class AppRole : IdentityRole<long>
    {
        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}
