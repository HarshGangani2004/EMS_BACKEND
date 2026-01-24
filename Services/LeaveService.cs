using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;

        public LeaveService(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // APPLY LEAVE (ALL ROLES)
        // =========================
        public async Task ApplyLeaveAsync(ApplyLeaveDto dto, string uc)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.FullName == uc);

            if (user == null)
                throw new Exception("User not found");

            var leave = new LeaveRequest
            {
                UserId = user.Id,
                UserName = user.FullName,
                LeaveType = dto.LeaveType,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                Reason = dto.Reason,
                Status = "Pending"
            };

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();
        }

        // =========================
        // USER: SEE OWN LEAVES
        // =========================
        public async Task<Interfaces.PagedResult<LeaveListDto>> GetMyLeavesPagedAsync(
           long userId, int page, int pageSize, LeaveFilterDto filter)
        {
            var query = _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .AsQueryable();

            // =========================
            // 🔥 FLEXIBLE FILTERS
            // =========================

            // Year filter
            // =========================
            // 🔥 FLEXIBLE FILTERS
            // =========================

            // Year filter
            // ✅ DATE FILTER (STRICT MONTH FIRST)
            // =========================
            // ✅ DATE FILTER (CORRECT)
            // =========================
            if (filter.Month.HasValue)
            {
                int month = filter.Month.Value;
                int year = filter.Year ?? DateTime.Now.Year; // fallback if year not sent

                query = query.Where(l =>
                    (l.FromDate.Year == year && l.FromDate.Month == month)
                    ||
                    (l.ToDate.Year == year && l.ToDate.Month == month)
                );
            }
            else if (filter.Year.HasValue)
            {
                int year = filter.Year.Value;

                query = query.Where(l =>
                    l.FromDate.Year == year ||
                    l.ToDate.Year == year
                );
            }


            // Status filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(l => l.Status == filter.Status);
            }

            // Leave type filter
            if (!string.IsNullOrWhiteSpace(filter.LeaveType))
            {
                query = query.Where(l => l.LeaveType == filter.LeaveType);
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 🔥 IMPORTANT SAFETY
            if (totalPages == 0)
            {
                totalPages = 1;
            }
            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status
                })
                .ToListAsync();

            return new Interfaces.PagedResult<LeaveListDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }



        // =========================
        // ADMIN / HR: SEE ALL
        // =========================
        public async Task<Interfaces.PagedResult<LeaveListDto>> GetAllLeavesPagedAsync(
            int page, int pageSize, LeaveFilterDto filter)
        {
            var query = _context.LeaveRequests.AsQueryable();

            // =========================
            // 🔥 FLEXIBLE FILTERS
            // =========================

            // Year filter
            // ✅ DATE FILTER (STRICT MONTH FIRST)
            // =========================
            // ✅ DATE FILTER (CORRECT)
            // =========================
            if (filter.Month.HasValue)
            {
                int month = filter.Month.Value;
                int year = filter.Year ?? DateTime.Now.Year; // fallback if year not sent

                query = query.Where(l =>
                    (l.FromDate.Year == year && l.FromDate.Month == month)
                    ||
                    (l.ToDate.Year == year && l.ToDate.Month == month)
                );
            }
            else if (filter.Year.HasValue)
            {
                int year = filter.Year.Value;

                query = query.Where(l =>
                    l.FromDate.Year == year ||
                    l.ToDate.Year == year
                );
            }



            // Status filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(l => l.Status == filter.Status);
            }

            // Leave type filter
            if (!string.IsNullOrWhiteSpace(filter.LeaveType))
            {
                query = query.Where(l => l.LeaveType == filter.LeaveType);
            }

            // 🔥 HR-only: User name filter
            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                query = query.Where(l => l.UserName.Contains(filter.UserName));
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status
                })
                .ToListAsync();

            return new Interfaces.PagedResult<LeaveListDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }


        // =========================
        // 🔥 GET LEAVE BY ID
        // =========================
        public async Task<LeaveListDto?> GetByIdAsync(long id)
        {
            return await _context.LeaveRequests
                .Where(l => l.Id == id)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status,
                    Reason = l.Reason
                })
                .FirstOrDefaultAsync();
        }

        // =========================
        // 🔥 APPROVE / REJECT
        // =========================
        public async Task UpdateStatusAsync(long id, string status, string actionBy)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);

            if (leave == null)
                throw new Exception("Leave not found");

            if (leave.Status != "Pending")
                throw new Exception("Leave already processed");

            leave.Status = status;
            await _context.SaveChangesAsync();
        }

        // =========================
        // 🔥 GET PENDING LEAVES
        // =========================
        public async Task<List<LeaveListDto>> GetPendingLeavesAsync()
        {
            return await _context.LeaveRequests
                .Where(l => l.Status == "Pending")
                .OrderBy(l => l.FromDate)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status
                })
                .ToListAsync();
        }

        // =========================
        // 🔥 DELETE LEAVE (OWN + PENDING)
        // =========================
        public async Task DeleteLeaveAsync(long id, long userId)
        {
            var leave = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (leave == null)
                throw new Exception("Leave not found");

            if (leave.Status != "Pending")
                throw new Exception("Cannot delete approved/rejected leave");

            _context.LeaveRequests.Remove(leave);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateLeaveAsync(long id, long userId, UpdateLeaveDto dto)
        {
            var leave = await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (leave == null)
                throw new Exception("Leave not found");

            // ❗ Important rule
            if (leave.Status != "Pending")
                throw new Exception("Only pending leave can be edited");

            // Update fields
            leave.LeaveType = dto.LeaveType;
            leave.FromDate = dto.FromDate;
            leave.ToDate = dto.ToDate;
            leave.Reason = dto.Reason;

            await _context.SaveChangesAsync();
        }

    }
}
