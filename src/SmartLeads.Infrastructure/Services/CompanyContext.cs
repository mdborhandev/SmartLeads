using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;
using SmartLeads.Domain.Enums;
using SmartLeads.Infrastructure.Persistence;

namespace SmartLeads.Infrastructure.Services;

/// <summary>
/// Service for managing the current user's company context.
/// Users can belong to multiple companies, so we need to track which company they're currently working with.
/// </summary>
public interface ICompanyContext
{
    /// <summary>
    /// The current user's ID.
    /// </summary>
    Guid? CurrentUserId { get; }

    /// <summary>
    /// The current company ID the user is working with.
    /// </summary>
    Guid? CurrentCompanyId { get; }

    /// <summary>
    /// The current employee ID for the current company.
    /// </summary>
    Guid? CurrentEmployeeId { get; }

    /// <summary>
    /// The current user's employee information for the current company.
    /// </summary>
    UserCompany? CurrentEmployeeRecord { get; }

    /// <summary>
    /// Sets the current company context for the user.
    /// </summary>
    void SetCurrentCompany(Guid companyId);

    /// <summary>
    /// Gets all companies the current user belongs to.
    /// </summary>
    Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(CancellationToken token = default);

    /// <summary>
    /// Gets the default company ID for the current user.
    /// </summary>
    Task<Guid?> GetDefaultCompanyIdAsync(CancellationToken token = default);
    
    /// <summary>
    /// Clears the cached user companies for the specified user.
    /// </summary>
    void ClearUserCompaniesCache(Guid userId);
    
    /// <summary>
    /// Gets the current user's role in the current company.
    /// </summary>
    Task<UserRole?> GetCurrentCompanyRoleAsync(CancellationToken token = default);
}

public class CompanyContext : ICompanyContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SystemDbContext _systemDbContext;
    private readonly IMemoryCache _cache;

    public CompanyContext(
        IHttpContextAccessor httpContextAccessor,
        SystemDbContext systemDbContext,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _systemDbContext = systemDbContext;
        _cache = cache;
    }

    public Guid? CurrentUserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public Guid? CurrentCompanyId
    {
        get
        {
            // First check session
            var sessionCompanyId = _httpContextAccessor.HttpContext?.Session.GetString("CurrentCompanyId");
            if (!string.IsNullOrEmpty(sessionCompanyId) && Guid.TryParse(sessionCompanyId, out var sessionId))
            {
                return sessionId;
            }

            // Then check cookie
            var cookieCompanyId = _httpContextAccessor.HttpContext?.Request.Cookies["CurrentCompanyId"];
            if (!string.IsNullOrEmpty(cookieCompanyId) && Guid.TryParse(cookieCompanyId, out var cookieId))
            {
                return cookieId;
            }

            // If no session or cookie, try to get default company
            var userId = CurrentUserId;
            if (userId.HasValue)
            {
                var defaultCompany = _systemDbContext.UserCompanies
                    .FirstOrDefault(uc => uc.UserId == userId.Value && uc.IsDefault && uc.IsActive && !uc.IsDeleted);
                return defaultCompany?.CompanyId;
            }

            return null;
        }
    }

    public Guid? CurrentEmployeeId
    {
        get
        {
            // Check cookie first
            var cookieEmployeeId = _httpContextAccessor.HttpContext?.Request.Cookies["CurrentEmployeeId"];
            if (!string.IsNullOrEmpty(cookieEmployeeId) && Guid.TryParse(cookieEmployeeId, out var employeeId))
            {
                return employeeId;
            }
            
            return null;
        }
    }

    public UserCompany? CurrentEmployeeRecord { get; private set; }

    public void SetCurrentCompany(Guid companyId)
    {
        _httpContextAccessor.HttpContext?.Session.SetString("CurrentCompanyId", companyId.ToString());
        CurrentEmployeeRecord = null; // Will be loaded on demand
    }

    public async Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(CancellationToken token = default)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
        {
            return Enumerable.Empty<UserCompany>();
        }

        var cacheKey = $"usercompanies_{userId.Value}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<UserCompany>? cached))
        {
            return cached ?? Enumerable.Empty<UserCompany>();
        }

        var userCompanies = await _systemDbContext.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId.Value && uc.IsActive && !uc.IsDeleted)
            .ToListAsync(token);

        _cache.Set(cacheKey, userCompanies, TimeSpan.FromMinutes(30));

        return userCompanies;
    }
    
    public async Task<Guid?> GetDefaultCompanyIdAsync(CancellationToken token = default)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
        {
            return null;
        }

        var defaultCompany = await _systemDbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.IsDefault && uc.IsActive && !uc.IsDeleted, token);

        return defaultCompany?.CompanyId;
    }

    public async Task<UserCompany?> GetCurrentEmployeeRecordAsync(CancellationToken token = default)
    {
        if (CurrentEmployeeRecord != null)
        {
            return CurrentEmployeeRecord;
        }

        var userId = CurrentUserId;
        var companyId = CurrentCompanyId;

        if (!userId.HasValue || !companyId.HasValue)
        {
            return null;
        }

        CurrentEmployeeRecord = await _systemDbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.CompanyId == companyId.Value, token);

        return CurrentEmployeeRecord;
    }
    
    public void ClearUserCompaniesCache(Guid userId)
    {
        var cacheKey = $"usercompanies_{userId}";
        _cache.Remove(cacheKey);
    }
    
    public async Task<UserRole?> GetCurrentCompanyRoleAsync(CancellationToken token = default)
    {
        var userId = CurrentUserId;
        var companyId = CurrentCompanyId;
        
        if (!userId.HasValue || !companyId.HasValue)
        {
            return null;
        }
        
        var userCompany = await _systemDbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.CompanyId == companyId.Value && uc.IsActive && !uc.IsDeleted, token);
        
        return userCompany?.Role;
    }
}
