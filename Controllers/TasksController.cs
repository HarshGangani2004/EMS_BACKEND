using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _service;

        public TasksController(ITaskService service) => _service = service;

        [HttpPost]
        [RequirePermission("task.create")]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var username = User?.Identity?.Name ?? "system";
            try
            {
                var res = await _service.Create(dto, username);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("paged")]
        [RequirePermission("task.view")]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10, string? search = null, long? assignedTo = null)
        {
            var res = await _service.GetPaged(page, pageSize, search, assignedTo);
            return Ok(res);
        }

        [HttpGet("{id:long}")]
        [RequirePermission("task.view")]
        public async Task<IActionResult> GetById(long id)
        {
            var res = await _service.GetById(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        [HttpPut("{id:long}")]
        [RequirePermission("task.edit")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateTaskDto dto)
        {
            var username = User?.Identity?.Name ?? "system";
            try
            {
                var ok = await _service.Update(id, dto, username);
                if (!ok) return NotFound();
                return Ok(true);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] string status)
        {
            var username = User?.Identity?.Name ?? "system";
            var ok = await _service.UpdateStatus(id, status, username);
            if (!ok) return NotFound();
            return Ok(true);
        }

        [HttpDelete("{id:long}")]
        [RequirePermission("task.delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var ok = await _service.Delete(id);
            if (!ok) return NotFound();
            return Ok(true);
        }
    }
}
