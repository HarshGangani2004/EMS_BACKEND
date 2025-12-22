namespace EmployeeManagement.Api.DTOs
{
    public class AssignPermissionsDto
    {
        public long RoleId { get; set; }
        public List<long> PermissionIds { get; set; } = new();
    }
}
