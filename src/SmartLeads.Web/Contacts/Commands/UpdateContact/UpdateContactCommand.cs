using MediatR;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Web.Contacts.Commands.UpdateContact;

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

    public UpdateContactCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _unitOfWork.contactRepository.GetContactByIdAndUserIdAsync(
            request.Id, request.UserId, cancellationToken);

        if (contact == null)
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

        _unitOfWork.contactRepository.Edit(contact);
        await _unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
