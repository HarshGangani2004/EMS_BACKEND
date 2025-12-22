namespace EmployeeManagement.Api.DTOs
{
    public class ProjectDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CreatedBy { get; set; }
    }

}
