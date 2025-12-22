using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeManagement.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            Console.WriteLine($"RequirePermission: checking '{_permission}'");

            // If not authenticated -> forbid
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                Console.WriteLine("RequirePermission: user not authenticated");
                context.Result = new ForbidResult();
                return;
            }

            var userPermissions = context.HttpContext.Items["Permissions"] as List<string>;

            Console.WriteLine("RequirePermission: permissions present? " + (userPermissions != null) +
                              " count=" + (userPermissions?.Count.ToString() ?? "0"));

            if (userPermissions == null ||
                !userPermissions.Contains(_permission, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"RequirePermission: permission denied for '{_permission}'");
                context.Result = new ForbidResult();
            }
            else
            {
                Console.WriteLine($"RequirePermission: allowed for '{_permission}'");
            }
        }
    }
}
