using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SmartLeads.Web.Controllers;

public abstract class BaseController : Controller
{
    protected Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
    protected Guid CompanyId => Guid.Parse(User.FindFirstValue("CompanyId") ?? Guid.Empty.ToString());

    protected string? UserIdString => User.FindFirstValue(ClaimTypes.NameIdentifier);
    protected string? CompanyIdString => User.FindFirstValue("CompanyId");
}
