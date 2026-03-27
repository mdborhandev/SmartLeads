# Per-Company Roles Implementation Summary

## Overview
Updated the SmartLeads application to support **per-company roles** instead of global user roles. Users can now have different roles in different companies.

## Key Changes

### 1. Database Schema Changes

#### UserCompany Model
- **Added**: `Role` property (UserRole enum) - stores the user's role in each specific company
- **Existing**: `IsDefault` property - identifies the user's default company

#### User Model
- **Removed**: `Role` property (was UserRole enum)
- Role is now stored per-company in UserCompany table

### 2. Files Modified

#### Domain Models
- `src/SmartLeads.Domain/Models/UserCompany.cs` - Added Role property
- `src/SmartLeads.Domain/Models/User.cs` - Removed Role property

#### Database Context
- `src/SmartLeads.Infrastructure/Persistence/SystemDbContext.cs` - Configured Role conversion

#### Services
- `src/SmartLeads.Infrastructure/Services/CompanyContext.cs` - Added `GetCurrentCompanyRoleAsync()` method

#### Repositories
- `src/SmartLeads.Infrastructure/Repositories/Implementation/UserRepository.cs` - Updated to get role from UserCompany

#### Utilities
- `src/SmartLeads.Utilities/Identity/JwtTokenGenerator.cs` - Updated to accept optional role parameter
- `src/SmartLeads.Utilities/Interfaces/IJwtTokenGenerator.cs` - Updated interface signature

#### Controllers
- `src/SmartLeads.Web/Controllers/UserCompanyController.cs` - Updated company creation to set SuperAdmin role
- `src/SmartLeads.Web/Controllers/InvitationsController.cs` - Updated to use invitation role in UserCompany
- `src/SmartLeads.Web/Controllers/AuthController.cs` - Updated login to get role from UserCompany
- `src/SmartLeads.Web/Controllers/UsersController.cs` - Updated user editing to modify role in UserCompany

#### Views
- `src/SmartLeads.Web/Views/Shared/_Layout.cshtml` - Updated role display and company switcher
- `src/SmartLeads.Web/Views/Shared/_UserCompanyLayout.cshtml` - Updated role display and company switcher

### 3. New Features

#### Company Creation
- When a user creates a company, they automatically get the **SuperAdmin** role in that company
- If the user already has a default company, the new company won't be set as default
- If this is their first company, it becomes the default

#### Invitation System
- When accepting an invitation, the user gets the role specified in the invitation
- If the user already has a default company, the new company won't override it
- Role is properly stored in UserCompany table

#### Company Switching
- When switching companies, the user's role changes based on the selected company
- JWT token is updated with the new role
- Role badge is displayed in the company switcher dropdown

#### Default Company Logic
- First company created/joined becomes the default
- Subsequent companies don't override the default
- Default company is used for login and initial session setup

## Database Migration Required

Run the following SQL script in pgAdmin to update existing database:

```sql
-- Add Role column to UserCompanies
ALTER TABLE "UserCompanies" 
ADD COLUMN "Role" VARCHAR(50) NOT NULL DEFAULT 'User';

-- Set SuperAdmin role for existing default company associations
UPDATE "UserCompanies" uc
SET "Role" = 'SuperAdmin'
WHERE uc."IsDefault" = true;

-- Remove Role column from Users table
ALTER TABLE "Users" 
DROP COLUMN "Role";
```

**File location**: `docs/DATABASE_MIGRATION_ROLE_UPDATE.sql`

## Testing Checklist

- [ ] Create a new company - verify creator gets SuperAdmin role
- [ ] Create a second company - verify it's not set as default
- [ ] Send invitation with Manager role - verify user gets Manager role
- [ ] Switch between companies - verify role changes in UI
- [ ] Check company switcher dropdown - verify role badges are displayed
- [ ] Login with multiple companies - verify default company role is used
- [ ] Edit user role in Users page - verify it updates UserCompany table

## Benefits

1. **Flexible Role Management**: Users can have different roles in different companies
2. **Better Security**: Role permissions are scoped to each company
3. **Accurate Representation**: Role badge shows actual role in current company
4. **Cleaner Data Model**: Role is stored where it belongs - in the company-user relationship

## Backward Compatibility

- Existing users will have their role set to 'User' by default
- Migration script sets SuperAdmin for existing default company associations
- JWT tokens now include company-specific role
