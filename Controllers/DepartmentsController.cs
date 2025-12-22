using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }
    
    [Authorize]
    [RequirePermission("department.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10, string? search = null)
    {
        var result = await _service.GetPaged(page, pageSize, search, User);
        return Ok(result);
    }

    [Authorize]
    [RequirePermission("department.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(long id)
    {
        var dept = await _service.GetById(id);
        if (dept == null) return NotFound();
        return Ok(dept);
    }

    [Authorize]
    [RequirePermission("department.create")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        try
        {
            string username = User.Identity.Name;
            var dept = await _service.Create(dto, username, User);
            return Ok(dept);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [Authorize]
    [RequirePermission("department.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, CreateDepartmentDto dto)
    {
        var updated = await _service.Update(id, dto);
        if (!updated) return NotFound();
        return Ok(new { message = "updated successfully" });
    }

    [Authorize]
    [RequirePermission("department.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.Delete(id);
        if (!deleted) return NotFound();
        return Ok(new { message = "Deleted successfully" });
    }
}
