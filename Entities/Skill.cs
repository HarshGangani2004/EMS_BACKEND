namespace EmployeeManagement.Api.Entities
{
    public class Skill : BaseEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<EmployeeSkill> EmployeeSkills { get; set; }
            = new List<EmployeeSkill>();
    }
}
