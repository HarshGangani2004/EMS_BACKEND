namespace EmployeeManagement.Api.DTOs
{
    public class CreateTaskDto
    {
        public long ProjectId { get; set; }
        public long AssignedTo { get; set; }
        public string Status { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
    }
}
