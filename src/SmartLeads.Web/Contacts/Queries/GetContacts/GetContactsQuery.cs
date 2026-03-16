using MediatR;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Interfaces.Repositories;

namespace SmartLeads.Web.Contacts.Queries.GetContacts;

public record GetContactsQuery(int UserId) : IRequest<IEnumerable<ContactDto>>;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, IEnumerable<ContactDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetContactsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.contactRepository.GetContactDtosByUserIdAsync(request.UserId, cancellationToken);
    }
}
