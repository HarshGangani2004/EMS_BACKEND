namespace EmployeeManagement.Api.Entities
{
    public class Employee : BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal Salary { get; set; }
        public DateTime JoinedOn { get; set; }

        public long DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<EmployeeSkill> EmployeeSkills { get; set; }
            = new List<EmployeeSkill>();
    }
}
