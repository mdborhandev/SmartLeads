using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IContactRepository : IBaseRepository<Contact, Guid>
{
    Task<IList<Contact>> GetContactsByUserIdAsync(Guid userId, CancellationToken token = default);
    Task<IList<Contact>> GetContactsByCompanyIdAsync(Guid companyId, CancellationToken token = default);
    Task<Contact?> GetContactByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken token = default);
    Task<IList<ContactDto>> GetContactDtosByUserIdAsync(Guid userId, CancellationToken token = default);
    Task<IList<ContactDto>> GetContactDtosByCompanyIdAsync(Guid companyId, CancellationToken token = default);
    Task<ContactDto?> GetContactDtoByIdAsync(Guid id, CancellationToken token = default);
    Task UpdateContactAsync(Guid id, ContactDto contactDto, CancellationToken token = default);
}
