namespace EmployeeManagement.Api.DTOs
{
    public class EmployeeDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal Salary { get; set; }
        public DateTime JoinedOn { get; set; }

        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public List<long> SkillIds { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public string? CreatedBy { get; set; }
    }
}
