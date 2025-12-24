namespace EmployeeManagement.Api.DTOs
{
    public class LeaveFilterDto
    {
        public int? Year { get; set; }        // 2024, 2025
        public int? Month { get; set; }       // 1 - 12
        public string? Status { get; set; }   // Pending / Approved / Rejected
        public string? LeaveType { get; set; } // CL / SL / PL
        public string? UserName { get; set; } // HR only
    }
}
