using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Web.Controllers;

[Authorize]
public class ContactsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ContactsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    // Main Index page
    public IActionResult Index()
    {
        return View();
    }

    // API: Get all contacts
    [HttpGet("/api/contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var contacts = await _unitOfWork.contactRepository.GetContactDtosByUserIdAsync(UserId);
        return Ok(contacts);
    }

    // API: Get single contact
    [HttpGet("/api/contacts/{id}")]
    public async Task<IActionResult> GetContact(int id)
    {
        var contact = await _unitOfWork.contactRepository.GetContactDtoByIdAsync(id);
        if (contact == null) return NotFound();
        
        return Ok(contact);
    }

    // API: Create contact
    [HttpPost("/api/contacts")]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = new[] { "Invalid model state" } });
        }

        var contact = new Domain.Models.Contact
        {
            UserId = UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Company = request.Company,
            JobTitle = request.JobTitle,
            Address = request.Address
        };

        await _unitOfWork.contactRepository.AddAsync(contact);
        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, id = contact.Id });
    }

    // API: Update contact
    [HttpPut("/api/contacts/{id}")]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] UpdateContactRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(new { success = false, message = "ID mismatch" });
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = new[] { "Invalid model state" } });
        }

        var contact = await _unitOfWork.contactRepository.GetContactByIdAndUserIdAsync(id, UserId);
        if (contact == null)
        {
            return NotFound();
        }

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.PhoneNumber = request.PhoneNumber;
        contact.Company = request.Company;
        contact.JobTitle = request.JobTitle;
        contact.Address = request.Address;

        _unitOfWork.contactRepository.Edit(contact);
        await _unitOfWork.SaveAsync();

        return Ok(new { success = true });
    }

    // API: Delete contact
    [HttpDelete("/api/contacts/{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var contact = await _unitOfWork.contactRepository.GetContactByIdAndUserIdAsync(id, UserId);
        if (contact == null)
        {
            return NotFound();
        }

        await _unitOfWork.contactRepository.RemoveAsync(id);
        await _unitOfWork.SaveAsync();

        return Ok(new { success = true });
    }

    // Legacy actions (kept for backward compatibility if needed)
    public IActionResult Create()
    {
        return View();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contact = await _unitOfWork.contactRepository.GetContactDtoByIdAsync(id);
        if (contact == null) return NotFound();

        return View(contact);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateContactRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var contact = new Domain.Models.Contact
        {
            UserId = UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Company = request.Company,
            JobTitle = request.JobTitle,
            Address = request.Address
        };

        await _unitOfWork.contactRepository.AddAsync(contact);
        await _unitOfWork.SaveAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateContactRequest request)
    {
        if (id != request.Id) return BadRequest();
        if (!ModelState.IsValid) return View(request);

        var contact = await _unitOfWork.contactRepository.GetContactByIdAndUserIdAsync(id, UserId);
        if (contact == null) return NotFound();

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.PhoneNumber = request.PhoneNumber;
        contact.Company = request.Company;
        contact.JobTitle = request.JobTitle;
        contact.Address = request.Address;

        _unitOfWork.contactRepository.Edit(contact);
        await _unitOfWork.SaveAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _unitOfWork.contactRepository.GetContactByIdAndUserIdAsync(id, UserId);
        if (contact != null)
        {
            await _unitOfWork.contactRepository.RemoveAsync(id);
            await _unitOfWork.SaveAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
