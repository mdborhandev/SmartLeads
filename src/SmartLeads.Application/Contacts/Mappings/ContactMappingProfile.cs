using AutoMapper;
using SmartLeads.Application.Contacts.Models;
using SmartLeads.Domain.Models;

namespace SmartLeads.Application.Contacts.Mappings;

public class ContactMappingProfile : Profile
{
    public ContactMappingProfile()
    {
        CreateMap<Contact, ContactDto>();
    }
}
