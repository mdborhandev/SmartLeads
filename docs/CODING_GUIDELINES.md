# SmartLeads Coding Guidelines

## Project Overview

SmartLeads is an ASP.NET Core MVC Multi-Tenant CRM/Lead Management System built with .NET 10.0, PostgreSQL, and Entity Framework Core.

---

## Architecture

### Clean Architecture (Modular Monolith)

```
src/
├── SmartLeads.Domain/           # Domain layer (Entities, DTOs, Enums)
├── SmartLeads.Infrastructure/   # Infrastructure layer (Repositories, Services, Data)
├── SmartLeads.Utilities/        # Cross-cutting utilities (Identity, Email, Services)
└── SmartLeads.Web/              # Presentation layer (MVC Controllers, Views)
```

---

## Backend Patterns

### 1. BaseRepository Pattern (Generic)

Base repository with all common CRUD methods already implemented.

**Location:** `src/SmartLeads.Infrastructure/Repositories/BaseRepository.cs`

```csharp
public abstract class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> where TEntity : class
{
    // All these methods are already available:
    // GetByIdAsync, GetAllAsync, AddAsync, EditAsync, RemoveAsync, SoftDeleteAsync
    // GetPagedAsync, GetCountAsync, AnyAsync, FindAsync, SingleOrDefaultAsync
}
```

### 2. Entity-Specific Repository Pattern

Each entity has its own repository interface and implementation.

**Interface Location:** `src/SmartLeads.Infrastructure/Repositories/Interface/IContactRepository.cs`
**Implementation Location:** `src/SmartLeads.Infrastructure/Repositories/Implementation/ContactRepository.cs`

```csharp
// Interface - extends IBaseRepository, add ONLY entity-specific methods
public interface IContactRepository : IBaseRepository<Contact, Guid>
{
    Task<IList<Contact>> GetContactsByUserIdAsync(Guid userId, CancellationToken token = default);
    Task<ContactDto?> GetContactDtoByIdAsync(Guid id, CancellationToken token = default);
}

// Implementation - extend BaseRepository, add ONLY entity-specific logic
public class ContactRepository : BaseRepository<Contact, Guid>, IContactRepository
{
    private readonly DefaultDbContext _defaultDbContext;

    public ContactRepository(DefaultDbContext dbContext) : base(dbContext)
    {
        _defaultDbContext = dbContext;
    }

    public async Task<ContactDto?> GetContactDtoByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _defaultDbContext.Contacts
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new ContactDto( /* ... */ ))
            .FirstOrDefaultAsync(token);
    }
}
```

### 3. UnitOfWork Pattern

Access all repositories through UnitOfWork.

**Interface:** `src/SmartLeads.Infrastructure/Repositories/Interface/IUnitOfWork.cs`
**Implementation:** `src/SmartLeads.Infrastructure/Repositories/UnitOfWork.cs`

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    IContactRepository contactRepository { get; }
    IUserRepository userRepository { get; }
    ICompanyRepository companyRepository { get; }
    IDepartmentRepository departmentRepository { get; }
    Task SaveAsync(CancellationToken token = default);
}
```

### 4. Controller Pattern

Inject `IUnitOfWork`, access specific repository, use BaseRepository methods first.

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ContactsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactDto>>> GetAll()
    {
        var contacts = await _unitOfWork.contactRepository.GetAllAsync();
        return Ok(contacts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContactDto>> GetById(Guid id)
    {
        var contact = await _unitOfWork.contactRepository.GetContactDtoByIdAsync(id);
        if (contact == null)
            return NotFound(new { message = "Contact not found" });
        return Ok(contact);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] ContactDto request)
    {
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            UserId = GetCurrentUserId()
        };

        await _unitOfWork.contactRepository.AddAsync(contact);
        await _unitOfWork.SaveAsync();
        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] ContactDto request)
    {
        var contact = await _unitOfWork.contactRepository.GetByIdAsync(id);
        if (contact == null)
            return NotFound(new { message = "Contact not found" });

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;

        _unitOfWork.contactRepository.Edit(contact);
        await _unitOfWork.SaveAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var contact = await _unitOfWork.contactRepository.GetByIdAsync(id);
        if (contact == null)
            return NotFound(new { message = "Contact not found" });

        _unitOfWork.contactRepository.SoftDelete(contact);
        await _unitOfWork.SaveAsync();
        return NoContent();
    }
}
```

### 5. When to Add Custom Repository Methods

**ONLY add custom methods when BaseRepository methods are insufficient.**

Examples of when custom methods ARE needed:
- Complex queries with JOINs and projections to DTO
- Filtering by multiple conditions
- Queries requiring specific includes

Examples of what is ALREADY available in BaseRepository:
- `GetByIdAsync` - Get by primary key
- `GetAllAsync` - Get all records
- `AddAsync` - Insert new record
- `Edit` - Update existing record
- `SoftDeleteAsync` - Soft delete by ID or predicate
- `GetPagedAsync` - Pagination with filtering/sorting

### 2. Entity Pattern

All entities inherit from `BaseEntity`:

```csharp
public class Contact : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
}
```

### 3. DTO Pattern

DTOs are stored in `src/SmartLeads.Domain/DTOs/`:

```csharp
// Request DTO
public class ContactRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int CompanyId { get; set; }
}

// Response DTO
public class ContactResponse
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string CompanyName { get; set; }
}
```



### 5. DbContext Pattern

Use multi-tenant DbContexts:
- `SystemDbContext` - For system-wide data (Users, Roles, Companies)
- `DefaultDbContext` - For tenant-specific data (Contacts, Leads, etc.)

```csharp
public class SystemDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
}
```

### 6. Validation Pattern

Use FluentValidation:

```csharp
public class ContactValidator : AbstractValidator<ContactRequest>
{
    public ContactValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
    }
}
```

### 7. Enums Pattern

Store enums in `src/SmartLeads.Domain/Enums/`:

```csharp
public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    Manager = 3,
    User = 4
}

public enum LeadStatus
{
    New = 1,
    InProgress = 2,
    Qualified = 3,
    Converted = 4,
    Lost = 5
}
```

---

## Frontend Patterns

### 1. View Structure

Razor Views organized by feature in `src/SmartLeads.Web/Views/`:

```
Views/
├── Shared/
│   ├── _Layout.cshtml
│   └── _Sidebar.cshtml
├── Contacts/
│   ├── Index.cshtml
│   ├── Create.cshtml
│   └── Edit.cshtml
└── Leads/
    ├── Index.cshtml
    └── Details.cshtml
```

### 2. ViewModel Pattern

```csharp
public class ContactViewModel
{
    public IEnumerable<ContactResponse> Contacts { get; set; }
    public ContactRequest NewContact { get; set; }
    public PaginationInfo Pagination { get; set; }
}
```

### 3. Form Pattern

```html
<form asp-action="Create" method="post">
    <div class="mb-3">
        <label asp-for="FirstName" class="form-label"></label>
        <input asp-for="FirstName" class="form-control" />
        <span asp-validation-for="FirstName" class="text-danger"></span>
    </div>
</form>
```

### 4. Data Table Pattern

Use DataTables with server-side pagination:

```html
<table id="contactsTable" class="table datatable-responsive">
    <thead>
        <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Company</th>
            <th>Actions</th>
        </tr>
    </thead>
</table>
```

### 5. AJAX Pattern - Fetch By ID (Get Single Record)

```javascript
// GET single record by ID - returns object
$.ajax({
    url: '/api/contacts/' + contactId,
    type: 'GET',
    dataType: 'json',
    success: function(data) {
        // Data is a single object, use directly
        $('#firstName').val(data.firstName);
        $('#lastName').val(data.lastName);
        $('#email').val(data.email);
        $('#companyId').val(data.companyId);
    },
    error: function(xhr, status, error) {
        showToast('Error loading contact', 'error');
    }
});
```

### 6. AJAX Pattern - Send Object for Create/Update

```javascript
// POST - Create new record with object
var contactData = {
    firstName: $('#firstName').val(),
    lastName: $('#lastName').val(),
    email: $('#email').val(),
    phone: $('#phone').val(),
    companyId: parseInt($('#companyId').val())
};

$.ajax({
    url: '/api/contacts',
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify(contactData),
    success: function(response) {
        showToast('Contact created successfully', 'success');
        // Redirect or refresh table
    }
});

// PUT - Update existing record with object
$.ajax({
    url: '/api/contacts/' + contactId,
    type: 'PUT',
    contentType: 'application/json',
    data: JSON.stringify(contactData),
    success: function(response) {
        showToast('Contact updated successfully', 'success');
    }
});
```

### 7. Edit Page Pattern (Frontend)

```html
@model ContactResponse

<form id="contactForm">
    <input type="hidden" id="contactId" value="@Model.Id" />
    <div class="mb-3">
        <label class="form-label">First Name</label>
        <input type="text" id="firstName" class="form-control" value="@Model.FirstName" />
    </div>
    <div class="mb-3">
        <label class="form-label">Last Name</label>
        <input type="text" id="lastName" class="form-control" value="@Model.LastName" />
    </div>
    <div class="mb-3">
        <label class="form-label">Email</label>
        <input type="email" id="email" class="form-control" value="@Model.Email" />
    </div>
    <button type="submit" class="btn btn-primary">Update</button>
</form>

<script>
$(document).ready(function() {
    $('#contactForm').on('submit', function(e) {
        e.preventDefault();
        
        var contactData = {
            firstName: $('#firstName').val(),
            lastName: $('#lastName').val(),
            email: $('#email').val()
        };
        
        var id = $('#contactId').val();
        
        $.ajax({
            url: '/api/contacts/' + id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(contactData),
            success: function() {
                showToast('Updated successfully', 'success');
            }
        });
    });
});
</script>
```

### 6. Toast Notifications

```html
<div class="toast-container" id="toastContainer"></div>
```

```javascript
function showToast(message, type = 'success') {
    // Using SweetAlert2 or Toastr patterns
}
```

---

## Naming Conventions

### Backend

| Type | Convention | Example |
|------|-----------|---------|
| Entities | PascalCase, singular noun | `Contact`, `Lead` |
| DTOs | PascalCase, suffix with Request/Response | `ContactRequest`, `ContactResponse` |
| Repository Interfaces | `I` prefix + EntityName + Repository | `IContactRepository` |
| Repository Implementations | EntityName + Repository | `ContactRepository` |
| Enums | PascalCase, singular | `LeadStatus` |
| Controller Methods | HttpVerb + EntityName | `GetContacts`, `CreateContact` |

### Frontend

| Type | Convention | Example |
|------|-----------|---------|
| Views | PascalCase, descriptive | `Index.cshtml`, `ContactDetails.cshtml` |
| ViewModels | PascalCase, suffix with ViewModel | `ContactViewModel` |
| Partial Views | Underscore prefix (optional) | `_ContactForm.cshtml` |

---

## Database Patterns

### Multi-Tenancy

- All tenant-specific tables include `CompanyId` column
- Use `[RequireCompany]` attribute for automatic filtering
- Default DbContext factory handles company context

### Audit Fields

All entities include:
- `CreatedAt` - Creation timestamp
- `CreatedBy` - User who created
- `UpdatedAt` - Last update timestamp
- `UpdatedBy` - User who last updated

---

## Service Layer Patterns

Services are in `src/SmartLeads.Utilities/Services/`:

```csharp
public class NotificationService : INotificationService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Implementation
    }
}
```

---

## Filter Patterns

Custom action filters for cross-cutting concerns:

```csharp
[RequireCompany]
public class ContactsController : ControllerBase
{
    // Company context automatically injected
}
```

---

## Best Practices

### DO
- Use dependency injection for all services
- Return `ActionResult<T>` from controllers
- Use async/await for all I/O operations
- Validate input with FluentValidation
- Use DTOs for API requests/responses
- Follow single responsibility principle
- **Inject IUnitOfWork in controllers**
- **Use BaseRepository methods first** (GetByIdAsync, GetAllAsync, AddAsync, Edit, SoftDelete)
- **Add custom methods only in entity-specific repositories** when BaseRepository is insufficient
- **Frontend sends/receives JSON objects** (not arrays for single records)

### DON'T
- Create custom repository methods when BaseRepository already has the method
- Create generic repository interfaces for simple CRUD
- Expose entities directly to API
- Use synchronous operations for I/O
- Use `int` for entity IDs when project uses `Guid`

---

## API Response Format

```json
{
    "success": true,
    "data": { },
    "message": "Operation successful",
    "errors": []
}
```

---

## Testing

Unit tests should follow pattern:
- `[Fact]` or `[Theory]` for test methods
- Mock dependencies with Moq
- Test repository methods
- Test service methods
- Test controller actions

---

## File Locations Quick Reference

| Component | Location |
|-----------|----------|
| Entities | `src/SmartLeads.Domain/Models/` |
| DTOs | `src/SmartLeads.Domain/DTOs/` |
| Enums | `src/SmartLeads.Domain/Enums/` |
| Repository Interfaces | `src/SmartLeads.Infrastructure/Repositories/Interface/` |
| Repository Implementations | `src/SmartLeads.Infrastructure/Repositories/Implementation/` |
| DbContexts | `src/SmartLeads.Infrastructure/Persistence/` |
| Controllers | `src/SmartLeads.Web/Controllers/` |
| Views | `src/SmartLeads.Web/Views/` |
| ViewModels | `src/SmartLeads.Web/Models/` |
| Services | `src/SmartLeads.Utilities/Services/` |
| Validators | `src/SmartLeads.Web/Validators/` |