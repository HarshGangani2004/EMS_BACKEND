namespace EmployeeManagement.Api.Entities
{
    public class TaskItem : BaseEntity
    {
        public long ProjectId { get; set; }
        public Project? Project { get; set; }

        public long AssignedTo { get; set; } // Employee Id
        public Employee? Employee { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string Status { get; set; } = "Pending";  // Pending, In Progress, Completed
        public string? Priority { get; set; } = "Medium"; // Low, Medium, High

        public DateTime? DueDate { get; set; }
    }
}
