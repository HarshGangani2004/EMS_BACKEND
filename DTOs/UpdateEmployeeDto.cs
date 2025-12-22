namespace EmployeeManagement.Api.DTOs
{
    public class UpdateEmployeeDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal Salary { get; set; }
        public DateTime JoinedOn { get; set; }

        public long DepartmentId { get; set; }

        public List<long> SkillIds { get; set; } = new();
    }
}
