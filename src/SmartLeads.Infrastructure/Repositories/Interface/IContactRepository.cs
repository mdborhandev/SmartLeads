using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface IContactRepository : IBaseRepository<Contact, int>
{
    Task<IList<Contact>> GetContactsByUserIdAsync(int userId, CancellationToken token = default);
    Task<Contact?> GetContactByIdAndUserIdAsync(int id, int userId, CancellationToken token = default);
    Task<IList<ContactDto>> GetContactDtosByUserIdAsync(int userId, CancellationToken token = default);
    Task<ContactDto?> GetContactDtoByIdAsync(int id, CancellationToken token = default);
    Task UpdateContactAsync(int id, ContactDto contactDto, CancellationToken token = default);
}
