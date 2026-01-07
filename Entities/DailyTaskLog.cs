using EmployeeManagement.Api.Entities;

namespace EmployeeManagement.Api.Entities
{
    public class DailyTaskLog : BaseEntity
    {
        public long ProjectId { get; set; }
        public Project? Project { get; set; }

        // Logged in user
        public long UserId { get; set; }
        public string UserName { get; set; } = "";

        // Work info
        public string Title { get; set; } = string.Empty;   // Work Title
        public string? Description { get; set; }            // Only for view/edit

        // Time
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal PlannedHours { get; set; }
        public string Status { get; set; } = "In Progress";
    }
}