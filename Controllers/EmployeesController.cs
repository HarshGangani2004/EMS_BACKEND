using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
      
            _service = service;
        }
            [Authorize]
            [RequirePermission("employee.view")]
            [HttpGet]
            public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 10,
            string? search = null)
            {
                var result = await _service.GetPaged(page, pageSize, search, User);
                return Ok(result);
            }

            [Authorize]
            [RequirePermission("employee.view")]
            [HttpGet("{id}")]
            public async Task<IActionResult> Get(long id)
            {
                var emp = await _service.GetById(id);
                if (emp == null) return NotFound();

                return Ok(emp);
            }
            [Authorize]
            [RequirePermission("employee.create")]
            [HttpPost]
            public async Task<IActionResult> Create(CreateEmployeeDto dto)
            {
                string username = User.Identity.Name;
                var emp = await _service.Create(dto, username, User);
                return Ok(emp);
            }
            [Authorize]
            [RequirePermission("employee.edit")]
            [HttpPut("{id}")]
            public async Task<IActionResult> Update(long id, UpdateEmployeeDto dto)
            {
                var updated = await _service.Update(id, dto);
                if (!updated) return NotFound();

                return Ok(new { message = "updated" });
            }
            [Authorize]
            [RequirePermission("employee.delete")]
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(long id)
            {
                var deleted = await _service.Delete(id);
                if (!deleted) return NotFound();

                return Ok(new { message = "deleted" });
            }


        }
    } 
