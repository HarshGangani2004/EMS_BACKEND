using System;

namespace EmployeeManagement.Api.DTOs
{
    public class LeaveListDto
    {
        public long Id { get; set; }

        public string UserName { get; set; } = null!;

        public string LeaveType { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string Status { get; set; } = null!;
        public string? Reason { get; set; }

    }
}
