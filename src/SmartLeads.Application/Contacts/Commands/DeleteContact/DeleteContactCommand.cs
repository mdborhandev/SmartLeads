using MediatR;
using SmartLeads.Domain.Interfaces.Repositories;
using SmartLeads.Domain.Models;

namespace SmartLeads.Application.Contacts.Commands.DeleteContact;

public record DeleteContactCommand(int Id, int UserId) : IRequest<Unit>;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Contact> _contactRepository;

    public DeleteContactCommandHandler(IUnitOfWork unitOfWork, IGenericRepository<Contact> contactRepository)
    {
        _unitOfWork = unitOfWork;
        _contactRepository = contactRepository;
    }

    public async Task<Unit> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.Id);
        if (contact == null || contact.UserId != request.UserId)
        {
            throw new Exception("Contact not found.");
        }

        _contactRepository.Remove(contact);
        await _unitOfWork.CompleteAsync();

        return Unit.Value;
    }
}
