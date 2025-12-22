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
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _service;


        public SkillsController(ISkillService service)
        {
            _service = service;
        }
      
        [RequirePermission("skill.view")]
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10, string? search = null)
        {
            var result = await _service.GetPaged(page, pageSize, search, User);
            return Ok(result);
        }

        [RequirePermission("skill.view")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var skill = await _service.GetById(id);
            if (skill == null) return NotFound();

            return Ok(skill);
        }

        [RequirePermission("skill.create")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateSkillDto dto)
        {

            try
            {
                string username = User.Identity.Name;
                var created = await _service.Create(dto, username, User);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [RequirePermission("skill.edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CreateSkillDto dto)
        {
            var result = await _service.Update(id, dto);
            if (!result) return NotFound();

            return Ok(new { message = "updated successfully" });
        }

        [RequirePermission("skill.delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {

            try
            {
                var result = await _service.Delete(id);
                if (!result) return NotFound();

                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
