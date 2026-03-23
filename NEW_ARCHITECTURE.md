# SmartLeads Multi-Company Database Architecture

## Overview

SmartLeads now uses a **multi-company architecture** where:
- **Users** have only login credentials and basic profile info
- **Companies** are separate entities
- **Users can belong to multiple companies** through UserCompany
- **Employees** are company-specific records
- **EmployeeUser** links Employees to Users

## Database Structure

### System Database Tables

#### 1. **Users** Table
Pure user authentication and basic info:
```csharp
public class User : BaseSystemEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePicture { get; set; }
    public UserRole Role { get; set; }
    
    // Navigation
    public ICollection<UserCompany> UserCompanies { get; set; }
    public ICollection<EmployeeUser> EmployeeUsers { get; set; }
}
```

#### 2. **Companies** Table
Company information:
```csharp
public class Company : BaseSystemEntity
{
    public string Name { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Logo { get; set; }
    public bool IsParent { get; set; }
    public Guid? ParentCompanyId { get; set; }
    
    // Navigation
    public ICollection<UserCompany> UserCompanies { get; set; }
    public ICollection<Employee> Employees { get; set; }
}
```

#### 3. **UserCompanies** Table
Links Users to Companies with default company flag:
```csharp
public class UserCompany : BaseSystemEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsDefault { get; set; }  // Default company for user
    
    public User User { get; set; }
    public Company Company { get; set; }
}
```

#### 4. **Employees** Table
Company-specific employee records:
```csharp
public class Employee : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    
    // Employee Information (per company)
    public string EmployeeId { get; set; }      // EMP001, EMP002, etc.
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public ICollection<EmployeeUser> EmployeeUsers { get; set; }
}
```

#### 5. **EmployeeUsers** Table
Links Employees to Users (many-to-many):
```csharp
public class EmployeeUser : BaseSystemEntity
{
    public Guid EmployeeId { get; set; }
    public Guid UserId { get; set; }
    public bool IsPrimary { get; set; }  // Primary employee record
    
    public Employee Employee { get; set; }
    public User User { get; set; }
}
```

#### 6. **Invitations** Table
Company invitations to users:
```csharp
public class Invitation : BaseSystemEntity
{
    public string Email { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    public Guid InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; }
    public UserRole Role { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public InvitationStatus Status { get; set; }
}
```

### Company Database Tables (Per Company)

Each company has its own database with:
- **Contacts** - Company's contacts/leads
- **Groups** - Contact groups
- **Tags** - Contact tags
- **Notes** - Notes on contacts
- **Attachments** - Contact attachments
- **ColumnFilters** - User-defined filters

## Relationships Diagram

```
┌─────────────┐         ┌──────────────────┐         ┌──────────────┐
│    Users    │         │   UserCompanies  │         │   Companies  │
├─────────────┤         ├──────────────────┤         ├──────────────┤
│ Id          │◄───────┤ UserId           │         │ Id           │
│ Username    │         │ CompanyId        │────────►│ Name         │
│ Email       │         │ IsDefault        │         │ Code         │
│ FirstName   │         │ IsActive         │         │ ...          │
│ LastName    │         └──────────────────┘         └──────────────┘
│ ...         │                    ▲                          │
└─────────────┘                    │                          │
       │                           │                          │
       │                           │                          │
       │                    ┌──────┴──────────┐               │
       │                    │                 │               │
       │                    │                 │               │
       │              ┌─────▼─────┐    ┌──────▼──────┐        │
       │              │ Employee  │    │EmployeeUser │        │
       │              ├───────────┤    ├─────────────┤        │
       └─────────────►│ UserId    │◄───│ EmployeeId  │        │
                      │ CompanyId │    │ UserId      │        │
                      │ EmployeeId│    │ IsPrimary   │        │
                      │ Department│    └─────────────┘        │
                      │ Designation│                          │
                      └───────────┘                          │
                                                              │
       ┌──────────────────────────────────────────────────────┘
       │
       │  Company Database
       ▼
┌────────────────────────────────────────────┐
│  Company-Specific Data                     │
│  - Contacts                                │
│  - Groups                                  │
│  - Tags                                    │
│  - Notes                                   │
│  - Attachments                             │
│  - ColumnFilters                           │
└────────────────────────────────────────────┘
```

## How It Works

### User Registration Flow

1. **Create User** → Users table
2. **Create UserCompany** → Link user to company (set IsDefault = true)
3. **Create Employee** → Employee record with company-specific info
4. **Create EmployeeUser** → Link Employee to User (set IsPrimary = true)

### User Login Flow

1. User logs in with username/password
2. Load all companies from UserCompanies
3. Load default company (IsDefault = true) or first company
4. Load employee record from Employee → EmployeeUser
5. Set company context for subsequent operations

### Multiple Company Access

```csharp
// Get all companies for user
var userCompanies = await _systemDbContext.UserCompanies
    .Include(uc => uc.Company)
    .Where(uc => uc.UserId == currentUserId)
    .ToListAsync();

// Switch to different company
await _companyContext.SetCurrentCompany(selectedCompanyId);

// Get employee record for current company
var employee = await _systemDbContext.Employees
    .FirstOrDefaultAsync(e => 
        e.CompanyId == currentCompanyId && 
        e.EmployeeUsers.Any(eu => eu.UserId == currentUserId));
```

## Connection Strings

```json
{
  "ConnectionStrings": {
    "SystemConnection": "Host=localhost;Database=SmartLeadsSystemDb;...",
    "CompanyConnection": "Host=localhost;Database=SmartLeadsCompanyDb;..."
  }
}
```

## Key Benefits

1. **Clean Separation**: User auth separate from company data
2. **Multiple Companies**: User can access multiple companies
3. **Company-Specific Employees**: Different roles/departments per company
4. **Default Company**: Quick access to primary company
5. **Flexible Mapping**: EmployeeUser allows multiple employees per user

## Migration Notes

### Old Structure → New Structure

**Before:**
- User had CompanyId, EmployeeId, Department, Designation, etc.

**After:**
- User: Only auth + basic info
- UserCompany: UserId + CompanyId + IsDefault
- Employee: CompanyId + EmployeeId + Department + Designation + ...
- EmployeeUser: Links Employee to User

## Next Steps

1. Create migrations for SystemDbContext
2. Create migrations for CompanyDbContext
3. Update controllers to use new structure
4. Update UI to support company switching
