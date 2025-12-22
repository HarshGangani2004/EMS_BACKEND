using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.Register(dto);

            if (result != null)
                return BadRequest(new { message = result });

            return Ok(new { message = "User registered successfully" });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _authService.Login(dto);

            if (token == null) return Unauthorized("Invalid login");

            return Ok(new { token });
        }
    }
}
