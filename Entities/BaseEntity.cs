using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Entities
{
    public class BaseEntity
    {
        [Key]
        public long Id { get; set; }

        // FIX: remove "required" and set default
        public string CreatedBy { get; set; } = "Admin";

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public string? LastModifiedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }

        public bool IsDelete { get; set; } = false;
    }
}
