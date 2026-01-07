using System.Security.Claims;
using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/Timesheet")]
    [Authorize]
    public class DailyTaskLogsController : ControllerBase
    {
        private readonly IDailyTaskLogService _service;

        public DailyTaskLogsController(IDailyTaskLogService service)
        {
            _service = service;
        }

        // =========================
        // CREATE TIMESHEET
        // =========================

        [HttpPost]
        [RequirePermission("timesheet.create")]
        public async Task<IActionResult> Create(CreateDailyTaskLogDto dto)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userName = User.Identity!.Name!;

            await _service.CreateAsync(dto, userId, userName);
            return Ok(new { message = "Timesheet created successfully" });
        }

        // =========================
        // MY TIMESHEETS
        // =========================
        [HttpGet("MyTS")]
        [RequirePermission("timesheet.view")]
        public async Task<IActionResult> MyTimesheets(
            [FromQuery] DailyTaskLogFilterDto filter,
            int page = 1,
            int pageSize = 10)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _service.GetMyLogsPagedAsync(
                userId, page, pageSize, filter);

            return Ok(result);
        }

        // =========================
        // ALL TIMESHEETS (ADMIN / HR)
        // =========================
        [HttpGet("AllTs")]
        [RequirePermission("timesheet.view.all")]
        public async Task<IActionResult> AllTimesheets(
            [FromQuery] DailyTaskLogFilterDto filter,
            int page = 1,
            int pageSize = 10)
        {
            var result = await _service.GetAllLogsPagedAsync(
                page, pageSize, filter);

            return Ok(result);
        }

        // =========================
        // GET BY ID (VIEW)
        // =========================
        [HttpGet("{id}")]
        [RequirePermission("timesheet.view")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Timesheet not found" });

            return Ok(result);
        }

        // =========================
        // UPDATE (OWNER ONLY)
        // =========================
        [HttpPut("{id}")]
        [RequirePermission("timesheet.update")]
        public async Task<IActionResult> Update(
            long id,
            CreateDailyTaskLogDto dto)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _service.UpdateAsync(id, userId, dto);
            return Ok(new { message = "Timesheet updated successfully" });
        }

        // =========================
        // DELETE (OWNER ONLY)
        // =========================
        [HttpDelete("{id}")]
        [RequirePermission("timesheet.delete")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Timesheet deleted successfully" });
        }

        // =========================
        // DASHBOARD – MY EFFICIENCY
        // =========================
        [HttpGet("dashboard/my")]
        public async Task<IActionResult> MyEfficiency()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _service.GetEfficiencySummaryAsync(
                userId, false);

            return Ok(result);
        }

        // =========================
        // DASHBOARD – ALL EFFICIENCY
        // =========================
        [HttpGet("dashboard/all")]
        public async Task<IActionResult> AllEfficiency()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _service.GetEfficiencySummaryAsync(
                userId, true);

            return Ok(result);
        }
    }
}
