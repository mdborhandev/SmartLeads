using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using IPasswordHasher = SmartLeads.Utilities.Interfaces.IPasswordHasher;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class UserRepository : GenericSystemRepository<User>, IUserRepository
{
    private new readonly SmartLeadsDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserRepository(SmartLeadsDbContext dbContext, IPasswordHasher passwordHasher) : base(dbContext)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
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
        var userCompany = await _dbContext.UserCompanies
            .Include(uc => uc.User)
            .FirstOrDefaultAsync(uc => uc.User.Username == username && uc.CompanyId == companyId && !uc.IsDeleted, token);
        
        return userCompany?.User;
    }

    public async Task<User?> GetUserByIdAndCompanyIdAsync(Guid id, Guid companyId, CancellationToken token = default)
    {
        var userCompany = await _dbContext.UserCompanies
            .Include(uc => uc.User)
            .FirstOrDefaultAsync(uc => uc.UserId == id && uc.CompanyId == companyId && !uc.IsDeleted, token);
        
        return userCompany?.User;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _dbContext.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaginationResponse<UserTableDto>> GetUsersPagedAsync(Guid companyId, PaginationRequest request, CancellationToken token = default)
    {
        var employees = await _dbContext.Employees
            .Where(e => e.CompanyId == companyId && !e.IsDeleted && e.IsActive)
            .Include(e => e.EmployeeUsers)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .ToListAsync(token);

        var userIds = employees
            .SelectMany(e => e.EmployeeUsers)
            .Select(eu => eu.UserId)
            .Distinct()
            .ToList();

        var users = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id) && !u.IsDeleted && u.IsActive)
            .ToListAsync(token);

        var userCompanies = await _dbContext.UserCompanies
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
                    Department = e.Department != null ? e.Department.Name : null,
                    Designation = e.Designation != null ? e.Designation.Name : null,
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

    public Task<bool> VerifyPasswordAsync(string password, string passwordHash)
    {
        return Task.FromResult(_passwordHasher.VerifyPassword(password, passwordHash));
    }

    public async Task<bool> UpdateProfileAsync(User user, CancellationToken token = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(Guid userId, CancellationToken token = default)
    {
        return await _dbContext.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId && uc.IsActive && !uc.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<bool> SetPasswordResetTokenAsync(string email, string token, DateTime expiryTime, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null)
        {
            return true;
        }

        user.ResetPasswordToken = token;
        user.ResetPasswordTokenExpiryTime = expiryTime;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPasswordHash, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null)
        {
            return false;
        }

        if (user.ResetPasswordToken != token)
        {
            return false;
        }

        if (user.ResetPasswordTokenExpiryTime < DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash = newPasswordHash;
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiryTime = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null, CancellationToken token = default)
    {
        var query = _dbContext.Users.AsQueryable();
        
        if (excludeUserId.HasValue)
        {
            return await query.AnyAsync(u => u.Username.ToLower() == username.ToLower() && u.Id != excludeUserId.Value, token);
        }
        
        return await query.AnyAsync(u => u.Username.ToLower() == username.ToLower(), token);
    }

    public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken token = default)
    {
        var query = _dbContext.Users.AsQueryable();
        
        if (excludeUserId.HasValue)
        {
            return await query.AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.Id != excludeUserId.Value, token);
        }
        
        return await query.AnyAsync(u => u.Email.ToLower() == email.ToLower(), token);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPasswordHash, string newPasswordHash, CancellationToken token = default)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { userId }, token);
        if (user == null)
        {
            return false;
        }

        if (user.PasswordHash != currentPasswordHash)
        {
            return false;
        }

        user.PasswordHash = newPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(token);
        return true;
    }
}
