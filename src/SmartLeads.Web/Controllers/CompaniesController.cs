using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Web.Controllers;

public class CompaniesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CompaniesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var companies = await _unitOfWork.companyRepository.GetCompanyDtosAsync();
        return View(companies);
    }
}
