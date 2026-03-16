using MediatR;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Web.Contacts.Commands.DeleteContact;

public record DeleteContactCommand(int Id, int UserId) : IRequest<Unit>;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteContactCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _unitOfWork.contactRepository.GetContactByIdAndUserIdAsync(
            request.Id, request.UserId, cancellationToken);

        if (contact == null)
        {
            throw new Exception("Contact not found.");
        }

        await _unitOfWork.contactRepository.RemoveAsync(contact);
        await _unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
