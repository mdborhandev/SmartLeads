using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class UserRepository : GenericSystemRepository<User>, IUserRepository
{
    private readonly SystemDbContext _systemDbContext;
    private readonly DefaultDbContext _defaultDbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserRepository(SystemDbContext systemDbContext, DefaultDbContext defaultDbContext, IPasswordHasher passwordHasher) : base(systemDbContext)
    {
        _systemDbContext = systemDbContext;
        _defaultDbContext = defaultDbContext;
        _passwordHasher = passwordHasher;
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
        var employees = await _defaultDbContext.Employees
            .Where(e => e.CompanyId == companyId && !e.IsDeleted && e.IsActive)
            .Include(e => e.EmployeeUsers)
            .ToListAsync(token);

        var userIds = employees
            .SelectMany(e => e.EmployeeUsers)
            .Select(eu => eu.UserId)
            .Distinct()
            .ToList();

        var users = await _systemDbContext.Users
            .Where(u => userIds.Contains(u.Id) && !u.IsDeleted && u.IsActive)
            .ToListAsync(token);

        // Get UserCompany records to fetch roles for this company
        var userCompanies = await _systemDbContext.UserCompanies
            .Where(uc => uc.CompanyId == companyId && userIds.Contains(uc.UserId))
            .ToListAsync(token);

        var usersById = users.ToDictionary(u => u.Id);
        var userCompaniesByUserId = userCompanies.ToDictionary(uc => uc.UserId);

        var rows = employees
            .Select(e =>
            {
                var primaryLink = e.EmployeeUsers.FirstOrDefault(eu => eu.IsPrimary)
                    ?? e.EmployeeUsers.FirstOrDefault();

                if (primaryLink == null || !usersById.TryGetValue(primaryLink.UserId, out var user))
                {
                    return null;
                }

                // Get role from UserCompany instead of User
                var role = UserRole.User;
                if (userCompaniesByUserId.TryGetValue(user.Id, out var userCompany))
                {
                    role = userCompany.Role;
                }

                return new UserTableDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmployeeId = e.EmployeeId,
                    Department = e.Department,
                    Designation = e.Designation,
                    Role = role,
                    IsActive = e.IsActive,
                    CreatedAt = user.CreatedAt
                };
            })
            .Where(x => x != null)
            .Select(x => x!)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            rows = rows.Where(e =>
                e.Username.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search) ||
                ((e.FirstName ?? string.Empty).ToLower().Contains(search)) ||
                ((e.LastName ?? string.Empty).ToLower().Contains(search)) ||
                ((e.EmployeeId ?? string.Empty).ToLower().Contains(search)) ||
                ((e.Department ?? string.Empty).ToLower().Contains(search)) ||
                ((e.Designation ?? string.Empty).ToLower().Contains(search))
            );
        }

        var totalCount = rows.Count();

        // Apply sorting
        var sortField = request.SortField?.ToLower();
        var sortOrder = request.SortOrder?.ToLower() ?? "desc";

        rows = sortField switch
        {
            "username" => sortOrder == "desc" ? rows.OrderByDescending(e => e.Username) : rows.OrderBy(e => e.Username),
            "email" => sortOrder == "desc" ? rows.OrderByDescending(e => e.Email) : rows.OrderBy(e => e.Email),
            "fullname" => sortOrder == "desc"
                ? rows.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName)
                : rows.OrderBy(e => e.FirstName).ThenBy(e => e.LastName),
            "employeeid" => sortOrder == "desc" ? rows.OrderByDescending(e => e.EmployeeId) : rows.OrderBy(e => e.EmployeeId),
            "department" => sortOrder == "desc" ? rows.OrderByDescending(e => e.Department) : rows.OrderBy(e => e.Department),
            "designation" => sortOrder == "desc" ? rows.OrderByDescending(e => e.Designation) : rows.OrderBy(e => e.Designation),
            "role" => sortOrder == "desc" ? rows.OrderByDescending(e => e.Role) : rows.OrderBy(e => e.Role),
            "isactive" => sortOrder == "desc" ? rows.OrderByDescending(e => e.IsActive) : rows.OrderBy(e => e.IsActive),
            "createdat" => sortOrder == "desc" ? rows.OrderByDescending(e => e.CreatedAt) : rows.OrderBy(e => e.CreatedAt),
            _ => rows.OrderByDescending(e => e.CreatedAt)
        };

        var pagedUsers = rows
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginationResponse<UserTableDto>
        {
            Data = pagedUsers,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            LastPage = request.PageSize > 0 ? (int)Math.Ceiling(totalCount / (double)request.PageSize) : 1
        };
    }

    // Authentication and profile methods
    public Task<bool> VerifyPasswordAsync(string password, string passwordHash)
    {
        return Task.FromResult(_passwordHasher.VerifyPassword(password, passwordHash));
    }

    public async Task<bool> UpdateProfileAsync(User user, CancellationToken token = default)
    {
        _systemDbContext.Users.Update(user);
        await _systemDbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(Guid userId, CancellationToken token = default)
    {
        return await _systemDbContext.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId && uc.IsActive && !uc.IsDeleted)
            .ToListAsync(token);
    }

    // Password reset methods
    public async Task<bool> SetPasswordResetTokenAsync(string email, string token, DateTime expiryTime, CancellationToken cancellationToken = default)
    {
        var user = await _systemDbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null)
        {
            return true; // Always return true to prevent email enumeration
        }

        user.ResetPasswordToken = token;
        user.ResetPasswordTokenExpiryTime = expiryTime;
        await _systemDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPasswordHash, CancellationToken cancellationToken = default)
    {
        var user = await _systemDbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null)
        {
            return false;
        }

        // Validate token
        if (user.ResetPasswordToken != token)
        {
            return false;
        }

        // Check token expiry
        if (user.ResetPasswordTokenExpiryTime < DateTime.UtcNow)
        {
            return false;
        }

        // Update password
        user.PasswordHash = newPasswordHash;
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiryTime = null;

        await _systemDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
