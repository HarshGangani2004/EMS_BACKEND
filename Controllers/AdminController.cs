using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        // ======================= GET PAGED USERS ===============================
        [RequirePermission("user.view")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsersPaged(int page = 1, int pageSize = 10, string? search = null)
        {
            var result = await _service.GetUsersPaged(page, pageSize, search);
            return Ok(result);
        }

        // ======================= GET SINGLE USER ===============================
        [HttpGet("user")]
        public async Task<IActionResult> GetUser([FromQuery] string email)
        {
            var result = await _service.GetUser(email);
            if (result == null)
                return NotFound("User not found");

            return Ok(result);
        }

        // ======================= CREATE USER ===============================
        [RequirePermission("user.create")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(RegisterDto dto)
        {
            var error = await _service.CreateUser(dto);
            if (error != null)
                return BadRequest(error);

            return Ok(new { message = "created successfully" });
        }

        // ======================= CHANGE ROLE ===============================
        [RequirePermission("user.edit")]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeRole(ChangeUserRoleDto dto)
        {
            var error = await _service.ChangeRole(dto);
            if (error != null)
                return BadRequest(error);

            return Ok(new { message = "updated successfully" });
        }

        // ======================= DELETE USER ===============================
        [RequirePermission("user.delete")]
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser([FromQuery] string email)
        {
            var error = await _service.DeleteUser(email);
            if (error != null)
                return BadRequest(error);

            return Ok(new { message = "deleted successfully" });
        }
    }
}
