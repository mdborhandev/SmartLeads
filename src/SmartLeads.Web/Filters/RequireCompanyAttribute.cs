using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace SmartLeads.Web.Filters;

/// <summary>
/// Attribute to check if user is associated with any company.
/// Redirects to NoCompany page if user has no company association.
/// </summary>
public class RequireCompanyAttribute : TypeFilterAttribute
{
    public RequireCompanyAttribute() : base(typeof(RequireCompanyFilter))
    {
    }

    private class RequireCompanyFilter : IAsyncActionFilter
    {
        private readonly IUserRepository _userRepository;
        private readonly SystemDbContext _systemDbContext;

        public RequireCompanyFilter(IUserRepository userRepository, SystemDbContext systemDbContext)
        {
            _userRepository = userRepository;
            _systemDbContext = systemDbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get user ID from claims or cookies
            var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Try to get from cookie
                userIdClaim = context.HttpContext.Request.Cookies["UserId"];
            }

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                // Check if user has any company association
                var hasCompany = await _systemDbContext.UserCompanies
                    .AnyAsync(uc => uc.UserId == userId && uc.IsActive && !uc.IsDeleted);

                if (!hasCompany)
                {
                    // Redirect to NoCompany page
                    context.Result = new RedirectToActionResult("NoCompany", "UserCompany", null);
                    return;
                }
            }

            // User has company, proceed with action
            await next();
        }
    }
}
