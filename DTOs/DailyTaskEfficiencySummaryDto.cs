namespace EmployeeManagement.Api.DTOs
{
    public class DailyTaskEfficiencySummaryDto
    {
        public decimal TotalPlannedHours { get; set; }
        public decimal TotalActualHours { get; set; }
        public decimal OverallEfficiency { get; set; }
    }
}
