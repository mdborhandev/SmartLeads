using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;
using SmartLeads.Domain.Enums;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Controllers;

public class HomeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public HomeController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public IActionResult Landing()
    {
// If user is already logged in, redirect to home
         if (User.Identity?.IsAuthenticated == true)
         {
             return RedirectToAction("Index", "Home");
         }

        return View();
    }

    public IActionResult Index()
    {
        // Show dashboard for authenticated users
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Components()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
