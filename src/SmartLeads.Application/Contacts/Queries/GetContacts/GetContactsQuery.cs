using MediatR;
using AutoMapper;
using SmartLeads.Application.Contacts.Models;
using SmartLeads.Domain.Interfaces.Repositories;
using SmartLeads.Domain.Models;

namespace SmartLeads.Application.Contacts.Queries.GetContacts;

public record GetContactsQuery(int UserId) : IRequest<IEnumerable<ContactDto>>;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, IEnumerable<ContactDto>>
{
    private readonly IGenericRepository<Contact> _contactRepository;
    private readonly IMapper _mapper;

    public GetContactsQueryHandler(IGenericRepository<Contact> contactRepository, IMapper mapper)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _contactRepository.FindAsync(c => c.UserId == request.UserId && !c.IsArchived);
        return _mapper.Map<IEnumerable<ContactDto>>(contacts);
    }
}
