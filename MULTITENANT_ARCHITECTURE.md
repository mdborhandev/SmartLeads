# Multi-Tenant Database Architecture

## Overview

SmartLeads now uses a **multi-tenant database architecture** where:
- **System Database** (`SmartLeadsSystemDb`): Contains system-wide data (Users, Companies, User-Company mappings)
- **Company Databases** (`SmartLeads_Company_{Id}`): Each company has its own isolated database for their data

## Database Structure

### 1. System Database (`SmartLeadsSystemDb`)

Contains:
- **Users**: User accounts (authentication, profile)
- **Companies**: Company/organization records
- **UserCompanies**: Junction table linking Users to Companies with employee details
- **Invitations**: User invitations to join companies

#### Key Entity: UserCompany

The `UserCompany` table is the heart of the multi-tenant system:
- Links a User to a Company
- Contains employee-specific information per company:
  - `EmployeeId`: Unique employee code (e.g., EMP001)
  - `Department`: Department name
  - `Designation`: Job title
  - `PhoneNumber`: Contact number
  - `Address`: Address
  - `DateOfJoining`: Joining date
  - `IsActive`: Active status in this company

**Example:**
```
User: John Doe (john@example.com)
├── UserCompany 1: Company A - EmployeeId: EMP001, Department: Sales
└── UserCompany 2: Company B - EmployeeId: MGR005, Department: Management
```

### 2. Company Database (per company)

Each company has its own database containing:
- **Contacts**: Company's contacts/leads
- **Groups**: Contact groups
- **Tags**: Contact tags
- **Notes**: Notes on contacts
- **Attachments**: Contact attachments
- **ColumnFilters**: User-defined column filters

## Connection Strings

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SystemConnection": "Host=localhost;Port=5432;Database=SmartLeadsSystemDb;Username=...;Password=...",
    "CompanyConnection": "Host=localhost;Port=5432;Database=SmartLeadsCompanyDb;Username=...;Password=..."
  }
}
```

## How It Works

### User Flow

1. **User logs in** → System validates against `SystemDbContext`
2. **User selects company** → System loads user's companies from `UserCompanies` table
3. **Company context set** → All subsequent queries use `CompanyDbContext` for that company's database
4. **Data operations** → Contacts, Groups, Tags, etc. are stored in the company's database

### Code Example

```csharp
// Get current user's companies
var userCompanies = await _companyContext.GetUserCompaniesAsync();

// Set current company
_companyContext.SetCurrentCompany(selectedCompanyId);

// Create company-specific DbContext
using var companyDb = _dbContextFactory.Create(selectedCompanyId);
var contacts = await companyDb.Contacts.ToListAsync();
```

## Migrations

### System Database Migrations
```bash
dotnet ef migrations add InitialSystemDb --context SystemDbContext
dotnet ef database update --context SystemDbContext
```

### Company Database Migrations
```bash
dotnet ef migrations add InitialCompanyDb --context CompanyDbContext
dotnet ef database update --context CompanyDbContext
```

## Benefits

1. **Data Isolation**: Each company's data is completely isolated
2. **Scalability**: Can move large companies to dedicated servers
3. **Compliance**: Easier to meet data residency requirements
4. **Performance**: Smaller, focused databases per company
5. **Backup/Restore**: Can backup/restore individual companies

## Key Classes

| Class | Purpose |
|-------|---------|
| `SystemDbContext` | DbContext for system database |
| `CompanyDbContext` | DbContext for company database |
| `ICompanyContext` | Tracks current user's company |
| `ICompanyDbContextFactory` | Creates company-specific DbContext instances |
| `UserCompany` | Junction table with employee details |

## Migration Guide

### Creating a New Company

1. Create Company record in System Database:
```csharp
var company = new Company { Name = "New Company", Code = "NC" };
systemDbContext.Companies.Add(company);
await systemDbContext.SaveChangesAsync();
```

2. Create company database:
```sql
CREATE DATABASE "SmartLeads_Company_{companyId:N}";
```

3. Run migrations on company database:
```bash
dotnet ef database update --context CompanyDbContext --connection "Host=...;Database=SmartLeads_Company_{companyId:N};..."
```

4. Add user to company:
```csharp
var userCompany = new UserCompany
{
    UserId = userId,
    CompanyId = companyId,
    EmployeeId = "EMP001",
    Department = "Sales",
    Designation = "Manager"
};
systemDbContext.UserCompanies.Add(userCompany);
await systemDbContext.SaveChangesAsync();
```

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    System Database                           │
│              (SmartLeadsSystemDb)                            │
│  ┌──────────┐  ┌───────────┐  ┌──────────────┐             │
│  │  Users   │  │ Companies │  │ UserCompanies│◄────┐       │
│  └──────────┘  └───────────┘  └──────────────┘     │       │
│                     │                │              │       │
│                     └────────────────┼──────────────┘       │
│                                      │                      │
└──────────────────────────────────────┼──────────────────────┘
                                       │
            ┌──────────────────────────┼──────────────────────┐
            │                          │                      │
            ▼                          ▼                      ▼
    ┌───────────────┐          ┌───────────────┐     ┌───────────────┐
    │ Company A DB  │          │ Company B DB  │     │ Company C DB  │
    │ ┌───────────┐ │          │ ┌───────────┐ │     │ ┌───────────┐ │
    │ │ Contacts  │ │          │ │ Contacts  │ │     │ │ Contacts  │ │
    │ │ Groups    │ │          │ │ Groups    │ │     │ │ Groups    │ │
    │ │ Tags      │ │          │ │ Tags      │ │     │ │ Tags      │ │
    │ │ Notes     │ │          │ │ Notes     │ │     │ │ Notes     │ │
    │ └───────────┘ │          │ └───────────┘ │     │ └───────────┘ │
    └───────────────┘          └───────────────┘     └───────────────┘
```
