using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly JwtService _jwtService;

        public AuthService(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            JwtService jwtService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        // ================================================================
        // REGISTER USER (for public register)
        // ================================================================
        public async Task<string?> Register(RegisterDto dto)
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists != null) return "User already exists";

            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return "User creation failed";

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new AppRole { Name = "User" });

            await _userManager.AddToRoleAsync(user, "User");

            return null;
        }

        // ================================================================
        // LOGIN
        // ================================================================
        public async Task<string?> Login(LoginDto dto)
        {
            // CASE-SENSITIVE EMAIL CHECK
            var user = await _context.Users
                .FirstOrDefaultAsync(x => EF.Functions.Collate(x.Email,"SQL_Latin1_General_CP1_CS_AS") == dto.Email);

            if (user == null)
                return null;

            // Password is already case-sensitive
            bool valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            return _jwtService.GenerateToken(user, roles);
        }
    

    }
}
