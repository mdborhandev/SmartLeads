using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartLeads.Application.Contacts.Queries.GetContacts;
using SmartLeads.Application.Contacts.Commands.CreateContact;
using SmartLeads.Application.Contacts.Commands.UpdateContact;
using SmartLeads.Application.Contacts.Commands.DeleteContact;
using SmartLeads.Application.Contacts.Models;

namespace SmartLeads.Web.Controllers;

[Authorize]
public class ContactsController : Controller
{
    private readonly ISender _sender;

    public ContactsController(ISender sender)
    {
        _sender = sender;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    public async Task<IActionResult> Index()
    {
        var contacts = await _sender.Send(new GetContactsQuery(UserId));
        return View(contacts);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateContactCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        await _sender.Send(command with { UserId = UserId });
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contacts = await _sender.Send(new GetContactsQuery(UserId));
        var contact = contacts.FirstOrDefault(c => c.Id == id);
        if (contact == null) return NotFound();

        return View(contact);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateContactCommand command)
    {
        if (id != command.Id) return BadRequest();
        if (!ModelState.IsValid) return View(command);

        await _sender.Send(command with { UserId = UserId });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteContactCommand(id, UserId));
        return RedirectToAction(nameof(Index));
    }
}
