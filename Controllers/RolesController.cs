using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly RolePermissionService _rp;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;

        public RolesController(
            RoleManager<AppRole> roleManager,
            RolePermissionService rp,
            UserManager<AppUser> userManager,
            AppDbContext db
        )
        {
            _roleManager = roleManager;
            _rp = rp;
            _userManager = userManager;
            _db = db;
        }

        // ------------------------------------------------------
        // GET PAGED ROLES (UI LIST)
        // ------------------------------------------------------
        [RequirePermission("role.view")]
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            int page = 1,
            int pageSize = 10,
            string? search = null)
        {
            var query = _roleManager.Roles.AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r => r.Name!.ToLower().Contains(s));
            }

            // COUNT
            var totalItems = query.Count();

            // PAGINATION
            var items = query
                .OrderBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Name
                })
                .ToList();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = items
            });
        }


        // ------------------------------------------------------
        // CREATE NEW ROLE
        // ------------------------------------------------------
        [RequirePermission("role.create")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            if (await _roleManager.RoleExistsAsync(dto.Name))
                return BadRequest(new { message = "Role already exists" });

            var role = new AppRole { Name = dto.Name };
            await _roleManager.CreateAsync(role);

            return Ok(role);
        }

        // ------------------------------------------------------
        // DELETE ROLE
        // ------------------------------------------------------
        [RequirePermission("role.delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound("Role not found");

            // prevent deleting System Admin
            if (role.Name == "Admin")
                return BadRequest("Cannot delete Admin role");

            // 🛑 Check if role is assigned to any user
            var usersInRole = await _db.UserRoles.AnyAsync(ur => ur.RoleId == id);
            if (usersInRole)
                return BadRequest("This role is assigned to user(s). Remove/change user role first.");

            // delete role-permission mappings
            var maps = _db.RolePermissions.Where(r => r.RoleId == id);
            _db.RolePermissions.RemoveRange(maps);
            await _db.SaveChangesAsync();

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest("Cannot delete role");

            return Ok(new { message = "Role deleted successfully" });
        }


        // ------------------------------------------------------
        // GET ALL PERMISSIONS (UI checkbox list)
        // ------------------------------------------------------
        [RequirePermission("role.manage")]
        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            var perms = await _rp.GetPermissions();
            return Ok(perms);
        }

        // ------------------------------------------------------
        // CREATE PERMISSION (rarely used)
        // ------------------------------------------------------
        [RequirePermission("role.manage")]
        [HttpPost("permission")]
        public async Task<IActionResult> CreatePermission(PermissionDto dto)
        {
            var p = await _rp.CreatePermission(dto.Name, dto.Description);
            return Ok(p);
        }

        // ------------------------------------------------------
        // ASSIGN PERMISSIONS TO ROLE
        // ------------------------------------------------------
        [RequirePermission("role.manage")]
        [HttpPost("assign-permissions")]
        public async Task<IActionResult> AssignPermissions(AssignPermissionsDto dto)
        {
            await _rp.AssignPermissions(dto.RoleId, dto.PermissionIds);

            return Ok(new
            {
                success = true,
                message = "Permissions assigned successfully"
            });
        }

        [RequirePermission("role.manage")]
        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissions(long roleId)
        {
            var perms = await _rp.GetRolePermissions(roleId);
            return Ok(perms);
        }

        [HttpGet("my-permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Unauthorized("Invalid token");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);

            var roleIds = _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToList();

            var permissions = await _db.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission!.Name)
                .Distinct()
                .ToListAsync();

            return Ok(permissions);
        }
    }
}
