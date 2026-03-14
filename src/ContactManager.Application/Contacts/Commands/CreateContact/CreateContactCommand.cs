using MediatR;
using ContactManager.Domain.Interfaces.Repositories;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Contacts.Commands.CreateContact;

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
    private readonly IGenericRepository<Contact> _contactRepository;

    public CreateContactCommandHandler(IUnitOfWork unitOfWork, IGenericRepository<Contact> contactRepository)
    {
        _unitOfWork = unitOfWork;
        _contactRepository = contactRepository;
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

        await _contactRepository.AddAsync(contact);
        await _unitOfWork.CompleteAsync();

        return contact.Id;
    }
}
