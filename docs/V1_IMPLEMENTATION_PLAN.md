# V1 Enterprise Company Management System - Implementation Plan

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
- [ ] User Registration
- [ ] User Login
- [ ] JWT Token Generation
- [ ] Cookie-based Session
- [ ] Workspace Switching
- [ ] Logout

### 3.2 Company Management
- [ ] Create Company (during setup)
- [ ] Create Child Company
- [ ] View Company Details
- [ ] Edit Company Info
- [ ] Company Hierarchy Display
- [ ] Parent-Child Relationship

### 3.3 Employee Management
- [ ] Create Employee (manual)
- [ ] Edit Employee
- [ ] Delete Employee (soft delete)
- [ ] View Employee List
- [ ] Employee Details View
- [ ] Multi-company Employee Support

### 3.4 Invitation System
- [ ] Send Invitation
- [ ] Invitation Email Template
- [ ] Accept Invitation (GET)
- [ ] Accept Invitation (POST)
- [ ] Token Validation
- [ ] Expiry Validation
- [ ] Attach Existing User
- [ ] Create New User on Accept

### 3.5 RBAC
- [ ] Role Definitions (SuperAdmin, Admin, Manager, User)
- [ ] Company-specific Role Assignment
- [ ] Permission Attributes
- [ ] Role-based View Rendering
- [ ] API Authorization

### 3.6 Multi-Company Support
- [ ] Switch Company Context
- [ ] Default Company Selection
- [ ] Company-specific Data Isolation
- [ ] Cross-company User Access

---

## 4. Step-by-Step Implementation Order

### Phase 1: Foundation
1. Create project structure (Clean Architecture folders)
2. Setup EF Core DbContext
3. Create all entity models
4. Run migrations
5. Setup dependency injection

### Phase 2: Core Services
6. Create base repository interface/implementation
7. Create unit of work
8. Create service layer base classes

### Phase 3: Authentication
9. Implement User registration
10. Implement User login
11. Implement JWT token generation
12. Implement company context middleware

### Phase 3b: Company Setup (Current Gap)
13. Create Setup/Company controller
14. Create company setup view
15. Implement auto-creation of:
    - Owner Employee
    - EmployeeCompany
    - UserCompanyRole with SuperAdmin

### Phase 4: Employee Management
16. Create Employee repository
17. Create Employee controller
18. Create employee list view
19. Create employee create/edit views

### Phase 5: Invitation System (Current Gap)
20. Create Invitation controller (MISSING)
21. Create invitation accept view (MISSING)
22. Implement token validation
23. Implement existing user attach logic
24. Implement new user creation logic

### Phase 6: RBAC
25. Create role service
26. Create authorization middleware
27. Add role-based attributes
28. Create permission system

### Phase 7: Multi-Company Features
29. Implement company switcher
30. Implement company isolation
31. Create child company feature

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
- [ ] Create all entity models
- [ ] Setup DbContext with relationships
- [ ] Add DbSets for all entities
- [ ] Run initial migration
- [ ] Add seed data for default roles

### Authentication
- [ ] User Register endpoint
- [ ] User Login endpoint
- [ ] JWT token generation
- [ ] Cookie authentication setup
- [ ] Logout endpoint

### Company Setup (Current Gap)
- [ ] Create SetupController
- [ ] Create /setup/company route
- [ ] Create company setup view
- [ ] Auto-create Employee on company creation
- [ ] Auto-create EmployeeCompany record
- [ ] Auto-create UserCompanyRole with SuperAdmin
- [ ] Auto-set as default company

### Employee Management
- [ ] Employee List endpoint
- [ ] Employee Create endpoint
- [ ] Employee Update endpoint
- [ ] Employee Delete (soft) endpoint
- [ ] Employee List view
- [ ] Employee Create/Edit view

### Invitation System (Current Gap)
- [ ] Create InvitationController
- [ ] Create /invitations/accept GET route
- [ ] Create accept invitation view
- [ ] Validate token exists
- [ ] Validate token not expired
- [ ] Validate token not used
- [ ] Handle existing user case
- [ ] Handle new user case
- [ ] Create User on accept
- [ ] Link user to Employee
- [ ] Create UserCompanyRole
- [ ] Update Invitation status
- [ ] Auto-login after accept

### RBAC
- [ ] Define role enum/constants
- [ ] Create role service
- [ ] Create authorization middleware
- [ ] Add [Authorize] attributes
- [ ] Create RequireRoleAttribute
- [ ] Create PermissionAttribute
- [ ] Implement role-based view logic

### Multi-Company Support
- [ ] Company context middleware
- [ ] Get current company service
- [ ] Switch company endpoint
- [ ] Default company selection
- [ ] Company isolation for queries

### Child Company
- [ ] Create child company endpoint
- [ ] Parent-Child dropdown UI
- [ ] Auto-assign SuperAdmin to creator
- [ ] Hierarchy display

### Middleware & Security
- [ ] CompanyContextMiddleware
- [ ] Request validation middleware
- [ ] Token expiry validation
- [ ] Rate limiting for auth endpoints
- [ ] SQL injection prevention
- [ ] XSS prevention

### UI/Pages
- [ ] Login page
- [ ] Register page
- [ ] Company setup page (NEW)
- [ ] Dashboard
- [ ] Employee list
- [ ] Employee create/edit
- [ ] User list
- [ ] Invite user form
- [ ] Accept invitation page (NEW)
- [ ] Company switcher UI

### API Endpoints
- [ ] POST /api/auth/register
- [ ] POST /api/auth/login
- [ ] POST /api/setup/company
- [ ] GET /api/companies
- [ ] POST /api/companies/child
- [ ] GET /api/employees
- [ ] POST /api/employees
- [ ] PUT /api/employees/{id}
- [ ] DELETE /api/employees/{id}
- [ ] POST /api/invitations
- [ ] GET /api/invitations/accept/{token}
- [ ] POST /api/invitations/accept
- [ ] POST /api/companies/switch

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
**Problem:** /Invitations/Accept route doesn't exist

**Solution:** Create:
- [ ] InvitationController.cs
- [ ] Accept GET action
- [ ] Accept POST action
- [ ] Accept.cshtml view

### Gap 3: Company Context
**Problem:** Need consistent way to get current company

**Solution:** Ensure ICompanyContext is:
- [ ] Set from JWT token
- [ ] Updated on company switch
- [ ] Used in all repository queries

---

## 12. Summary

The V1 system will support:
- ✅ User registration and login
- ✅ Company setup with auto-owned employee
- ✅ Employee management (manual creation)
- ✅ Invitation system (send + accept)
- ✅ Multi-company user access
- ✅ Child company hierarchy
- ✅ Role-based access control

**Key missing pieces to implement:**
1. Company setup automation (likely already done)
2. Invitation acceptance controller + view
3. Company context middleware enhancement
4. RBAC implementation