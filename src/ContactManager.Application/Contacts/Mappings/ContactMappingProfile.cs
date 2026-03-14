using AutoMapper;
using ContactManager.Application.Contacts.Models;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Contacts.Mappings;

public class ContactMappingProfile : Profile
{
    public ContactMappingProfile()
    {
        CreateMap<Contact, ContactDto>();
    }
}
