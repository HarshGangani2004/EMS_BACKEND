using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class RolePermissionService
    {
        private readonly AppDbContext _db;
        private readonly RoleManager<AppRole> _roleManager;

        public RolePermissionService(AppDbContext db, RoleManager<AppRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
        }

        public async Task<List<Permission>> GetPermissions()
        {
            return await _db.Permissions.ToListAsync();
        }

        public async Task<Permission> CreatePermission(string name, string? desc)
        {
            var exist = await _db.Permissions.FirstOrDefaultAsync(x => x.Name == name);
            if (exist != null) return exist;

            var p = new Permission { Name = name, Description = desc };
            _db.Permissions.Add(p);
            await _db.SaveChangesAsync();
            return p;
        }

        public async Task AssignPermissions(long roleId, List<long> permissionIds)
        {
            // Remove old mapping
            var old = _db.RolePermissions.Where(x => x.RoleId == roleId);
            _db.RolePermissions.RemoveRange(old);

            // Add new mapping
            foreach (var pid in permissionIds.Distinct())
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = pid
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<Permission>> GetRolePermissions(long roleId)
        {
            return await _db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!)
                .ToListAsync();
        }
    }
}
