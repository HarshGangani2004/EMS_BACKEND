using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeManagement.Api.Services
{
    public class SkillService : ISkillService
    {
        private readonly AppDbContext _context;

        public SkillService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SkillDto>> GetAll()
        {
            return await _context.Skills
                .Select(x => new SkillDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }
        public async Task<Interfaces.PagedResult<SkillDto>> GetPaged(int page, int pageSize, string? search, ClaimsPrincipal user)
        {
            var query = _context.Skills.AsQueryable();
           
            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            // COUNT
            var totalItems = await query.CountAsync();

            // PAGING
            var items = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SkillDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedBy = user.IsInRole("Admin") ? x.CreatedBy : null   
                })
                .ToListAsync();

            return new Interfaces.PagedResult<SkillDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = items
            };
        }

        public async Task<SkillDto?> GetById(long id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return null;

            return new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            };
        }

        public async Task<SkillDto> Create(CreateSkillDto dto, string u, ClaimsPrincipal user)
        {
            bool nameExists;

            if (!user.IsInRole("Admin"))
            {
                nameExists= await _context.Skills.AnyAsync(x => x.Name == dto.Name && x.CreatedBy == u);

            }
            else
            {
                nameExists= await _context.Skills.AnyAsync(x => x.Name == dto.Name);
            }

            if (nameExists)
             throw new Exception("Skill name already exists"); 
          

            var skill = new Skill
            {
                Name = dto.Name,
                CreatedBy = u
            };

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            return new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            };
        }

        public async Task<bool> Update(long id, CreateSkillDto dto)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            skill.Name = dto.Name;
            skill.LastModifiedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(long id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            // ❗ Check if skill is used by any employee
            bool inUse = await _context.EmployeeSkills
                             .AnyAsync(x => x.SkillId == id);

            if (inUse)
                throw new Exception("This skill is assigned to employee(s). Remove or update employee first.");

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
