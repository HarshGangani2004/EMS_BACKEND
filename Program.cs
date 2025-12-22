using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using EmployeeManagement.Api.Middleware;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Optional: bind explicit URL so logs show the expected address (useful in dev)
builder.WebHost.UseUrls("http://localhost:5093");

// ------------------------------
// DATABASE
// ------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ------------------------------
// IDENTITY
// ------------------------------
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ------------------------------
// JWT AUTH
// ------------------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});

// ------------------------------
// CORS
// ------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin());
});

// ------------------------------
// CUSTOM SERVICES
// ------------------------------
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RolePermissionService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

// ------------------------------
// MVC
// ------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Enter JWT like: Bearer <your_token>",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
        });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ------------------------------
// SWAGGER
// ------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<PermissionMiddleware>();

app.MapControllers();

await CreateDefaultAdmin(app);

app.Run();



async Task CreateDefaultAdmin(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 1️⃣ Create Default Roles
    string[] roles = { "Admin", "User", "Manager" };
    foreach (var r in roles)
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new AppRole { Name = r });


    // 2️⃣ Create Default Admin User
    var adminEmail = "admin@ems.com";
    var adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            FullName = "Admin",
            Email = adminEmail,
            UserName = adminEmail
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // 3️⃣ Create Permissions
    var permissionList = new string[]
    {
        "admin","employee.view", "employee.create", "employee.edit", "employee.delete",
        "department.view", "department.create", "department.edit", "department.delete",
        "skill.view", "skill.create", "skill.edit", "skill.delete",
        "user.view", "user.create", "user.edit", "user.delete",
        "role.view", "role.create", "role.manage", "role.delete",
        "project.view","project.create","project.edit","project.delete",
        "task.view","task.create","task.edit","task.delete","salary",
        "leave.create","leave.view","leave.view.all","leave.update.status",
        "leave.delete","leave.update"

    };


    foreach (var perm in permissionList)
    {
        if (!db.Permissions.Any(p => p.Name == perm))
        {
            db.Permissions.Add(new Permission
            {
                Name = perm,
                Description = perm,
                CreatedBy = "Admin"
            });
        }
    }

    await db.SaveChangesAsync();

    // 4️⃣ Assign All Permissions To Admin
    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (adminRole != null)
    {
        var allPermissions = db.Permissions.ToList();

        var oldMap = db.RolePermissions.Where(x => x.RoleId == adminRole.Id);
        db.RolePermissions.RemoveRange(oldMap);

        foreach (var p in allPermissions)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id,
               
            });
        }

        await db.SaveChangesAsync();
    }

    // 5️⃣ DEFAULT DEPARTMENTS
    string[] defaultDepartments = { "IT", "HR", "Finance", "Marketing" };

    foreach (var dep in defaultDepartments)
        if (!db.Departments.Any(x => x.Name == dep))
            db.Departments.Add(new Department { Name = dep });

    // 6️⃣ DEFAULT SKILLS
    string[] defaultSkills = { "Angular", "React", "SQL", "JavaScript", "C#" };

    foreach (var skill in defaultSkills)
        if (!db.Skills.Any(x => x.Name == skill))
            db.Skills.Add(new Skill { Name = skill });

    await db.SaveChangesAsync();
}


// ------------------------------
