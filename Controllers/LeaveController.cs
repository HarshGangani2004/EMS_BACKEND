using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _service;

        public LeaveController(ILeaveService service)
        {
            _service = service;
        }

        // =========================
        // APPLY LEAVE (ALL ROLES)
        // =========================
        [HttpPost("apply")]
        [RequirePermission("leave.create")]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveDto dto)
        {
            if (dto.FromDate > dto.ToDate)
                return BadRequest("From date cannot be greater than To date");

            // ✅ SAFE USER IDENTIFIER
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            await _service.ApplyLeaveAsync(dto, userName);
            return Ok(new { message = "Leave applied successfully" });
        }

        // =========================
        // USER: SEE OWN LEAVES
        // =========================
        [HttpGet("my")]
        [RequirePermission("leave.view")]
        public async Task<IActionResult> GetMyLeavesPaged(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            long userId = long.Parse(userIdClaim);

            var result = await _service.GetMyLeavesPagedAsync(
                userId, page, pageSize, search);

            return Ok(result);
        }
        // =========================
        // ADMIN / HR: SEE ALL LEAVES
        // =========================
        [HttpGet("all")]
        [RequirePermission("leave.view.all")]
        public async Task<IActionResult> GetAllLeavesPaged(
                int page = 1,
                int pageSize = 10,
                string? search = null)
        {
            var result = await _service.GetAllLeavesPagedAsync(
                page, pageSize, search);

            return Ok(result);
        }

        // =========================
        // 🔥 GET LEAVE BY ID (VIEW)
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var leave = await _service.GetByIdAsync(id);
            if (leave == null)
                return NotFound("Leave not found");

            return Ok(leave);
        }

        // =========================
        // 🔥 GET PENDING LEAVES
        // =========================
        [HttpGet("pending")]
        [RequirePermission("leave.view.all")]
        public async Task<IActionResult> GetPendingLeaves()
        {
            var leaves = await _service.GetPendingLeavesAsync();
            return Ok(leaves);
        }

        // =========================
        // 🔥 APPROVE / REJECT LEAVE
        // =========================
        [HttpPut("{id}/status")]
        [RequirePermission("leave.update.status")]
        public async Task<IActionResult> UpdateStatus(
            long id,
            [FromBody] UpdateLeaveStatusDto dto)
        {

            var actionBy = User.Identity?.Name ?? "System";

            await _service.UpdateStatusAsync(id, dto.Status, actionBy);
            return Ok(new { message = "Leave status updated successfully" });
        }

         // =========================
        // 🔥 DELETE OWN LEAVE (ONLY PENDING)
        // =========================
        [HttpDelete("{id}")]
        [RequirePermission("leave.delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            long userId = long.Parse(userIdClaim);

            await _service.DeleteLeaveAsync(id, userId);
            return Ok(new { message = "Leave deleted successfully" });
        }
        [HttpPut("{id}")]
        [RequirePermission("leave.update")]
        public async Task<IActionResult> Update(
            long id,
            [FromBody] UpdateLeaveDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            long userId = long.Parse(userIdClaim);

            await _service.UpdateLeaveAsync(id, userId, dto);
            return Ok(new { message = "Leave updated successfully" });
        }

    }
}
