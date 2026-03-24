# User Registration and Company Check Feature

## Overview

This document describes the newly implemented user registration system with automatic company association checking.

---

## Features Implemented

### 1. User Registration

**Location:** `/Auth/Register`

**Description:** Allows new users to create an account with basic information.

**Flow:**
1. User fills registration form (username, email, password, name)
2. System creates user account
3. User is automatically logged in
4. User is redirected to "No Company" page since they have no company association

---

### 2. No Company Page

**Location:** `/Auth/NoCompany`

**Description:** Displayed when a logged-in user is not associated with any company.

**Options Available:**
- **Join Existing Company** - Enter company code to join
- **Create New Company** - Create your own company
- **Sign Out** - Logout from account

**Layout:** `_NoCompanyLayout.cshtml` - Standalone page with gradient background

---

### 3. Company Check Filter

**Attribute:** `[RequireCompany]`

**Location:** `SmartLeads.Web.Filters.RequireCompanyAttribute`

**Usage:** Apply to controllers that require company association

**Example:**
```csharp
[RequireCompany]
public class ContactsController : Controller
{
    // Actions...
}
```

**Behavior:**
- Checks if user has any active company association
- Redirects to `/Auth/NoCompany` if no company found
- Allows request to proceed if user has company

---

## Files Created/Modified

### New Files:

1. **Views/Auth/Register.cshtml**
   - User registration form
   - Validation for all fields
   - Link to login page

2. **Views/Auth/_NoCompanyLayout.cshtml**
   - Standalone layout for users without company
   - Two options: Join or Create company
   - Modern gradient design

3. **Filters/RequireCompanyAttribute.cs**
   - Action filter for company checking
   - Injects via dependency injection
   - Checks UserCompany table

### Modified Files:

1. **Domain/DTOs/AuthViewModels.cs**
   - Added `RegisterViewModel` class
   - Validation attributes for registration

2. **Controllers/AuthController.cs**
   - Added `Register()` GET/POST actions
   - Added `NoCompany()` action
   - Added `JoinCompany()` action (placeholder)
   - Added `CreateCompany()` action (placeholder)
   - Updated Login to check company association

3. **Controllers/ContactsController.cs**
   - Added `[RequireCompany]` attribute
   - Example for other controllers

4. **Services/Interface/IUserService.cs**
   - Added `GetUserCompaniesAsync()` method

5. **Services/Implementation/UserService.cs**
   - Implemented `GetUserCompaniesAsync()`
   - Queries UserCompany table

---

## Registration Flow

```
┌─────────────────┐
│  User Visits    │
│  /Auth/Register │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Fill Registration│
│ Form            │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Submit Form     │
│ (POST)          │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Validate Model  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Create User     │
│ (No Company)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Auto Login      │
│ (Set Cookies)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Check Companies │
│ (None Found)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Redirect to     │
│ /Auth/NoCompany │
└─────────────────┘
```

---

## Login Flow (Updated)

```
┌─────────────────┐
│  User Logs In   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Validate        │
│ Credentials     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Set Cookies     │
│ (JwtToken,      │
│  UserId)        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Check User's    │
│ Companies       │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌─────────┐ ┌──────────┐
│ Has     │ │ No       │
│ Company │ │ Company  │
└────┬────┘ └────┬─────┘
     │           │
     │           ▼
     │    ┌──────────────┐
     │    │ Redirect to  │
     │    │ NoCompany    │
     │    └──────────────┘
     │
     ▼
┌──────────────┐
│ Redirect to  │
│ Contacts     │
└──────────────┘
```

---

## Model Validation

### RegisterViewModel

```csharp
[Required]
[StringLength(50, MinimumLength = 3)]
public string Username { get; set; }

[Required]
[EmailAddress]
public string Email { get; set; }

[Required]
[StringLength(100, MinimumLength = 6)]
[DataType(DataType.Password)]
public string Password { get; set; }

[DataType(DataType.Password)]
[Compare("Password")]
public string ConfirmPassword { get; set; }

public string? FirstName { get; set; }
public string? LastName { get; set; }
```

---

## Usage Examples

### Apply Company Check to Controller

```csharp
using SmartLeads.Web.Filters;

[RequireCompany]
public class UsersController : Controller
{
    // All actions will check for company association
}
```

### Apply to Specific Action

```csharp
public class DashboardController : Controller
{
    [RequireCompany]
    public IActionResult Index()
    {
        // Only accessible if user has company
    }
    
    public IActionResult Public()
    {
        // Accessible without company
    }
}
```

---

## Future Enhancements

### 1. Join Company Implementation

```csharp
[HttpPost]
public async Task<IActionResult> JoinCompany(string companyCode)
{
    // TODO: 
    // 1. Find company by code
    // 2. Create UserCompany record
    // 3. Create Employee record
    // 4. Create EmployeeUser record
    // 5. Redirect to dashboard
}
```

### 2. Create Company Implementation

```csharp
[HttpPost]
public async Task<IActionResult> CreateCompany()
{
    // TODO:
    // 1. Create Company record
    // 2. Create UserCompany record (IsDefault = true)
    // 3. Create Employee record
    // 4. Create EmployeeUser record (IsPrimary = true)
    // 5. Redirect to company dashboard
}
```

### 3. Company Selection Page

For users with multiple companies, add a company selection page before redirecting to dashboard.

---

## Testing

### Test Scenarios:

1. **New User Registration**
   - ✓ Register with valid data
   - ✓ Auto-login after registration
   - ✓ Redirect to NoCompany page

2. **Login Without Company**
   - ✓ Login with valid credentials
   - ✓ Check company association
   - ✓ Redirect to NoCompany if none

3. **Login With Company**
   - ✓ Login with valid credentials
   - ✓ Check company association
   - ✓ Redirect to Contacts if has company

4. **Protected Pages**
   - ✓ Try to access Contacts without company
   - ✓ Should redirect to NoCompany page
   - ✓ Try to access Contacts with company
   - ✓ Should allow access

---

## Security Considerations

1. **Anti-Forgery Tokens:** All POST forms include `@Html.AntiForgeryToken()`
2. **Password Validation:** Minimum 6 characters, confirmation required
3. **Email Validation:** Proper email format required
4. **Username Validation:** 3-50 characters, alphanumeric recommended
5. **Cookie Security:** HttpOnly, Secure, SameSite=Strict

---

## Styling

The `_NoCompanyLayout.cshtml` uses:
- Bootstrap 5.3.0
- Font Awesome 6.4.0
- Google Fonts (Inter)
- Custom gradient background
- Smooth animations
- Responsive design

---

## Next Steps

1. Implement actual company joining logic
2. Implement company creation logic
3. Add company code generation
4. Add invitation system integration
5. Add company selection for multi-company users
