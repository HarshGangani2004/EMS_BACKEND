namespace EmployeeManagement.Api.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}
