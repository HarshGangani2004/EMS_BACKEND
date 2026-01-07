namespace EmployeeManagement.Api.DTOs
{
    public class DailyTaskLogListDto
    {
        public long Id { get; set; }
        public DateTime WorkDate { get; set; }

        public string ProjectName { get; set; } = "";
        public string WorkTitle { get; set; } = "";

        public string UserName { get; set; } = "";

        public decimal PlannedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal Efficiency { get; set; }

        public string Status { get; set; } = "";
    }
}
