using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;

        public ProjectService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ProjectDto> Create(CreateProjectDto dto, string username)
        {
            var entity = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedBy = username
            };

            _db.Projects.Add(entity);
            await _db.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<ProjectDto?> GetById(long id)
        {
            var p = await _db.Projects.FindAsync(id);
            if (p == null) return null;
            return MapToDto(p);
        }

        public async Task<Interfaces.PagedResult<ProjectDto>> GetPaged(int page, int pageSize, string? search)
        {
            var query = _db.Projects.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Name.Contains(search));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtoItems = items.Select(MapToDto).ToList();

            return new Interfaces.PagedResult<ProjectDto>
            {
                Items = dtoItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<bool> Update(long id, UpdateProjectDto dto, string username)
        {
            var p = await _db.Projects.FindAsync(id);
            if (p == null) return false;

            p.Name = dto.Name;
            p.Description = dto.Description;
            p.StartDate = dto.StartDate;
            p.EndDate = dto.EndDate;
            // you can add UpdatedBy / UpdatedAt if in BaseEntity

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(long id)
        {
            var p = await _db.Projects.FindAsync(id);
            if (p == null) return false;

            // Optional: prevent delete if tasks exist
            var anyTasks = await _db.Tasks.AnyAsync(t => t.ProjectId == id);
            if (anyTasks)
            {
                // If you want to allow cascade delete, configure EF; here we block deletion if tasks exist
                throw new InvalidOperationException("Cannot delete project with existing tasks.");
            }

            _db.Projects.Remove(p);
            await _db.SaveChangesAsync();
            return true;
        }

        private static ProjectDto MapToDto(Project p) => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            CreatedBy = p.CreatedBy
        };
    }
}
