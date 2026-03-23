# SmartLeads Database Structure

## Database Configuration

Your SmartLeads system uses **2 databases**:

### 1. SmartLeadsSystemDb
**Connection String:** `SystemConnection`

**Purpose:** System-wide data that is shared across all companies/tenants

**Tables:**
- `Users` - User accounts and authentication
- `Companies` - Company/organization records
- `UserCompanies` - Links users to companies (with IsDefault flag)
- `Employees` - Company-specific employee records
- `EmployeeUsers` - Links employees to users (with IsPrimary flag)
- `Invitations` - Company invitations to users

**DbContext:** `SystemDbContext`

---

### 2. SmartLeadsDb
**Connection String:** `CompanyConnection`

**Purpose:** Company-specific business data (shared by all companies in single database, filtered by CompanyId)

**Tables:**
- `Contacts` - Company contacts/leads
- `Groups` - Contact groups
- `Tags` - Contact tags
- `Notes` - Contact notes
- `Attachments` - Contact file attachments
- `ColumnFilters` - User-defined column filters
- `ContactGroups` - Contact ↔ Group junction table
- `ContactTags` - Contact ↔ Tag junction table

**DbContext:** `CompanyDbContext`

---

## Connection Strings (appsettings.json)

```json
{
  "ConnectionStrings": {
    "SystemConnection": "Host=localhost;Port=5432;Database=SmartLeadsSystemDb;Username=borhanuddin;Password=borhan444",
    "CompanyConnection": "Host=localhost;Port=5432;Database=SmartLeadsDb;Username=borhanuddin;Password=borhan444"
  }
}
```

---

## Architecture Notes

### Multi-Tenancy Approach

The system uses a **hybrid approach**:

1. **System Database** - Single database for all system-level data
   - Users can belong to multiple companies
   - Company information is centralized
   - Employee records link users to companies

2. **Company Database** - Single shared database for all company business data
   - All companies share the same database
   - Data is filtered by `CompanyId` where applicable
   - Simpler than creating separate database per company

### When to Use Each Database

**Use `SystemDbContext` when:**
- Authenticating users
- Managing user accounts
- Creating/managing companies
- Linking users to companies
- Managing employee records
- Sending company invitations

**Use `CompanyDbContext` when:**
- Managing contacts
- Creating groups and tags
- Adding notes to contacts
- Uploading attachments
- Managing column filters

---

## Applying Migrations

### System Database
```bash
cd src/SmartLeads.Infrastructure
dotnet ef database update --context SystemDbContext
```

### Company Database
```bash
cd src/SmartLeads.Infrastructure
dotnet ef database update --context CompanyDbContext
```

---

## Data Flow Example

### User Registration → Company Join Flow

```
1. User Registers
   ↓ (SystemDbContext)
   - Creates User in SmartLeadsSystemDb
   
2. User Joins Company
   ↓ (SystemDbContext)
   - Creates UserCompany in SmartLeadsSystemDb
   - Creates Employee in SmartLeadsSystemDb
   - Creates EmployeeUser in SmartLeadsSystemDb
   
3. User Creates Contact
   ↓ (CompanyDbContext)
   - Creates Contact in SmartLeadsDb
   - Contact.UserId = Current User Id
   - (Optionally filter by CompanyId if needed)
```

---

## Key Design Decisions

### Why 2 Databases Instead of 1?

1. **Separation of Concerns**
   - System data (users, companies) is separate from business data (contacts, notes)
   - Easier to backup and maintain

2. **Security**
   - System database has sensitive authentication data
   - Company database has business data
   - Can apply different security policies

3. **Scalability**
   - System database is read-heavy (authentication)
   - Company database is write-heavy (CRUD operations)
   - Can scale independently if needed

### Why Not Separate Database Per Company?

Your current setup uses a **single shared database** for all company business data instead of creating a separate database per company because:

1. **Simpler Maintenance** - Only one database to manage
2. **Easier Queries** - No need to switch connections
3. **Cost Effective** - Less database overhead
4. **Sufficient Isolation** - CompanyId filtering provides logical separation

If you need physical isolation per company in the future, you can:
1. Create separate databases: `SmartLeads_Company_{CompanyId}`
2. Update `CompanyDbContext` connection string dynamically
3. Use the `ICompanyDbContextFactory` to create per-company contexts

---

## Troubleshooting

### Connection String Issues

**Error:** "Company connection string not found"
- Check `appsettings.json` has `CompanyConnection`
- Ensure connection string is valid

**Error:** "Failed to connect to database"
- Check PostgreSQL is running
- Verify username/password
- Ensure database exists

### Migration Issues

**Error:** "Table doesn't exist"
- Run migrations: `dotnet ef database update`

**Error:** "Context is not configured"
- Check DbContext is registered in `DependencyInjection.cs`

---

## Future Enhancements

### Option 1: Add CompanyId to Company Database Tables

If you want to support multiple companies in SmartLeadsDb:

```csharp
public class Contact : BaseEntity
{
    public Guid CompanyId { get; set; }  // Add this
    public Company Company { get; set; } // Add this
    // ... other properties
}
```

### Option 2: Separate Database Per Company

Modify `CompanyDbContextFactory` to create dynamic connection strings:

```csharp
public CompanyDbContext Create(Guid companyId)
{
    var connectionString = $"Host=localhost;Database=SmartLeads_Company_{companyId};...";
    return new CompanyDbContext(connectionString);
}
```

---

## Summary

✅ **SmartLeadsSystemDb** - Users, Companies, Employee relationships  
✅ **SmartLeadsDb** - Contacts, Groups, Tags, Notes (business data)  
✅ **2 Databases** - Clean separation, easy to maintain  
✅ **Ready to Use** - Migrations created, configuration complete
