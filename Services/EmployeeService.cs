using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeManagement.Api.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------- GET ALL ----------------------
        public async Task<Interfaces.PagedResult<EmployeeDto>> GetPaged(
        int page,
        int pageSize,
        string? search,
        ClaimsPrincipal user)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.EmployeeSkills)
                    .ThenInclude(es => es.Skill)
                .AsQueryable();

            // ROLE-BASED FILTER
       

            // SEARCH FILTER
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(s) ||
                    e.Email.ToLower().Contains(s)
                );
            }

            // TOTAL COUNT
            var totalItems = await query.CountAsync();

            // PAGINATION + PROJECTION
            var items = await query
                .OrderBy(e => e.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    Salary = e.Salary,
                    JoinedOn = e.JoinedOn,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department.Name,
                    SkillIds = e.EmployeeSkills.Select(x => x.SkillId).ToList(),
                    Skills = e.EmployeeSkills.Select(x => x.Skill.Name).ToList(),
                    CreatedBy = user.IsInRole("Admin") ? e.CreatedBy : null
                })
                .ToListAsync();

            // RETURN PAGED RESULT
            return new Interfaces.PagedResult<EmployeeDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }





        // ---------------------- PAGINATION + SEARCH ----------------------


        // ---------------------- GET BY ID ----------------------
        public async Task<EmployeeDto?> GetById(long id)
        {
            var e = await _context.Employees
                .Include(x => x.Department)
                .Include(x => x.EmployeeSkills)
                    .ThenInclude(es => es.Skill)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null) return null;

            return new EmployeeDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Salary = e.Salary,
                JoinedOn = e.JoinedOn,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.Name,
                SkillIds = e.EmployeeSkills.Select(x => x.SkillId).ToList(),
                Skills = e.EmployeeSkills.Select(x => x.Skill.Name).ToList()
            };
        }

        // ---------------------- CREATE ----------------------
        public async Task<EmployeeDto> Create(CreateEmployeeDto dto, string u, ClaimsPrincipal user)
        {
            bool nameExists;
         

            if(!user.IsInRole("Admin"))
            {
                nameExists = await _context.Employees.AnyAsync(x => x.Email == dto.Email && x.CreatedBy== u);
                
            }
            else {
                nameExists = await _context.Employees.AnyAsync(x => x.Email == dto.Email);
                
            }
            if (nameExists)
                throw new Exception("Email already exists");

   

            var emp = new Employee
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Salary = dto.Salary,
                JoinedOn = dto.JoinedOn,
                DepartmentId = dto.DepartmentId,
                CreatedBy = u
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            foreach (var skillId in dto.SkillIds)
            {
                _context.EmployeeSkills.Add(new EmployeeSkill
                {
                    EmployeeId = emp.Id,
                    SkillId = skillId
                });
            }

            await _context.SaveChangesAsync();

            return await GetById(emp.Id);
        }

        // ---------------------- UPDATE ----------------------
        public async Task<bool> Update(long id, UpdateEmployeeDto dto)
        {

            var emp = await _context.Employees
                .Include(x => x.EmployeeSkills)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (emp == null) return false;

            emp.FullName = dto.FullName;
            emp.Email = dto.Email;
            emp.Salary = dto.Salary;
            emp.JoinedOn = dto.JoinedOn;
            emp.DepartmentId = dto.DepartmentId;
            emp.LastModifiedAt = DateTimeOffset.UtcNow;

            _context.EmployeeSkills.RemoveRange(emp.EmployeeSkills);

            foreach (var skillId in dto.SkillIds)
            {
                _context.EmployeeSkills.Add(new EmployeeSkill
                {
                    EmployeeId = emp.Id,
                    SkillId = skillId
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ---------------------- DELETE ----------------------
        public async Task<bool> Delete(long id)
        {
            var emp = await _context.Employees
                .Include(x => x.EmployeeSkills)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (emp == null) return false;

            _context.EmployeeSkills.RemoveRange(emp.EmployeeSkills);
            _context.Employees.Remove(emp);

            await _context.SaveChangesAsync();

            return true;
        }

        
    }
}
