using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class UserRepository : GenericSystemRepository<User>, IUserRepository
{
    private readonly SystemDbContext _systemDbContext;

    public UserRepository(SystemDbContext systemDbContext) : base(systemDbContext)
    {
        _systemDbContext = systemDbContext;
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _systemDbContext.Users
            .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _systemDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _systemDbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByUsernameAndCompanyIdAsync(string username, Guid companyId, CancellationToken token = default)
    {
        // Get user through UserCompany junction
        var userCompany = await _systemDbContext.UserCompanies
            .Include(uc => uc.User)
            .FirstOrDefaultAsync(uc => uc.User.Username == username && uc.CompanyId == companyId && !uc.IsDeleted, token);
        
        return userCompany?.User;
    }

    public async Task<User?> GetUserByIdAndCompanyIdAsync(Guid id, Guid companyId, CancellationToken token = default)
    {
        // Get user through UserCompany junction
        var userCompany = await _systemDbContext.UserCompanies
            .Include(uc => uc.User)
            .FirstOrDefaultAsync(uc => uc.UserId == id && uc.CompanyId == companyId && !uc.IsDeleted, token);
        
        return userCompany?.User;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _systemDbContext.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaginationResponse<UserTableDto>> GetUsersPagedAsync(Guid companyId, PaginationRequest request, CancellationToken token = default)
    {
        // Query through Employee -> EmployeeUser -> User
        var query = _systemDbContext.Employees
            .Where(e => e.CompanyId == companyId && !e.IsDeleted && e.IsActive)
            .Include(e => e.EmployeeUsers)
                .ThenInclude(eu => eu.User)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e =>
                e.EmployeeUsers.Any(eu => eu.User.Username.ToLower().Contains(search)) ||
                e.EmployeeUsers.Any(eu => eu.User.Email.ToLower().Contains(search)) ||
                e.EmployeeUsers.Any(eu => eu.User.FirstName.ToLower().Contains(search)) ||
                e.EmployeeUsers.Any(eu => eu.User.LastName.ToLower().Contains(search)) ||
                (e.EmployeeId != null && e.EmployeeId.ToLower().Contains(search)) ||
                (e.Department != null && e.Department.ToLower().Contains(search)) ||
                (e.Designation != null && e.Designation.ToLower().Contains(search))
            );
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(token);

        // Apply sorting
        var sortField = request.SortField?.ToLower();
        var sortOrder = request.SortOrder?.ToLower() ?? "desc";

        query = sortField switch
        {
            "username" => sortOrder == "desc" 
                ? query.OrderByDescending(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Username) 
                : query.OrderBy(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Username),
            "email" => sortOrder == "desc" 
                ? query.OrderByDescending(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Email) 
                : query.OrderBy(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Email),
            "fullname" => sortOrder == "desc" 
                ? query.OrderByDescending(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.LastName)
                    .ThenByDescending(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.FirstName) 
                : query.OrderBy(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.FirstName)
                    .ThenBy(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.LastName),
            "employeeid" => sortOrder == "desc" ? query.OrderByDescending(e => e.EmployeeId) : query.OrderBy(e => e.EmployeeId),
            "department" => sortOrder == "desc" ? query.OrderByDescending(e => e.Department) : query.OrderBy(e => e.Department),
            "designation" => sortOrder == "desc" ? query.OrderByDescending(e => e.Designation) : query.OrderBy(e => e.Designation),
            "role" => sortOrder == "desc" 
                ? query.OrderByDescending(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Role) 
                : query.OrderBy(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Role),
            "isactive" => sortOrder == "desc" ? query.OrderByDescending(e => e.IsActive) : query.OrderBy(e => e.IsActive),
            "createdat" => sortOrder == "desc" 
                ? query.OrderByDescending(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.CreatedAt) 
                : query.OrderBy(e => e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.CreatedAt),
            _ => query.OrderByDescending(e => e.CreatedAt)
        };

        // Apply pagination
        var employees = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new UserTableDto
            {
                Id = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Id,
                Username = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Username,
                Email = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Email,
                FirstName = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.FirstName,
                LastName = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.LastName,
                EmployeeId = e.EmployeeId,
                Department = e.Department,
                Designation = e.Designation,
                Role = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.Role,
                IsActive = e.IsActive,
                CreatedAt = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)!.User.CreatedAt
            })
            .ToListAsync(token);

        return new PaginationResponse<UserTableDto>
        {
            Data = employees,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            LastPage = request.PageSize > 0 ? (int)Math.Ceiling(totalCount / (double)request.PageSize) : 1
        };
    }
}
