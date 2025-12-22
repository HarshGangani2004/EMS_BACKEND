namespace EmployeeManagement.Api.DTOs
{
    public class SkillDto
    {

        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? CreatedBy { get; set; }
    }

}
