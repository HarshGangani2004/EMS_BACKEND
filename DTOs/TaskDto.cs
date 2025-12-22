namespace EmployeeManagement.Api.DTOs
{
    public class TaskDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public long AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
