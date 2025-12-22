using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        // ================= CREATE =================
        public async Task<TaskDto> Create(CreateTaskDto dto, string username)
        {
            // Validate Project
            var project = await _db.Projects.FindAsync(dto.ProjectId);
            if (project == null)
                throw new KeyNotFoundException("Project not found.");

            // Validate Employee
            var employee = await _db.Employees.FindAsync(dto.AssignedTo);
            if (employee == null)
                throw new KeyNotFoundException("Assigned employee not found.");

            var entity = new TaskItem
            {
                ProjectId = dto.ProjectId,
                AssignedTo = dto.AssignedTo,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority ?? "Medium",
                DueDate = dto.DueDate,
                CreatedBy = username,
                Status = "Pending"
            };

            _db.Tasks.Add(entity);
            await _db.SaveChangesAsync();

            // ⭐ LOAD REQUIRED NAVIGATION PROPERTIES
            await _db.Entry(entity).Reference(x => x.Employee).LoadAsync();
            await _db.Entry(entity).Reference(x => x.Project).LoadAsync();

            return MapToDto(entity);
        }

        // ================= GET BY ID =================
        public async Task<TaskDto?> GetById(long id)
        {
            var t = await _db.Tasks
                .Include(x => x.Employee)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (t == null) return null;

            return MapToDto(t);
        }

        // ================= GET PAGED =================
        public async Task<Interfaces.PagedResult<TaskDto>> GetPaged(
            int page,
            int pageSize,
            string? search,
            long? assignedTo)
        {
            var query = _db.Tasks
                .Include(x => x.Employee)
                .Include(x => x.Project)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.Description ?? "").Contains(search));
            }

            if (assignedTo.HasValue)
            {
                query = query.Where(x => x.AssignedTo == assignedTo.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Interfaces.PagedResult<TaskDto>
            {
                Items = items.Select(MapToDto).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        // ================= UPDATE =================
        public async Task<bool> Update(long id, UpdateTaskDto dto, string username)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return false;

            if (t.ProjectId != dto.ProjectId)
            {
                if (!await _db.Projects.AnyAsync(x => x.Id == dto.ProjectId))
                    throw new KeyNotFoundException("Project not found.");
            }

            if (t.AssignedTo != dto.AssignedTo)
            {
                if (!await _db.Employees.AnyAsync(x => x.Id == dto.AssignedTo))
                    throw new KeyNotFoundException("Assigned employee not found.");
            }

            t.ProjectId = dto.ProjectId;
            t.AssignedTo = dto.AssignedTo;
            t.Title = dto.Title;
            t.Description = dto.Description;
            t.Status = dto.Status;
            t.Priority = dto.Priority ?? "Medium";
            t.DueDate = dto.DueDate;

            await _db.SaveChangesAsync();
            return true;
        }

        // ================= DELETE =================
        public async Task<bool> Delete(long id)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return false;

            _db.Tasks.Remove(t);
            await _db.SaveChangesAsync();
            return true;
        }

        // ================= UPDATE STATUS =================
        public async Task<bool> UpdateStatus(long id, string status, string username)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return false;

            t.Status = status;
            await _db.SaveChangesAsync();
            return true;
        }

        // ================= DTO MAPPER =================
        private static TaskDto MapToDto(TaskItem t) => new TaskDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name,
            AssignedTo = t.AssignedTo,
            AssignedToName = t.Employee?.FullName,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            DueDate = t.DueDate,
            CreatedBy = t.CreatedBy
        };
    }
}
