using MediatR;
using ContactManager.Domain.Interfaces.Repositories;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Contacts.Commands.UpdateContact;

public record UpdateContactCommand(
    int Id,
    int UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Address) : IRequest<Unit>;

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Contact> _contactRepository;

    public UpdateContactCommandHandler(IUnitOfWork unitOfWork, IGenericRepository<Contact> contactRepository)
    {
        _unitOfWork = unitOfWork;
        _contactRepository = contactRepository;
    }

    public async Task<Unit> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.Id);
        if (contact == null || contact.UserId != request.UserId)
        {
            throw new Exception("Contact not found.");
        }

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.PhoneNumber = request.PhoneNumber;
        contact.Company = request.Company;
        contact.JobTitle = request.JobTitle;
        contact.Address = request.Address;

        _contactRepository.Update(contact);
        await _unitOfWork.CompleteAsync();

        return Unit.Value;
    }
}
