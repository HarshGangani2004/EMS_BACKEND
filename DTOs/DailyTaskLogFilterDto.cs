namespace EmployeeManagement.Api.DTOs
{
    public class DailyTaskLogFilterDto
    {
        public int? Year { get; set; }
        public int? Month { get; set; }

        public long? ProjectId { get; set; }
        public string? Status { get; set; }

        public string? Search { get; set; } // title + username
    }
}
