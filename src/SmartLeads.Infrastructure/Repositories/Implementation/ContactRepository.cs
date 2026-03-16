using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class ContactRepository : BaseRepository<Contact, Guid>, IContactRepository
{
    public ContactRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IList<Contact>> GetContactsByUserIdAsync(Guid userId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<IList<Contact>> GetContactsByCompanyIdAsync(Guid companyId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .Where(c => c.CompanyId == companyId && !c.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<Contact?> GetContactByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted, token);
    }

    public async Task<IList<ContactDto>> GetContactDtosByUserIdAsync(Guid userId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
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
                c.CompanyId,
                c.UserId))
            .ToListAsync(token);
    }

    public async Task<IList<ContactDto>> GetContactDtosByCompanyIdAsync(Guid companyId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .Where(c => c.CompanyId == companyId && !c.IsDeleted)
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
                c.CompanyId,
                c.UserId))
            .ToListAsync(token);
    }

    public async Task<ContactDto?> GetContactDtoByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbContext.Contacts
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
                c.CompanyId,
                c.UserId))
            .FirstOrDefaultAsync(token);
    }

    public async Task UpdateContactAsync(Guid id, ContactDto contactDto, CancellationToken token = default)
    {
        var existingContact = await _dbContext.Contacts.FindAsync(new object[] { id }, token);
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

        await _dbContext.SaveChangesAsync(token);
    }
}
