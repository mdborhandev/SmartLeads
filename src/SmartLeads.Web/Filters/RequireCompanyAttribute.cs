using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace SmartLeads.Web.Filters;

        /// <summary>
        /// Attribute to check if user is associated with any company.
        /// Redirects to CreateCompany page if user has no company association.
        /// </summary>
public class RequireCompanyAttribute : TypeFilterAttribute
{
    public RequireCompanyAttribute() : base(typeof(RequireCompanyFilter))
    {
    }

    private class RequireCompanyFilter : IAsyncActionFilter
    {
        private readonly IUserRepository _userRepository;
        private readonly SmartLeadsDbContext _dbContext;

        public RequireCompanyFilter(IUserRepository userRepository, SmartLeadsDbContext dbContext)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = context.HttpContext.Request.Cookies["UserId"];
            }

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var hasCompany = await _dbContext.UserCompanies
                    .AnyAsync(uc => uc.UserId == userId && uc.IsActive && !uc.IsDeleted);

                if (!hasCompany)
                {
                    context.Result = new RedirectToActionResult("CreateCompany", "UserCompany", null);
                    return;
                }
            }

            await next();
        }
    }
}
