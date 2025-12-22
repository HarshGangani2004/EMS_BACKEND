using System;

namespace EmployeeManagement.Api.Entities
{
    public class LeaveRequest : BaseEntity
    {
        // =========================
        // USER INFO (LOGIN USER)
        // =========================
        public long UserId { get; set; }          // AppUser.Id
        public string UserName { get; set; } = null!;

        // =========================
        // LEAVE DETAILS
        // =========================
        public string LeaveType { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; } = null!;

        // =========================
        // STATUS
        // =========================
        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected
    }
}
