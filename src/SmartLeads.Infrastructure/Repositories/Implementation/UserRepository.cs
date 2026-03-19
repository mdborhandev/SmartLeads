using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByUsernameAndCompanyIdAsync(string username, Guid companyId, CancellationToken token = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.CompanyId == companyId && !u.IsDeleted, token);
    }

    public async Task<User?> GetUserByIdAndCompanyIdAsync(Guid id, Guid companyId, CancellationToken token = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId && !u.IsDeleted, token);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _dbContext.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaginationResponse<UserTableDto>> GetUsersPagedAsync(Guid companyId, PaginationRequest request, CancellationToken token = default)
    {
        var query = _dbContext.Users
            .Where(u => u.CompanyId == companyId && !u.IsDeleted)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                (u.EmployeeId != null && u.EmployeeId.ToLower().Contains(search)) ||
                (u.Department != null && u.Department.ToLower().Contains(search)) ||
                (u.Designation != null && u.Designation.ToLower().Contains(search))
            );
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(token);

        // Apply sorting
        var sortField = request.SortField?.ToLower();
        var sortOrder = request.SortOrder?.ToLower() ?? "desc";
        
        query = sortField switch
        {
            "username" => sortOrder == "desc" ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "email" => sortOrder == "desc" ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "fullname" => sortOrder == "desc" ? query.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            "employeeid" => sortOrder == "desc" ? query.OrderByDescending(u => u.EmployeeId) : query.OrderBy(u => u.EmployeeId),
            "department" => sortOrder == "desc" ? query.OrderByDescending(u => u.Department) : query.OrderBy(u => u.Department),
            "designation" => sortOrder == "desc" ? query.OrderByDescending(u => u.Designation) : query.OrderBy(u => u.Designation),
            "role" => sortOrder == "desc" ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            "isactive" => sortOrder == "desc" ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
            "createdat" => sortOrder == "desc" ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        // Apply pagination
        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserTableDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                EmployeeId = u.EmployeeId,
                Department = u.Department,
                Designation = u.Designation,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(token);

        return new PaginationResponse<UserTableDto>
        {
            Data = users,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            LastPage = request.PageSize > 0 ? (int)Math.Ceiling(totalCount / (double)request.PageSize) : 1
        };
    }
}
