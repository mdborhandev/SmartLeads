using MediatR;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Web.Contacts.Commands.CreateContact;

public record CreateContactCommand(
    int UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Address) : IRequest<int>;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateContactCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            UserId = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Company = request.Company,
            JobTitle = request.JobTitle,
            Address = request.Address
        };

        await _unitOfWork.contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return contact.Id;
    }
}
