using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AdminService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ======================= GET PAGED USERS ===============================
        public async Task<Interfaces.PagedResult<UserDto>> GetUsersPaged(int page, int pageSize, string? search)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(u =>
                    u.Email!.ToLower().Contains(s) ||
                    u.FullName!.ToLower().Contains(s));
            }

            var totalItems = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var resultList = new List<UserDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                resultList.Add(new UserDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FullName = u.FullName ?? "",
                    Roles = roles.ToList()
                });
            }

            return new Interfaces.PagedResult<UserDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = resultList
            };
        }

        // ======================= GET SINGLE USER ===============================
        public async Task<UserDto?> GetUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName ?? "",
                Roles = roles.ToList()
            };
        }

        // ======================= CREATE USER ===============================
        public async Task<string?> CreateUser(RegisterDto dto)
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists != null)
                return "User already exists";

            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return "Failed to create user";

            string role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new AppRole { Name = role });

            await _userManager.AddToRoleAsync(user, role);

            return null; // success
        }

        // ======================= CHANGE ROLE ===============================
        public async Task<string?> ChangeRole(ChangeUserRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return "User not found";

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return "Role does not exist";

            var oldRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, oldRoles);

            var result = await _userManager.AddToRoleAsync(user, dto.Role);

            if (!result.Succeeded)
                return "Failed to update role";

            return null; // success
        }

        // ======================= DELETE USER ===============================
        public async Task<string?> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return "User not found";
            if(user.Email!.ToLower() == "admin@ems.com")
                return "Cannot delete default admin user";

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return "Failed to delete user";

            return null;
        }
    }
}
