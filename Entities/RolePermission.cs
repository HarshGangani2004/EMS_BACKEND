namespace EmployeeManagement.Api.Entities
{
    public class RolePermission
    {
        public long RoleId { get; set; }
        public AppRole? Role { get; set; }

        public long PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
