using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class ContactRepository : BaseRepository<Contact, Guid>, IContactRepository
{
    private readonly DefaultDbContext _defaultDbContext;

    public ContactRepository(DefaultDbContext dbContext) : base(dbContext)
    {
        _defaultDbContext = dbContext;
    }

    public async Task<IList<Contact>> GetContactsByUserIdAsync(Guid userId, CancellationToken token = default)
    {
        return await _defaultDbContext.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<IList<Contact>> GetContactsByCompanyIdAsync(Guid companyId, CancellationToken token = default)
    {
        // In company database, all contacts belong to that company
        // companyId is used for validation/filtering if needed
        return await _defaultDbContext.Contacts
            .Where(c => !c.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<Contact?> GetContactByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken token = default)
    {
        return await _defaultDbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted, token);
    }

    public async Task<IList<ContactDto>> GetContactDtosByUserIdAsync(Guid userId, CancellationToken token = default)
    {
        return await _defaultDbContext.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Select(c => new ContactDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.ContactCompany,
                c.JobTitle,
                c.Address,
                c.IsArchived,
                null, // CompanyId not needed in company DB
                c.UserId))
            .ToListAsync(token);
    }

    public async Task<IList<ContactDto>> GetContactDtosByCompanyIdAsync(Guid companyId, CancellationToken token = default)
    {
        return await _defaultDbContext.Contacts
            .Where(c => !c.IsDeleted)
            .Select(c => new ContactDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.ContactCompany,
                c.JobTitle,
                c.Address,
                c.IsArchived,
                null, // CompanyId not needed in company DB
                c.UserId))
            .ToListAsync(token);
    }

    public async Task<ContactDto?> GetContactDtoByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _defaultDbContext.Contacts
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new ContactDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.ContactCompany,
                c.JobTitle,
                c.Address,
                c.IsArchived,
                null, // CompanyId not needed in company DB
                c.UserId))
            .FirstOrDefaultAsync(token);
    }

    public async Task UpdateContactAsync(Guid id, ContactDto contactDto, CancellationToken token = default)
    {
        var existingContact = await _defaultDbContext.Contacts.FindAsync(new object[] { id }, token);
        if (existingContact == null)
        {
            throw new ArgumentException("Contact not found.");
        }

        existingContact.FirstName = contactDto.FirstName;
        existingContact.LastName = contactDto.LastName;
        existingContact.Email = contactDto.Email;
        existingContact.PhoneNumber = contactDto.PhoneNumber;
        existingContact.ContactCompany = contactDto.Company;
        existingContact.JobTitle = contactDto.JobTitle;
        existingContact.Address = contactDto.Address;
        existingContact.IsArchived = contactDto.IsArchived;
        existingContact.UpdatedAt = DateTime.UtcNow;

        await _defaultDbContext.SaveChangesAsync(token);
    }
}
