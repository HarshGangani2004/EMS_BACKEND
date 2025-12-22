using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Api.Data;
using System;
using System.Collections.Generic;

namespace EmployeeManagement.Api.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var db = context.RequestServices.GetRequiredService<AppDbContext>();

                Console.WriteLine("PermissionMiddleware: running. IsAuthenticated=" + (context.User?.Identity?.IsAuthenticated ?? false));

                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var identifier = context.User.Identity?.Name
                                     ?? context.User.FindFirst(ClaimTypes.Email)?.Value
                                     ?? context.User.FindFirst("email")?.Value
                                     ?? context.User.FindFirst("preferred_username")?.Value
                                     ?? context.User.FindFirst("unique_name")?.Value
                                     ?? context.User.FindFirst("sub")?.Value;

                    Console.WriteLine("PermissionMiddleware: identifier=" + (identifier ?? "<null>"));

                    if (!string.IsNullOrEmpty(identifier))
                    {
                        // try find user by email/username/sub depending on your token
                        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == identifier || u.FullName == identifier);

                        if (user != null)
                        {
                            // Get role ids from Identity's UserRoles table
                            var roleIds = await db.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Select(ur => ur.RoleId)
                                .ToListAsync();

                            // Query permissions for those roleIds
                            var permissions = await (
                                from rp in db.RolePermissions
                                join p in db.Permissions on rp.PermissionId equals p.Id
                                where roleIds.Contains(rp.RoleId)
                                select p.Name
                            ).ToListAsync();

                            context.Items["Permissions"] = permissions ?? new List<string>();
                            Console.WriteLine("PermissionMiddleware: loaded permissions count = " + (permissions?.Count ?? 0));
                        }
                        else
                        {
                            Console.WriteLine("PermissionMiddleware: user not found for identifier");
                            context.Items["Permissions"] = new List<string>();
                        }
                    }
                    else
                    {
                        Console.WriteLine("PermissionMiddleware: identifier claim not present in token");
                        context.Items["Permissions"] = new List<string>();
                    }
                }
                else
                {
                    // not authenticated -> no permissions
                    context.Items["Permissions"] = new List<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PermissionMiddleware exception: " + ex);
                context.Items["Permissions"] = new List<string>();
            }

            await _next(context);
        }
    }
}
