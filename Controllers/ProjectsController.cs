using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectsController(IProjectService service) => _service = service;

        [HttpPost]
        [RequirePermission("project.create")]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            var username = User?.Identity?.Name ?? "system";
            var result = await _service.Create(dto, username);
            return Ok(result);
        }

        [HttpGet("paged")]
        [RequirePermission("project.view")]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10, string? search = null)
        {
            var res = await _service.GetPaged(page, pageSize, search);
            return Ok(res);
        }
        [RequirePermission("project.view")]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var res = await _service.GetById(id);
            if (res == null) return NotFound();
            return Ok(res);
        }
        [RequirePermission("project.edit")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateProjectDto dto)
        {
            var username = User?.Identity?.Name ?? "system";
            var ok = await _service.Update(id, dto, username);
            if (!ok) return NotFound();
            return Ok(true);
        }

        [HttpDelete("{id:long}")]
        [RequirePermission("project.delete")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var ok = await _service.Delete(id);
                if (!ok) return NotFound();
                return Ok(true);
            }
            catch (InvalidOperationException ex)
            {
                // business rule violation (e.g. cannot delete project with tasks)
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
