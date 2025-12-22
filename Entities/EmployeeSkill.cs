namespace EmployeeManagement.Api.Entities
{
    public class EmployeeSkill
    {
        public long EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public long SkillId { get; set; }
        public Skill? Skill { get; set; }
    }
}
