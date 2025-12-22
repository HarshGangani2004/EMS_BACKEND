using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.DTOs
{
    public class UpdateLeaveDto
    {

        public string LeaveType { get; set; } = string.Empty;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
