using System;

namespace EmployeeManagement.Api.DTOs
{
    public class ApplyLeaveDto
    {
        public long UserId { get; set; }
 
        public string LeaveType { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; } = null!;
    }
}
