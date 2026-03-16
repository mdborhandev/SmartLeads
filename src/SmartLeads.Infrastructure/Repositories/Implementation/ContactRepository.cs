using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class ContactRepository : BaseRepository<Contact, int>, IContactRepository
{
    public ContactRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IList<Contact>> GetContactsByUserIdAsync(int userId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync(token);
    }

    public async Task<Contact?> GetContactByIdAndUserIdAsync(int id, int userId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted, token);
    }

    public async Task<IList<ContactDto>> GetContactDtosByUserIdAsync(int userId, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Select(c => new ContactDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.Company,
                c.JobTitle,
                c.Address,
                c.IsArchived))
            .ToListAsync(token);
    }

    public async Task<ContactDto?> GetContactDtoByIdAsync(int id, CancellationToken token = default)
    {
        return await _dbContext.Contacts
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new ContactDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.Company,
                c.JobTitle,
                c.Address,
                c.IsArchived))
            .FirstOrDefaultAsync(token);
    }

    public async Task UpdateContactAsync(int id, ContactDto contactDto, CancellationToken token = default)
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
        existingContact.Company = contactDto.Company;
        existingContact.JobTitle = contactDto.JobTitle;
        existingContact.Address = contactDto.Address;
        existingContact.IsArchived = contactDto.IsArchived;
        existingContact.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(token);
    }
}
