# SmartLeads Domain Models with Relationships

## Overview

This document describes all domain models in the SmartLeads multi-company architecture, including their properties and relationships.

---

## System Database Models

These models are stored in the **System Database** (`SmartLeadsSystemDb`).

### 1. User

**Purpose:** User authentication and basic profile information only.

**Table:** `Users`

```csharp
public class User : BaseSystemEntity
{
    // Primary Key
    public Guid Id { get; set; }
    
    // Authentication
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // Profile
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePicture { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    
    // Security
    public bool IsPasswordSetByUser { get; set; } = false;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiryTime { get; set; }
    
    // Navigation Properties
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public ICollection<EmployeeUser> EmployeeUsers { get; set; } = new List<EmployeeUser>();
    public ICollection<Invitation> InvitationsSent { get; set; } = new List<Invitation>();
}
```

**Relationships:**
- **One-to-Many** with `UserCompany` (A user can belong to multiple companies)
- **One-to-Many** with `EmployeeUser` (A user can have multiple employee records)
- **One-to-Many** with `Invitation` (A user can send multiple invitations)

---

### 2. Company

**Purpose:** Company/organization information.

**Table:** `Companies`

```csharp
public class Company : BaseSystemEntity
{
    // Primary Key
    public Guid Id { get; set; }
    
    // Company Information
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Logo { get; set; }
    
    // Hierarchy
    public bool IsParent { get; set; } = false;
    public Guid? ParentCompanyId { get; set; }
    public Company? ParentCompany { get; set; }
    
    // Navigation Properties
    public ICollection<Company> ChildCompanies { get; set; } = new List<Company>();
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}
```

**Relationships:**
- **Self-Referencing One-to-Many** (Parent/Child companies)
- **One-to-Many** with `UserCompany` (A company can have multiple users)
- **One-to-Many** with `Employee` (A company can have multiple employees)
- **One-to-Many** with `Contact` (Company's contacts - in Company DB)
- **One-to-Many** with `Group`, `Tag`, `Note`, `Attachment` (Company data - in Company DB)
- **One-to-Many** with `Invitation` (Company can send multiple invitations)

---

### 3. UserCompany

**Purpose:** Junction table linking Users to Companies. Tracks which companies a user belongs to and identifies their default company.

**Table:** `UserCompanies`

```csharp
public class UserCompany : BaseSystemEntity
{
    // Primary Key
    public Guid Id { get; set; }
    
    // Foreign Keys
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    
    // Flags
    public bool IsDefault { get; set; } = false;  // User's default company
    
    // Navigation Properties
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
```

**Relationships:**
- **Many-to-One** with `User` (Multiple user-company links per user)
- **Many-to-One** with `Company` (Multiple user-company links per company)
- **Unique Index:** `(UserId, CompanyId)` - Prevents duplicate links

---

### 4. Employee

**Purpose:** Company-specific employee records. Contains all employee information that varies by company.

**Table:** `Employees`

```csharp
public class Employee : BaseEntity
{
    // Primary Key
    public Guid Id { get; set; }
    
    // Foreign Key
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    // Employee Information (per company)
    public string EmployeeId { get; set; } = string.Empty;  // e.g., EMP001
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public ICollection<EmployeeUser> EmployeeUsers { get; set; } = new List<EmployeeUser>();
}
```

**Relationships:**
- **Many-to-One** with `Company` (Multiple employees per company)
- **One-to-Many** with `EmployeeUser` (An employee can be linked to multiple users)
- **Unique Index:** `(CompanyId, EmployeeId)` - Unique employee ID per company

---

### 5. EmployeeUser

**Purpose:** Junction table linking Employees to Users. Allows one user to have multiple employee records across different companies.

**Table:** `EmployeeUsers`

```csharp
public class EmployeeUser : BaseSystemEntity
{
    // Primary Key
    public Guid Id { get; set; }
    
    // Foreign Keys
    public Guid EmployeeId { get; set; }
    public Guid UserId { get; set; }
    
    // Flags
    public bool IsPrimary { get; set; } = false;  // Primary employee record for user
    
    // Navigation Properties
    public Employee Employee { get; set; } = null!;
    public User User { get; set; } = null!;
}
```

**Relationships:**
- **Many-to-One** with `Employee` (Multiple employee-user links per employee)
- **Many-to-One** with `User` (Multiple employee-user links per user)
- **Unique Index:** `(EmployeeId, UserId)` - Prevents duplicate links

---

### 6. Invitation

**Purpose:** Company invitations sent to users to join the company.

**Table:** `Invitations`

```csharp
public class Invitation : BaseSystemEntity
{
    // Primary Key
    public Guid Id { get; set; }
    
    // Invitation Details
    public string Email { get; set; } = string.Empty;
    
    // Foreign Keys
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public Guid InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;
    
    // Role & Status
    public UserRole Role { get; set; } = UserRole.User;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; } = false;
    public DateTime? AcceptedAt { get; set; }
    public string? RejectedReason { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string? Metadata { get; set; }  // Additional info as JSON
}
```

**Relationships:**
- **Many-to-One** with `Company` (Multiple invitations per company)
- **Many-to-One** with `User` (Multiple invitations sent by a user)

---

## Company Database Models

These models are stored in **Company-Specific Databases** (one database per company).

### 7. Contact

**Purpose:** Company's contacts/leads.

**Table:** `Contacts`

```csharp
public class Contact : BaseEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ContactCompany { get; set; }
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    public bool IsArchived { get; set; } = false;
    
    // Foreign Key
    public Guid UserId { get; set; }  // Owner
    public User User { get; set; } = null!;
    
    // Navigation
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<ContactTag> ContactTags { get; set; } = new List<ContactTag>();
    public ICollection<ContactGroup> ContactGroups { get; set; } = new List<ContactGroup>();
}
```

---

### 8. Group

**Purpose:** Contact groups for organizing contacts.

**Table:** `Groups`

```csharp
public class Group : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Foreign Key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    // Navigation
    public ICollection<ContactGroup> ContactGroups { get; set; } = new List<ContactGroup>();
}
```

---

### 9. Tag

**Purpose:** Tags for labeling contacts.

**Table:** `Tags`

```csharp
public class Tag : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    
    // Foreign Key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    // Navigation
    public ICollection<ContactTag> ContactTags { get; set; } = new List<ContactTag>();
}
```

---

### 10. Note

**Purpose:** Notes on contacts.

**Table:** `Notes`

```csharp
public class Note : BaseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    // Foreign Keys
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
```

---

### 11. Attachment

**Purpose:** File attachments for contacts.

**Table:** `Attachments`

```csharp
public class Attachment : BaseEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    
    // Foreign Key
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
}
```

---

### 12. ColumnFilter

**Purpose:** User-defined column filters for lists.

**Table:** `ColumnFilters`

```csharp
public class ColumnFilter : BaseEntity
{
    public Guid Id { get; set; }
    public string ListName { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty;
    
    // Foreign Key
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
```

---

### 13. ContactGroup (Junction)

**Purpose:** Many-to-Many relationship between Contact and Group.

**Table:** `ContactGroups`

```csharp
public class ContactGroup
{
    // Composite Primary Key
    public Guid ContactId { get; set; }
    public Guid GroupId { get; set; }
    
    // Navigation
    public Contact Contact { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
```

---

### 14. ContactTag (Junction)

**Purpose:** Many-to-Many relationship between Contact and Tag.

**Table:** `ContactTags`

```csharp
public class ContactTag
{
    // Composite Primary Key
    public Guid ContactId { get; set; }
    public Guid TagId { get; set; }
    
    // Navigation
    public Contact Contact { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
```

---

## Base Classes

### BaseSystemEntity

Used for system-level entities (no company association).

```csharp
public abstract class BaseSystemEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

### BaseEntity

Used for company-specific entities.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        SYSTEM DATABASE                               │
│                     (SmartLeadsSystemDb)                             │
│                                                                      │
│  ┌──────────────┐          ┌──────────────────┐                     │
│  │    Users     │          │ UserCompanies    │                     │
│  ├──────────────┤          ├──────────────────┤                     │
│  │ Id           │◄────────┤ UserId           │                     │
│  │ Username     │         │ CompanyId        │                     │
│  │ Email        │         │ IsDefault        │                     │
│  │ ...          │         └────────┬─────────┘                     │
│  └──────┬───────┘                  │                                │
│         │                          │                                │
│         │                          │                                │
│         ▼                          ▼                                │
│  ┌──────────────────┐      ┌──────────────┐                        │
│  │  EmployeeUsers   │      │  Companies   │                        │
│  ├──────────────────┤      ├──────────────┤                        │
│  │ EmployeeId       │─────►│ Id           │                        │
│  │ UserId           │      │ Name         │                        │
│  │ IsPrimary        │      │ Code         │                        │
│  └────────┬─────────┘      │ ...          │                        │
│           │                └──────┬───────┘                        │
│           │                       │                                │
│           ▼                       │                                │
│  ┌──────────────────┐            │                                │
│  │   Employees      │◄───────────┘                                │
│  ├──────────────────┤                                             │
│  │ Id               │                                             │
│  │ CompanyId        │                                             │
│  │ EmployeeId       │                                             │
│  │ Department       │                                             │
│  │ Designation      │                                             │
│  │ ...              │                                             │
│  └──────────────────┘                                             │
│                                                                    │
│  ┌──────────────────┐                                             │
│  │   Invitations    │                                             │
│  ├──────────────────┤                                             │
│  │ Id               │                                             │
│  │ CompanyId        │───────► Companies                          │
│  │ InvitedByUserId  │───────► Users                              │
│  │ Token            │                                             │
│  │ Status           │                                             │
│  └──────────────────┘                                             │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      COMPANY DATABASE                                │
│              (SmartLeads_Company_{CompanyId})                        │
│                                                                      │
│  ┌──────────────┐          ┌──────────────────┐                     │
│  │   Contacts   │◄────────┤  ContactGroups   │                     │
│  ├──────────────┤          ├──────────────────┤                     │
│  │ Id           │          │ ContactId        │                     │
│  │ FirstName    │          │ GroupId          │                     │
│  │ UserId       │          └──────────────────┘                     │
│  │ ...          │                                                  │
│  └──────┬───────┘          ┌──────────────────┐                     │
│         │                  │     Groups       │                     │
│         │                  ├──────────────────┤                     │
│         │                  │ Id               │                     │
│         │                  │ Name             │                     │
│         │                  │ UserId           │                     │
│         │                  └──────────────────┘                     │
│         │                                                           │
│         │                  ┌──────────────────┐                     │
│         └─────────────────►│  ContactTags     │                     │
│                            ├──────────────────┤                     │
│                            │ ContactId        │                     │
│                            │ TagId            │                     │
│                            └────────┬─────────┘                     │
│                                     │                                │
│                                     ▼                                │
│                            ┌──────────────────┐                     │
│                            │      Tags        │                     │
│                            ├──────────────────┤                     │
│                            │ Id               │                     │
│                            │ Name             │                     │
│                            │ UserId           │                     │
│                            └──────────────────┘                     │
│                                                                      │
│  ┌──────────────┐          ┌──────────────────┐                     │
│  │    Notes     │          │  Attachments     │                     │
│  ├──────────────┤          ├──────────────────┤                     │
│  │ Id           │          │ Id               │                     │
│  │ ContactId    │          │ ContactId        │                     │
│  │ UserId       │          │ FileName         │                     │
│  │ Title        │          │ FilePath         │                     │
│  │ Content      │          └──────────────────┘                     │
│  └──────────────┘                                                  │
│                                                                      │
│  ┌──────────────────┐                                              │
│  │  ColumnFilters   │                                              │
│  ├──────────────────┤                                              │
│  │ Id               │                                              │
│  │ CreatedByUserId  │                                              │
│  │ ListName         │                                              │
│  │ KeyValue         │                                              │
│  └──────────────────┘                                              │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Key Design Decisions

### 1. Separation of Concerns
- **User** contains only authentication and basic profile data
- **Employee** contains company-specific employment information
- This allows one user to work for multiple companies with different roles

### 2. Many-to-Many Relationships
- **User ↔ Company** via `UserCompany` (with `IsDefault` flag)
- **User ↔ Employee** via `EmployeeUser` (with `IsPrimary` flag)
- **Contact ↔ Group** via `ContactGroup`
- **Contact ↔ Tag** via `ContactTag`

### 3. Multi-Tenancy
- System database is shared across all companies
- Each company has its own isolated database for business data
- Company context determines which database to query

### 4. Audit Trail
- All entities inherit from `BaseSystemEntity` or `BaseEntity`
- Automatic tracking of `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`

---

## Usage Examples

### Get User's Companies
```csharp
var userCompanies = await _systemDbContext.UserCompanies
    .Include(uc => uc.Company)
    .Where(uc => uc.UserId == currentUserId)
    .ToListAsync();
```

### Get Employee Record for Current Company
```csharp
var employee = await _systemDbContext.Employees
    .FirstOrDefaultAsync(e => 
        e.CompanyId == currentCompanyId && 
        e.EmployeeUsers.Any(eu => eu.UserId == currentUserId));
```

### Get User's Default Company
```csharp
var defaultCompany = await _systemDbContext.UserCompanies
    .Include(uc => uc.Company)
    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.IsDefault);
```
