# V1 Enterprise Company Management System - Implementation Plan

> **Status: ~75% Complete** | Last verified: May 16, 2026 (QC performed)
>
> ✅ **Completed:** Auth (Register/Login/Logout/JWT/Cookies/WorkspaceSwitch), Company Mgmt, Employee CRUD, Multi-Company (Switch/Isolation/Default), Company Setup automation, Database/Infrastructure, UI Pages, API Endpoints
> ⏳ **Partially:** Invitation Sending (send works, accept broken), RBAC (role enum done, service/attributes pending), Child Company hierarchy display
> ❌ **Broken/Missing:** Invitation Acceptance flow (no controller/view for `/Invitations/Accept`), Role service, Permission attributes, Auth middleware, Seed data, Rate limiting
>
> **QC CRITICAL FINDING:** Invitation email links to `/Invitations/Accept?token=...&email=...` but **no route, controller, or view exists** for this URL — returns HTTP 404.

---

## 1. System Architecture

### Architecture Overview
```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  MVC Controllers + Razor Views + Web API Controllers        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Service Layer                            │
│  AuthService, CompanyService, EmployeeService, InvitationService │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Repository Layer                          │
│  Generic Repository + Specific Repositories                 │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer                         │
│  EF Core + PostgreSQL                                        │
└─────────────────────────────────────────────────────────────┘
```

### Multi-Company Design Pattern
```
User → HasMany → UserCompanyRole → HasOne → Company
                                        → HasOne → Role

Employee → HasMany → EmployeeCompany → HasOne → Company
                                    → HasOne → Department
                                    → HasOne → Designation
```

---

## 2. Database Design

### Core Tables

#### Users
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key |
| Username | string | Unique |
| Email | string | Unique |
| PasswordHash | string | Hashed password |
| FirstName | string | |
| LastName | string | |
| ProfilePicture | string | Path to image |
| IsActive | bool | |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

#### Companies
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key |
| Name | string | Company name |
| Code | string | Unique code |
| Email | string | |
| Phone | string | |
| Address | string | |
| IsParent | bool | Is parent company |
| ParentCompanyId | Guid? | FK to parent |
| IsActive | bool | |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

#### Employees
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key |
| EmployeeId | string | Company-specific ID |
| FirstName | string | |
| LastName | string | |
| MiddleName | string? | |
| WorkEmail | string | Company email |
| PersonalEmail | string? | |
| PhoneNumber | string? | |
| DateOfBirth | DateTime? | |
| Gender | string? | |
| IsActive | bool | |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

#### Roles
| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary Key |
| Name | string | Role name |
| Description | string | |

### Relationship Tables

#### UserCompanyRoles
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key |
| UserId | Guid | FK to Users |
| CompanyId | Guid | FK to Companies |
| RoleId | int | FK to Roles |
| IsDefault | bool | Default company |
| IsActive | bool | |
| CreatedAt | DateTime | |

#### EmployeeCompanies
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key |
| EmployeeId | Guid | FK to Employees |
| CompanyId | Guid | FK to Companies |
| DepartmentId | Guid? | FK to Departments |
| DesignationId | Guid? | FK to Designations |
| DateOfJoining | DateTime | |
| IsActive | bool | |

#### Invitations
| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key |
| Email | string | Invitee email |
| Token | string | Unique token |
| CompanyId | Guid | FK to Companies |
| EmployeeId | Guid? | FK to Employees |
| RoleId | int | FK to Roles |
| InvitedByUserId | Guid | FK to Users |
| ExpiresAt | DateTime | Expiry date |
| IsAccepted | bool | |
| AcceptedAt | DateTime? | |
| Status | string | Pending/Accepted/Expired |
| CreatedAt | DateTime | |

### Entity Relationship Diagram
```
Users ─────────┐
               │
               ▼
        ┌──────────────┐
        │UserCompanyRoles│
        └──────┬─────────┘
               │
        ┌──────┴─────────┐
        ▼                ▼
   Companies         Roles
        │
        │ (Parent/Child)
        ▼
   Companies (self-ref)

Companies ────────┐
                  │
                  ▼
         ┌───────────────┐
         │EmployeeCompanies│
         └───────┬─────────┘
                 │
                 ▼
           Employees
                 │
                 ▼
         ┌───────────────┐
         │ EmployeeUsers  │ (links to Users)
         └───────────────┘
```

---

## 3. Feature Breakdown

### 3.1 Authentication
- [x] User Registration
- [x] User Login
- [x] JWT Token Generation
- [x] Cookie-based Session
- [x] Workspace Switching
- [x] Logout

### 3.2 Company Management
- [x] Create Company (during setup)
- [x] Create Child Company
- [x] View Company Details
- [x] Edit Company Info
- [ ] Company Hierarchy Display
- [x] Parent-Child Relationship

### 3.3 Employee Management
- [x] Create Employee (manual)
- [x] Edit Employee
- [x] Delete Employee (soft delete)
- [x] View Employee List
- [ ] Employee Details View
- [x] Multi-company Employee Support

### 3.4 Invitation System
- [x] Send Invitation
- [x] Invitation Email Template
- [ ] Accept Invitation (GET) — BROKEN: no controller/route for `/Invitations/Accept`
- [ ] Accept Invitation (POST) — BROKEN: no controller/route
- [ ] Token Validation — BROKEN: not exposed via web endpoint
- [ ] Expiry Validation — BROKEN: not exposed via web endpoint
- [ ] Attach Existing User — BROKEN: no acceptance flow
- [ ] Create New User on Accept — BROKEN: no acceptance flow

### 3.5 RBAC
- [x] Role Definitions (SuperAdmin, Admin, Manager, User)
- [x] Company-specific Role Assignment
- [ ] Permission Attributes
- [ ] Role-based View Rendering
- [ ] API Authorization

### 3.6 Multi-Company Support
- [x] Switch Company Context
- [x] Default Company Selection
- [x] Company-specific Data Isolation
- [x] Cross-company User Access

---

## 4. Step-by-Step Implementation Order

### Phase 1: Foundation
1. ✅ Create project structure (Clean Architecture folders)
2. ✅ Setup EF Core DbContext
3. ✅ Create all entity models
4. ✅ Run migrations
5. ✅ Setup dependency injection

### Phase 2: Core Services
6. ✅ Create base repository interface/implementation
7. ✅ Create unit of work
8. ✅ Create service layer base classes

### Phase 3: Authentication
9. ✅ Implement User registration
10. ✅ Implement User login
11. ✅ Implement JWT token generation
12. ✅ Implement company context middleware

### Phase 3b: Company Setup (Current Gap)
13. ✅ Create Setup/Company controller (UserCompanyController)
14. ✅ Create company setup view
15. ✅ Implement auto-creation of:
    - Owner Employee
    - EmployeeCompany (EmployeeUser)
    - UserCompanyRole with SuperAdmin

### Phase 4: Employee Management
16. ✅ Create Employee repository
17. ✅ Create Employee controller
18. ✅ Create employee list view
19. ✅ Create employee create/edit views

### Phase 5: Invitation System (Current Gap)
20. ❌ Create Invitation controller (MISSING — no /Invitations/Accept route)
21. ❌ Create invitation accept view (MISSING — no Accept.cshtml)
22. ❌ Implement token validation (no web endpoint)
23. ❌ Implement existing user attach logic (no web endpoint)
24. ❌ Implement new user creation logic (no web endpoint)

### Phase 6: RBAC
25. ⏳ Create role service (enum defined, pending dedicated service)
26. ⏳ Create authorization middleware (RequireCompanyAttribute exists)
27. ⏳ Add role-based attributes
28. ⏳ Create permission system

### Phase 7: Multi-Company Features
29. ✅ Implement company switcher
30. ✅ Implement company isolation
31. ✅ Create child company feature

---

## 5. Clean Enterprise Folder Structure

```
src/
├── SmartLeads.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Company.cs
│   │   ├── Employee.cs
│   │   ├── Role.cs
│   │   ├── UserCompanyRole.cs
│   │   ├── EmployeeCompany.cs
│   │   ├── Invitation.cs
│   │   ├── Department.cs
│   │   └── Designation.cs
│   ├── Enums/
│   │   ├── UserRole.cs
│   │   └── InvitationStatus.cs
│   └── Interfaces/
│       ├── IRepository.cs
│       ├── IUnitOfWork.cs
│       └── IService.cs
│
├── SmartLeads.Application/
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Company/
│   │   ├── Employee/
│   │   └── Invitation/
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── ICompanyService.cs
│   │   │   ├── IEmployeeService.cs
│   │   │   ├── IInvitationService.cs
│   │   │   └── IRoleService.cs
│   │   └── Implementations/
│   │       ├── AuthService.cs
│   │       ├── CompanyService.cs
│   │       ├── EmployeeService.cs
│   │       ├── InvitationService.cs
│   │       └── RoleService.cs
│   └── Behaviors/
│       ├── LoggingBehavior.cs
│       └── ValidationBehavior.cs
│
├── SmartLeads.Infrastructure/
│   ├── Data/
│   │   ├── SmartLeadsDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── GenericRepository.cs
│   │   ├── UnitOfWork.cs
│   │   └── CompanyRepository.cs
│   └── Services/
│       ├── EmailService.cs
│       ├── JwtTokenService.cs
│       └── PasswordHasher.cs
│
└── SmartLeads.Web/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── SetupController.cs          (NEW)
    │   ├── CompanyController.cs
    │   ├── EmployeeController.cs
    │   ├── InvitationController.cs     (NEW)
    │   └── UserController.cs
    ├── Views/
    │   ├── Auth/
    │   ├── Setup/
    │   │   └── Company.cshtml          (NEW)
    │   ├── Company/
    │   ├── Employee/
    │   ├── Invitation/
    │   │   └── Accept.cshtml           (NEW)
    │   └── User/
    ├── Middleware/
    │   ├── CompanyContextMiddleware.cs (NEW)
    │   └── AuthorizationMiddleware.cs
    ├── Filters/
    │   ├── RequireCompanyAttribute.cs
    │   └── PermissionAttribute.cs
    └── wwwroot/
```

---

## 6. Complete V1 TODO Checklist

### Database & Infrastructure
- [x] Create all entity models
- [x] Setup DbContext with relationships
- [x] Add DbSets for all entities
- [x] Run initial migration
- [ ] Add seed data for default roles

### Authentication
- [x] User Register endpoint
- [x] User Login endpoint
- [x] JWT token generation
- [x] Cookie authentication setup
- [x] Logout endpoint

### Company Setup (Current Gap)
- [x] Create UserCompanyController
- [x] Create /setup/company route
- [x] Create company setup view
- [x] Auto-create Employee on company creation
- [x] Auto-create EmployeeUser record
- [x] Auto-create UserCompanyRole with SuperAdmin
- [x] Auto-set as default company

### Employee Management
- [x] Employee List endpoint
- [x] Employee Create endpoint
- [x] Employee Update endpoint
- [x] Employee Delete (soft) endpoint
- [x] Employee List view
- [x] Employee Create/Edit view

### Invitation System (Current Gap)
- [ ] Create InvitationController (MISSING — no controller exists)
- [ ] Create /invitations/accept GET route (MISSING — route returns 404)
- [ ] Create accept invitation view (MISSING — no Accept.cshtml)
- [ ] Validate token exists (no web endpoint calls repository method)
- [ ] Validate token not expired (no web endpoint)
- [ ] Validate token not used (no web endpoint)
- [ ] Handle existing user case (no acceptance flow)
- [ ] Handle new user case (no acceptance flow)
- [ ] Create User on accept (no acceptance flow)
- [ ] Link user to Employee (no acceptance flow)
- [ ] Create UserCompanyRole (no acceptance flow)
- [ ] Update Invitation status (no acceptance flow)
- [ ] Auto-login after accept (no acceptance flow)

### RBAC
- [x] Define role enum/constants (UserRole enum)
- [ ] Create role service
- [ ] Create authorization middleware
- [x] Add [Authorize] attributes
- [ ] Create RequireRoleAttribute
- [ ] Create PermissionAttribute
- [ ] Implement role-based view logic

### Multi-Company Support
- [x] Company context middleware/service (CompanyContext)
- [x] Get current company service
- [x] Switch company endpoint
- [x] Default company selection
- [x] Company isolation for queries

### Child Company
- [x] Create child company endpoint
- [x] Parent-Child dropdown UI
- [x] Auto-assign SuperAdmin to creator
- [ ] Hierarchy display

### Middleware & Security
- [x] CompanyContextMiddleware (CompanyContext service)
- [ ] Request validation middleware
- [x] Token expiry validation (JWT expiry)
- [ ] Rate limiting for auth endpoints
- [x] SQL injection prevention (EF Core parameterized queries)
- [x] XSS prevention (Razor auto-encoding)

### UI/Pages
- [x] Login page
- [x] Register page
- [x] Company setup page
- [x] Dashboard
- [x] Employee list
- [x] Employee create/edit
- [x] User list
- [x] Invite user form
- [ ] Accept invitation page — BROKEN: no view exists
- [x] Company switcher UI

### API Endpoints
- [x] POST /api/auth/register
- [x] POST /api/auth/login
- [x] POST /api/setup/company
- [x] GET /api/companies
- [x] POST /api/companies/child
- [x] GET /api/employees
- [x] POST /api/employees
- [x] PUT /api/employees/{id}
- [x] DELETE /api/employees/{id}
- [x] POST /api/invitations
- [ ] GET /api/invitations/accept/{token} — BROKEN: no route
- [ ] POST /api/invitations/accept — BROKEN: no route
- [x] POST /api/companies/switch

---

## 7. Recommended Services & Classes

### Services to Create

```csharp
// ICompanyService
public interface ICompanyService
{
    Task<Company> CreateAsync(CreateCompanyDto dto, Guid userId);
    Task<Company> CreateChildCompanyAsync(CreateChildCompanyDto dto, Guid userId);
    Task<Company> GetByIdAsync(Guid id);
    Task<IEnumerable<Company>> GetUserCompaniesAsync(Guid userId);
    Task SwitchCompanyAsync(Guid userId, Guid companyId);
}

// IEmployeeService
public interface IEmployeeService
{
    Task<Employee> CreateAsync(CreateEmployeeDto dto, Guid companyId);
    Task<Employee> UpdateAsync(Guid id, UpdateEmployeeDto dto);
    Task<IEnumerable<Employee>> GetByCompanyAsync(Guid companyId);
    Task<Employee> GetByIdAsync(Guid id);
}

// IInvitationService
public interface IInvitationService
{
    Task<Invitation> CreateAsync(CreateInvitationDto dto, Guid invitedByUserId);
    Task<Invitation> ValidateTokenAsync(string token);
    Task<AcceptInvitationResult> AcceptAsync(AcceptInvitationDto dto);
}

// ICompanySetupService (NEW)
public interface ICompanySetupService
{
    Task<CompanySetupResult> SetupCompanyAsync(SetupCompanyDto dto, Guid userId);
}
```

### Classes to Implement

1. **CompanyContextService** - Manage current company state
2. **PermissionService** - Check user permissions
3. **InvitationTokenService** - Generate/validate tokens
4. **EmailTemplateService** - Generate invitation emails
5. **AuditService** - Log important actions

---

## 8. Middleware Requirements

### Required Middleware

1. **CompanyContextMiddleware**
   - Extract company ID from token/cookie
   - Set ICompanyContext service
   - Handle switch company

2. **AuthenticationMiddleware**
   - Validate JWT token
   - Set HttpContext.User
   - Handle token refresh

3. **RequestLoggingMiddleware**
   - Log all requests
   - Track execution time

4. **ErrorHandlingMiddleware**
   - Global exception handling
   - Return proper error responses

---

## 9. Security Considerations

### Token Security
- Use cryptographically secure tokens
- Implement token expiry
- Single-use token validation
- Store token hash in database

### Company Isolation
- Always filter by CurrentCompanyId
- Use company context in all queries
- Validate user belongs to company before access

### Role-Based Access
- Validate role per company
- SuperAdmin can do everything in their company
- Regular users restricted to assigned permissions

### Input Validation
- Validate all inputs
- Sanitize string inputs
- Use model validation attributes

### Password Security
- Hash passwords (bcrypt/argon2)
- Never store plain text
- Validate password strength

---

## 10. Implementation Priority

### HIGH PRIORITY (V1 Must-Have)
1. Company Setup automation (auto-create Employee + Role)
2. Invitation Acceptance flow (create missing controller)
3. Company Context middleware
4. Basic RBAC

### MEDIUM PRIORITY (V1 Should-Have)
5. Child company creation
6. Company switcher UI
7. Employee management
8. Permission attributes

### LOW PRIORITY (V1 Nice-to-Have)
9. Advanced logging
10. Audit trails
11. Activity notifications

---

## 11. Current Gaps to Fill

### Gap 1: Company Setup Automation
**Problem:** Currently creates company but need to verify Employee + UserCompanyRole creation happens automatically

**Solution:** Ensure UserCompanyController.CreateCompany does:
- [x] Create Company
- [x] Create Employee (for creator)
- [x] Create EmployeeUser (link to User)
- [x] Create UserCompany with SuperAdmin role

### Gap 2: Invitation Acceptance
**Problem:** /Invitations/Accept route doesn't exist (STILL MISSING)

**Solution:** Need to create:
- [ ] InvitationController.cs
- [ ] Accept GET action
- [ ] Accept POST action
- [ ] Accept.cshtml view

### Gap 3: Company Context
**Problem:** Need consistent way to get current company

**Solution:** ICompanyContext implemented in CompanyContext service:
- [x] Set from JWT token
- [x] Updated on company switch
- [x] Used in all repository queries (via BaseEntity.CompanyId)

---

## 12. Summary

The V1 system supports:
- ✅ User registration and login
- ✅ Company setup with auto-owned employee
- ✅ Employee management (manual creation)
- ❌ Invitation system (send only — ACCEPTANCE FLOW BROKEN)
- ✅ Multi-company user access
- ✅ Child company hierarchy
- ⏳ Role-based access control (partially done)

**Key missing pieces to implement:**
1. ✅ Company setup automation (done - auto-creates Employee + EmployeeUser + UserCompanyRole)
2. ❌ Invitation acceptance controller + view (MISSING — redirection email links to /Invitations/Accept which returns 404)
3. ✅ Company context middleware enhancement (done - CompanyContext service)
4. ⏳ RBAC implementation (role service, authorization middleware, permission attributes)