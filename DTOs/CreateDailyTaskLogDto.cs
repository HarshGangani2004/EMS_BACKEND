namespace EmployeeManagement.Api.DTOs
{
    public class CreateDailyTaskLogDto
    {
        public long ProjectId { get; set; }

        public string Title { get; set; } = "";
        public string? Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal PlannedHours { get; set; }
        public string Status { get; set; } = "In Progress";
    }
}
