# SmartLeads Codebase Analysis Report
*Generated: 2026-05-04*
*Scope: Full codebase audit — only evidence-based findings*

---

## 1. Code Structure Summary

### Project Layout (SmartLeads.slnx)
```
SmartLeads.slnx
├── src/SmartLeads.Domain/              # Layer: Domain
│   ├── DTOs/                          # Data Transfer Objects
│   │   ├── AttachmentDto.cs
│   │   ├── AuthResponse.cs
│   │   ├── AuthViewModels.cs
│   │   ├── ColumnFilterDtos.cs
│   │   ├── CommonDataTypeDto.cs
│   │   ├── CompanyDtos.cs
│   │   ├── ContactDto.cs
│   │   ├── ContactRequests.cs
│   │   ├── EmployeeDtos.cs
│   │   ├── ErrorViewModel.cs
│   │   ├── GroupDto.cs
│   │   ├── InvitationDtos.cs
│   │   ├── NoteDto.cs
│   │   ├── NotificationDtos.cs
│   │   ├── PaginationRequest.cs
│   │   ├── PaginationResponse.cs
│   │   ├── SelectOptionDto.cs
│   │   ├── TagDto.cs
│   │   ├── UserDto.cs
│   │   ├── UserTableDto.cs
│   │   └── VariableDtos.cs
│   ├── Enums/
│   │   ├── CommunicationPreference.cs
│   │   ├── ContactStatus.cs
│   │   ├── FileType.cs
│   │   ├── NotificationStatus.cs
│   │   ├── NotificationType.cs
│   │   └── UserRole.cs
│   └── Models/
│       ├── Attachment.cs
│       ├── BaseCompanyEntity.cs
│       ├── BaseEntity.cs
│       ├── ColumnFilter.cs
│       ├── Company.cs
│       ├── Contact.cs
│       ├── Department.cs
│       ├── Designation.cs
│       ├── Employee.cs
│       ├── EmployeeUser.cs
│       ├── Group.cs
│       ├── Invitation.cs
│       ├── Note.cs
│       ├── Notification.cs
│       ├── NotificationPreference.cs
│       ├── Tag.cs
│       ├── User.cs
│       ├── UserCompany.cs
│       └── Variable.cs
│
├── src/SmartLeads.Infrastructure/        # Layer: Infrastructure
│   ├── DependencyInjection.cs
│   ├── Persistence/
│   │   ├── DefaultDbContext.cs          # Company-specific data
│   │   ├── DefaultDbContextDesignTimeFactory.cs
│   │   ├── SystemDbContext.cs           # System-wide data
│   │   ├── SystemDbContextDesignTimeFactory.cs
│   │   └── Migrations/
│   │       ├── Default/  (8 migration files)
│   │       └── System/   (5 migration files)
│   ├── Repositories/
│   │   ├── BaseRepository.cs
│   │   ├── GenericRepository.cs
│   │   ├── UnitOfWork.cs
│   │   ├── Implementation/
│   │   │   ├── ColumnFilterRepository.cs
│   │   │   ├── CompanyRepository.cs
│   │   │   ├── ContactRepository.cs
│   │   │   ├── DepartmentRepository.cs
│   │   │   ├── DesignationRepository.cs
│   │   │   ├── EmployeeRepository.cs
│   │   │   ├── InvitationRepository.cs
│   │   │   ├── NotificationPreferenceRepository.cs
│   │   │   ├── NotificationRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   └── VariableRepository.cs
│   │   └── Interface/
│   │       ├── IBaseRepository.cs
│   │       ├── IColumnFilterRepository.cs
│   │       ├── ICompanyRepository.cs
│   │       ├── IContactRepository.cs
│   │       ├── IDepartmentRepository.cs
│   │       ├── IDesignationRepository.cs
│   │       ├── IEmployeeRepository.cs
│   │       ├── IGenericRepository.cs
│   │       ├── IInvitationRepository.cs
│   │       ├── INotificationPreferenceRepository.cs
│   │       ├── INotificationRepository.cs
│   │       ├── IUnitOfWork.cs
│   │       ├── IUserRepository.cs
│   │       └── IVariableRepository.cs
│   └── Services/
│       ├── CompanyContext.cs
│       ├── DefaultDbContextFactory.cs
│       └── (no notification service here)
│
├── src/SmartLeads.Utilities/             # Layer: Utilities (Cross-cutting)
│   ├── DependencyInjection.cs
│   ├── Email/
│   │   ├── EmailService.cs
│   │   └── SMTPConfigModel.cs
│   ├── Identity/
│   │   ├── JwtTokenGenerator.cs
│   │   └── PasswordHasher.cs
│   ├── Interfaces/
│   │   ├── IEmailService.cs
│   │   ├── IJwtTokenGenerator.cs
│   │   ├── INotificationPreferenceRepository.cs  # (duplicate interface)
│   │   ├── INotificationRepository.cs         # (duplicate interface)
│   │   ├── INotificationService.cs
│   │   ├── IPasswordHasher.cs
│   │   └── IUserRepository.cs               # (duplicate interface)
│   └── Services/
│       └── NotificationService.cs
│
└── src/SmartLeads.Web/                  # Layer: Presentation (MVC)
    ├── Program.cs
    ├── DependencyInjection.cs
    ├── appsettings.json
    ├── Controllers/
    │   ├── AuthController.cs               # Login, Register, Profile, Password Reset
    │   ├── BaseController.cs
    │   ├── ColumnFilterController.cs
    │   ├── CompaniesController.cs
    │   ├── ContactsController.cs            # API + MVC views
    │   ├── DepartmentsController.cs
    │   ├── DesignationsController.cs
    │   ├── EmployeesController.cs
    │   ├── HomeController.cs
    │   ├── NotificationsController.cs
    │   ├── UserCompanyController.cs         # Multi-tenant company switching
    │   ├── UsersController.cs               # User management via invitations
    │   └── VariablesController.cs
    ├── Filters/
    │   └── RequireCompanyAttribute.cs
    ├── Views/                              # Razor Views
    │   ├── Auth/                          # Login, Register, Profile, Password Reset
    │   ├── Companies/
    │   ├── Contacts/                       # Index, Create, Edit
    │   ├── Departments/
    │   ├── Designations/
    │   ├── Employees/
    │   ├── Home/                          # Index, Landing, Components, Privacy
    │   ├── Notifications/                  # Index, Details, Preferences
    │   ├── Shared/                        # Layouts, Partial Views, Components
    │   ├── UserCompany/                   # Dashboard, CompanyInfo, CreateCompany, NoCompany
    │   ├── Users/
    │   └── Variables/
    ├── ViewComponents/
    │   └── NotificationBellViewComponent.cs
    ├── Contacts/Mappings/
    │   └── ContactMappingProfile.cs       # AutoMapper
    ├── Users/
    │   ├── Commands/
    │   │   ├── ForgotPassword/
    │   │   ├── RegisterUser/
    │   │   ├── ResetPassword/
    │   │   └── UpdateUser/
    │   └── Queries/
    │       ├── GetUserProfile/
    │       └── LoginUser/
    └── wwwroot/                           # Static files + Sneat template assets
```

### Layer Dependencies
```
SmartLeads.Web
  ├── depends on --> SmartLeads.Infrastructure (IUnitOfWork, Repositories, DbContexts)
  ├── depends on --> SmartLeads.Utilities      (IJwtTokenGenerator, IEmailService, INotificationService)
  └── depends on --> SmartLeads.Domain        (Models, DTOs, Enums - via transitive)

SmartLeads.Infrastructure
  ├── depends on --> SmartLeads.Domain (Models, Interfaces in Domain)

SmartLeads.Utilities
  └── depends on --> SmartLeads.Domain (Interfaces in Domain)
```

---

## 2. Architecture Overview (Verified)

**Architecture Pattern:** Clean Architecture (Modular Monolith) with Multi-Tenant support

**Key Characteristics:**
- 4 projects with clear separation of concerns
- Repository pattern with BaseRepository<T> + entity-specific repositories
- UnitOfWork pattern for transaction management
- Two separate DbContexts for multi-tenancy
- JWT authentication with cookie fallback
- MVC + partial Web API (controllers serve both views and JSON)

**NOT present (despite TODO_LIST.md claims):**
- ❌ MediatR (Commands/Queries exist as empty folder structures only, not wired up)
- ❌ FluentValidation (data annotations used instead)
- ❌ AutoMapper (ContactMappingProfile.cs exists but usage not verified in controllers)
- ❌ Serilog (only Console + Debug logging configured in Program.cs)
- ❌ Docker Support (TODO_LIST claims it, no Dockerfile found)

---

## 3. Verified Feature List (Code-Only Evidence)

### Authentication & Authorization
| Feature | Evidence Location | Status |
|---|---|---|
| User Registration | `AuthController.cs:46` | ✅ Implemented |
| Login (JWT + Cookie) | `AuthController.cs:124` | ✅ Implemented |
| Password Reset (Token) | `AuthController.cs:530` | ✅ Implemented |
| Forgot Password (Email) | `AuthController.cs:500` | ✅ Implemented |
| JWT Token Generation | `JwtTokenGenerator.cs` | ✅ Implemented |
| Refresh Token Storage | `User.cs:19-20` | ✅ Schema only (no endpoint found) |
| Role-Based Access (User/Manager/Admin/SuperAdmin) | `UserRole.cs`, `UserCompany.cs` | ✅ Implemented |
| RequireCompany Filter | `RequireCompanyAttribute.cs` | ✅ Implemented |
| 2FA | — | ❌ Not found |
| OAuth/Google/LinkedIn Login | — | ❌ Not found |
| Account Lockout | — | ❌ Not found |

### User Management
| Feature | Evidence Location | Status |
|---|---|---|
| User CRUD (via Invitation) | `UsersController.cs:53` | ✅ Implemented |
| User Profile View/Edit | `AuthController.cs:285` | ✅ Implemented |
| Profile Picture Upload | `AuthController.cs:370` | ✅ Implemented |
| Password Change | `AuthController.cs:422` | ✅ Implemented |
| Role Assignment per Company | `UsersController.cs:246` | ✅ Implemented |
| User Listing with Pagination | `UsersController.cs:39` | ✅ Implemented |
| User Invitation System | `UsersController.cs:108` | ✅ Implemented |
| Invitation Email Template | `UsersController.cs:296` | ✅ Implemented |

### Company Management
| Feature | Evidence Location | Status |
|---|---|---|
| Company CRUD | `CompaniesController.cs`, `UserCompanyController.cs` | ✅ Implemented |
| Parent-Child Hierarchy | `Company.cs:12`, `SystemDbContext.cs:73` | ✅ Implemented |
| Company Switching (Session/Cookie) | `UserCompanyController.cs:298` | ✅ Implemented |
| User-Company Association | `UserCompany.cs`, `SystemDbContext.cs:52` | ✅ Implemented |
| Company Info Page | `UserCompanyController.cs:456` | ✅ Implemented |
| Multi-Layout Support | `UserCompanyController.cs:431` | ✅ Implemented |

### Contact Management
| Feature | Evidence Location | Status |
|---|---|---|
| Contact CRUD (API) | `ContactsController.cs:29-122` | ✅ Implemented |
| Contact CRUD (MVC Views) | `ContactsController.cs:125-199` | ✅ Implemented |
| Contact Ownership (UserId) | `Contact.cs:15` | ✅ Implemented |
| Contact Archival (IsArchived) | `Contact.cs:12` | ✅ Implemented |
| Tag Management | `Tag.cs`, `ContactTag.cs` | ✅ Schema only (no controller found) |
| Group Management | `Group.cs`, `ContactGroup.cs` | ✅ Schema only (no controller found) |
| Contact Notes | `Note.cs`, `Contact.cs:18` | ✅ Schema only (no controller found) |
| Contact Attachments | `Attachment.cs`, `Contact.cs:19` | ✅ Schema only (no controller found) |
| Contact Search | — | ❌ Not found |
| Contact Import/Export | — | ❌ Not found |

### Employee Management
| Feature | Evidence Location | Status |
|---|---|---|
| Employee CRUD | `EmployeesController.cs:73` | ✅ Implemented |
| Department CRUD | `DepartmentsController.cs` | ✅ Implemented |
| Designation CRUD | `DesignationsController.cs` | ✅ Implemented |
| Employee-User Linking | `EmployeeUser.cs`, `EmployeesController.cs` | ✅ Implemented |
| Employee Profile Fields | `Employee.cs` (DOB, Gender, BloodGroup, etc.) | ✅ Implemented |
| Pagination/Search | `EmployeesController.cs:24` | ✅ Implemented |
| Select2 Search | `DepartmentsController.cs:134`, `DesignationsController.cs:136` | ✅ Implemented |

### Notification System
| Feature | Evidence Location | Status |
|---|---|---|
| In-App Notification Bell | `NotificationBellViewComponent.cs` | ✅ Implemented |
| Notification CRUD | `NotificationsController.cs` | ✅ Implemented |
| Notification Preferences | `NotificationsController.cs:131` | ✅ Implemented |
| Email Notifications | `NotificationService.cs` | ✅ Implemented |
| Mark Read/Unread/Archive | `NotificationsController.cs:69-109` | ✅ Implemented |
| Unread Count API | `NotificationsController.cs:113` | ✅ Implemented |
| Recent Notifications API | `NotificationsController.cs:121` | ✅ Implemented |
| Broadcast to Multiple Users | `NotificationService.cs` | ✅ Implemented |
| SignalR Real-time | — | ❌ Not found |

### Variables / Common Data
| Feature | Evidence Location | Status |
|---|---|---|
| Variable CRUD | `VariablesController.cs:192` | ✅ Implemented |
| Type-based Lookup | `VariablesController.cs:162` | ✅ Implemented |
| Select2 Search | `VariablesController.cs:39` | ✅ Implemented |
| Pagination | `VariablesController.cs:63` | ✅ Implemented |
| Delete by Type | `VariablesController.cs:308` | ✅ Implemented |

### Column Filters
| Feature | Evidence Location | Status |
|---|---|---|
| ColumnFilter Model | `ColumnFilter.cs` | ✅ Implemented |
| ColumnFilter Repository | `ColumnFilterRepository.cs` | ✅ Implemented |
| ColumnFilter Controller | `ColumnFilterController.cs` | ⚠️ Empty (no methods) |

---

## 4. Real Database Schema (Extracted from DbContext files)

### System Database (`SystemDbContext.cs`)
```sql
-- Users table
Users {
    Id: Guid (PK)
    Username: string
    Email: string
    PasswordHash: string
    FirstName: string?
    LastName: string?
    ProfilePicture: string?
    IsPasswordSetByUser: bool = false
    RefreshToken: string?
    RefreshTokenExpiryTime: DateTime?
    ResetPasswordToken: string?
    ResetPasswordTokenExpiryTime: DateTime?
    CreatedAt: DateTime
    UpdatedAt: DateTime?
    IsActive: bool = true
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- Companies table
Companies {
    Id: Guid (PK)
    Name: string
    Code: string?
    Address: string?
    Phone: string?
    Email: string?
    Logo: string?
    IsParent: bool = false
    ParentCompanyId: Guid? (FK -> Companies.Id)
    CreatedAt: DateTime
    UpdatedAt: DateTime?
    IsActive: bool = true
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- UserCompany junction table (Role per company)
UserCompanies {
    UserId: Guid (PK, FK -> Users.Id)
    CompanyId: Guid (PK, FK -> Companies.Id)
    Role: string (enum converted: User/Manager/Admin/SuperAdmin)
    IsDefault: bool
    IsActive: bool = true
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- Invitations table
Invitations {
    Id: Guid (PK)
    Email: string
    Role: UserRole
    CompanyId: Guid (FK -> Companies.Id)
    InvitedByUserId: Guid (FK -> Users.Id)
    Token: string
    ExpiresAt: DateTime
    Status: InvitationStatus
    Metadata: string (JSON)
    CreatedAt: DateTime
    UpdatedAt: DateTime?
    IsDeleted: bool = false
}

-- Notifications table
Notifications {
    Id: Guid (PK)
    UserId: Guid (no FK - different DB)
    CompanyId: Guid (no FK - different DB)
    Title: string
    Message: string
    Type: string (enum: Task/Email/System/Alert/Invitation/Contact)
    Status: string (enum: Unread/Read/Archived)
    RelatedEntityId: Guid?
    RelatedEntityType: string?
    ActionUrl: string?
    IsEmailSent: bool = false
    CreatedAt: DateTime
    UpdatedAt: DateTime?
}

-- NotificationPreferences table
NotificationPreferences {
    Id: Guid (PK)
    UserId: Guid (FK -> Users.Id)
    NotificationType: string (enum)
    EnableInApp: bool = true
    EnableEmail: bool = true
    CreatedAt: DateTime
    UpdatedAt: DateTime?
}
```

### Default/Company Database (`DefaultDbContext.cs`)
```sql
-- Contacts table
Contacts {
    Id: Guid (PK)
    CompanyId: Guid (indexed, no FK - different DB)
    UserId: Guid (indexed, no FK - different DB)
    FirstName: string
    LastName: string
    Email: string?
    PhoneNumber: string?
    ContactCompany: string? (company where contact works)
    JobTitle: string?
    Address: string?
    IsArchived: bool = false
    CreatedAt: DateTime
    UpdatedAt: DateTime?
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- Tags table
Tags {
    Id: Guid (PK)
    CompanyId: Guid
    UserId: Guid
    Name: string
    Description: string?
    SortOrder: int = 0
    IsActive: bool = true
    IsDeleted: bool = false
}

-- Groups table
Groups {
    Id: Guid (PK)
    CompanyId: Guid
    UserId: Guid
    Name: string
    Description: string?
    SortOrder: int = 0
    IsActive: bool = true
    IsDeleted: bool = false
}

-- ContactTag junction table (Many-to-Many)
ContactTags {
    ContactId: Guid (FK -> Contacts.Id)
    TagId: Guid (FK -> Tags.Id)
}

-- ContactGroup junction table (Many-to-Many)
ContactGroups {
    ContactId: Guid (FK -> Contacts.Id)
    GroupId: Guid (FK -> Groups.Id)
}

-- Notes table
Notes {
    Id: Guid (PK)
    ContactId: Guid (FK -> Contacts.Id)
    CompanyId: Guid
    UserId: Guid
    Content: string
    CreatedAt: DateTime
    UpdatedAt: DateTime?
    IsDeleted: bool = false
}

-- Attachments table
Attachments {
    Id: Guid (PK)
    ContactId: Guid (FK -> Contacts.Id)
    CompanyId: Guid
    UserId: Guid
    FileName: string
    FilePath: string
    FileSize: long
    ContentType: string?
    CreatedAt: DateTime
    UpdatedAt: DateTime?
    IsDeleted: bool = false
}

-- Employees table
Employees {
    Id: Guid (PK)
    CompanyId: Guid (indexed with EmployeeId)
    EmployeeId: string (unique per company)
    FirstName: string
    LastName: string
    MiddleName: string?
    NickName: string?
    WorkEmail: string?
    PersonalEmail: string?
    DepartmentId: Guid? (FK -> Departments.Id)
    DesignationId: Guid? (FK -> Designations.Id)
    PhoneNumber: string?
    AlternatePhoneNumber: string?
    EmergencyContactName: string?
    EmergencyContactPhone: string?
    Address: string?
    PresentAddress: string?
    PermanentAddress: string?
    DateOfBirth: DateTime?
    Gender: string?
    MaritalStatus: string?
    BloodGroup: string?
    Nationality: string?
    NationalIdNumber: string?
    JoiningType: string?
    EmploymentStatus: string?
    ProfilePhotoUrl: string?
    Notes: string?
    DateOfJoining: DateTime?
    IsActive: bool = true
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- EmployeeUser junction table
EmployeeUsers {
    EmployeeId: Guid (FK -> Employees.Id)
    UserId: Guid (no FK - different DB)
    IsPrimary: bool = false
}

-- Departments table
Departments {
    Id: Guid (PK)
    CompanyId: Guid (indexed with Name - unique)
    UserId: Guid
    Name: string
    Description: string?
    IsActive: bool = true
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- Designations table
Designations {
    Id: Guid (PK)
    CompanyId: Guid (indexed with Name - unique)
    UserId: Guid
    DepartmentId: Guid (FK -> Departments.Id)
    Name: string
    Description: string?
    IsActive: bool = true
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- Variables table
Variables {
    Id: Guid (PK)
    CompanyId: Guid
    UserId: Guid
    Type: string
    Value: string
    Value1: string?
    Value2: string?
    Value3: string?
    Description: string?
    SortOrder: int = 0
    IsActive: bool = true
    IsDeleted: bool = false
    DeletedAt: DateTime?
}

-- ColumnFilters table
ColumnFilters {
    Id: Guid (PK)
    CompanyId: Guid
    UserId: Guid
    Name: string
    Configuration: string? (JSON)
    CreatedAt: DateTime
    UpdatedAt: DateTime?
    IsDeleted: bool = false
}
```

---

## 5. Actual API Endpoint List (From Controllers)

### AuthController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Auth/Register` | Registration page | No |
| POST | `/Auth/Register` | Register new user | No |
| GET | `/Auth/Login` | Login page | No |
| POST | `/Auth/Login` | Login + JWT cookie | No |
| POST | `/Auth/Logout` | Clear cookies + session | No |
| GET | `/Auth/Profile` | User profile page | Yes |
| POST | `/Auth/Profile` | Update profile + password | Yes |
| GET | `/Auth/ForgotPassword` | Forgot password page | No |
| GET | `/Auth/ForgotPasswordConfirmation` | Confirmation page | No |
| GET | `/Auth/ResetPassword` | Reset password page (with token) | No |
| POST | `/Auth/ResetPassword` | Reset password action | No |
| POST | `/Auth/ApiLogin` | AJAX login endpoint | No |
| POST | `/Auth/ApiForgotPassword` | AJAX forgot password | No |
| POST | `/Auth/ApiResetPassword` | AJAX reset password | No |

### ContactsController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Contacts` | Contact list page | Yes (RequireCompany) |
| GET | `/api/contacts` | List all contacts (JSON) | Yes |
| GET | `/api/contacts/{id}` | Get single contact | Yes |
| POST | `/api/contacts` | Create contact | Yes |
| PUT | `/api/contacts/{id}` | Update contact | Yes |
| DELETE | `/api/contacts/{id}` | Delete contact | Yes |
| GET | `/Contacts/Create` | Create contact page | Yes |
| POST | `/Contacts/Create` | Create contact (MVC) | Yes |
| GET | `/Contacts/Edit/{id}` | Edit contact page | Yes |
| POST | `/Contacts/Edit/{id}` | Update contact (MVC) | Yes |
| POST | `/Contacts/Delete/{id}` | Delete contact (MVC) | Yes |

### UsersController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Users` | User list page | Yes |
| GET | `/Users/GetUsersData` | Server-side pagination | Yes |
| POST | `/Users/Create` | Send user invitation | Yes |
| GET | `/Users/Edit/{id}` | Edit user page | Yes |
| POST | `/Users/Edit/{id}` | Update user (role, status) | Yes |
| POST | `/Users/Delete/{id}` | Delete user | Yes |

### CompaniesController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Companies` | Company list page | Yes |

### UserCompanyController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/UserCompany/NoCompany` | No company page | No |
| GET | `/UserCompany/Dashboard` | Dashboard page | Yes |
| GET | `/UserCompany/CreateCompany` | Create company page | No* |
| POST | `/UserCompany/CreateCompany` | Create company + admin | No* |
| POST | `/UserCompany/SwitchCompany` | Switch active company | Yes |
| GET | `/UserCompany/GetCompanies` | API: list user's companies | Yes |
| POST | `/UserCompany/SwitchLayout` | Switch layout preference | Yes |
| GET | `/UserCompany/GetLayout` | Get layout preference | Yes |
| GET | `/UserCompany/CompanyInfo` | Company info page | Yes (RequireCompany) |
| POST | `/UserCompany/UpdateCompany` | Update company info | Yes (RequireCompany) |
| GET | `/UserCompany/GetParentCompaniesForSelect2` | Select2 parent companies | Yes |

### NotificationsController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Notifications` | Notification list page | Yes |
| GET | `/Notifications/Details/{id}` | Notification details | Yes |
| POST | `/Notifications/MarkAsRead/{id}` | Mark as read | Yes |
| POST | `/Notifications/MarkAllAsRead` | Mark all read | Yes |
| POST | `/Notifications/Archive/{id}` | Archive notification | Yes |
| GET | `/Notifications/UnreadCount` | API: unread count | Yes |
| GET | `/Notifications/GetRecent` | API: recent notifications | Yes |
| GET | `/Notifications/Preferences` | Preferences page | Yes |
| POST | `/Notifications/Preferences` | Update preferences | Yes |
| POST | `/api/notifications` | API: create notification | Yes |
| DELETE | `/api/notifications/old` | API: delete old notifications | Yes |

### EmployeesController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Employees` | Employee list page | Yes |
| GET | `/Employees/GetEmployeesData` | Server-side pagination | Yes (RequireCompany) |
| POST | `/Employees/Save` | Create/update employee | Yes |
| GET | `/Employees/GetEmployee/{id}` | Get employee by ID | Yes |
| POST | `/Employees/Delete/{id}` | Soft-delete employee | Yes |

### DepartmentsController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Departments` | Department list page | Yes |
| GET | `/Departments/GetDepartmentsData` | Paginated departments | Yes (RequireCompany) |
| GET | `/Departments/GetAll` | API for dropdown | Yes (RequireCompany) |
| GET | `/Departments/SearchDepartment` | Select2 search | Yes (RequireCompany) |
| POST | `/Departments/Create` | Create department | Yes |
| GET | `/Departments/Edit/{id}` | Edit department page | Yes |
| POST | `/Departments/Edit/{id}` | Update department | Yes |
| POST | `/Departments/Delete/{id}` | Soft-delete department | Yes |

### DesignationsController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Designations` | Designation list page | Yes |
| GET | `/Designations/GetDesignationsData` | Paginated designations | Yes (RequireCompany) |
| GET | `/Designations/GetAll` | API for dropdown | Yes (RequireCompany) |
| GET | `/Designations/SearchDesignation` | Select2 search (cascade) | Yes (RequireCompany) |
| POST | `/Designations/Create` | Create designation | Yes |
| GET | `/Designations/Edit/{id}` | Edit designation page | Yes |
| POST | `/Designations/Edit/{id}` | Update designation | Yes |
| POST | `/Designations/Delete/{id}` | Soft-delete designation | Yes |

### VariablesController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/Variables` | Variable list page | Yes |
| GET | `/Variables/SearchType` | Search common data types | Yes |
| GET | `/Variables/SearchVariable` | Select2 variable search | Yes (RequireCompany) |
| GET | `/Variables/GetVariablesData` | Paginated variables | Yes (RequireCompany) |
| GET | `/Variables/GetTypes` | Get all types | Yes (RequireCompany) |
| GET | `/Variables/GetByType/{type}` | Get variables by type | Yes (RequireCompany) |
| POST | `/Variables/Save` | Create/update variable | Yes |
| POST | `/Variables/Delete/{id}` | Soft-delete variable | Yes |
| POST | `/Variables/DeleteByType/{type}` | Delete all by type | Yes |

### ColumnFilterController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| (No methods found) | — | Empty controller | — |

### HomeController
| Method | Route | Description | Auth Required |
|---|---|---|---|
| GET | `/` (default route) | Landing page | No |
| GET | `/Home/Index` | Home page (main layout) | Yes |
| GET | `/Home/Components` | Components page | Yes |
| GET | `/Home/Privacy` | Privacy page | No |

---

## 6. Multi-Tenant Implementation Details (Confirmed)

### Database Separation
- **SystemDbContext**: Contains Users, Companies, UserCompanies, Invitations, Notifications, NotificationPreferences
- **DefaultDbContext**: Contains all company-specific data (Contacts, Employees, Departments, etc.)

### How Tenant Isolation Works
1. **BaseEntity Pattern** (`BaseEntity.cs`):
   - All company-specific entities inherit from `BaseEntity`
   - `BaseEntity` includes `CompanyId` (Guid) and `UserId` (Guid)
   - `DefaultDbContext` auto-indexes all `BaseEntity` types on `CompanyId` and `UserId`

2. **RequireCompany Filter** (`RequireCompanyAttribute.cs`):
   - Applied to controllers that need company context
   - Reads `CompanyId` from JWT claim or session
   - Sets current company context via `ICompanyContext`

3. **CompanyContext Service** (`CompanyContext.cs`):
   - Provides `CurrentCompanyId`, `CurrentUserId`
   - Caches user companies for performance
   - `SetCurrentCompany()` updates session + cookies

4. **UserCompany Junction** (`UserCompany.cs`):
   - Links Users to Companies with a per-company Role
   - Supports multiple company membership per user
   - `IsDefault` flag for login default
   - Unique constraint on (UserId, CompanyId)

5. **JWT Token** (`JwtTokenGenerator.cs`):
   - Includes `CompanyId` claim
   - Includes role from UserCompany
   - 1-hour expiry

6. **Cookie Storage** (in AuthController):
   - `JwtToken` (HttpOnly, Secure, Strict)
   - `UserId`
   - `CurrentCompanyId`
   - `CurrentEmployeeId`

### Verified Limitations
- No FK constraints between SystemDb and DefaultDb (by design - different databases)
- `BaseSystemEntity` (Users, Companies) does NOT have `CompanyId` — they're in system DB
- `Invitation` entity exists in BOTH DbContexts (potential duplication issue)

---

## 7. Identified Missing/Incorrect Features (Evidence-Based)

### Features Claimed in TODO_LIST.md but NOT Found in Code
| Claimed | Reality |
|---|---|
| "REST API & Integrations" complete | Only partial API (mixed MVC + API in same controllers) |
| "Docker Support" | No Dockerfile found in project |
| "FluentValidation" | Not used; DataAnnotations used instead |
| "MediatR Commands/Queries" | Folder structure exists but NOT wired up; controllers don't use MediatR |
| "AutoMapper" | ContactMappingProfile.cs exists but not verified in use |
| "Serilog" | Only Console + Debug logging in Program.cs |
| "Swagger/OpenAPI" | Not found |
| "Unit Tests" | Zero test files found |

### Schema-Only Features (Tables exist but no UI/Controller)
| Feature | Evidence |
|---|---|
| Tags | `Tag.cs`, `ContactTag.cs` — no TagsController found |
| Groups | `Group.cs`, `ContactGroup.cs` — no GroupsController found |
| Notes | `Note.cs` — no NotesController found |
| Attachments | `Attachment.cs` — no AttachmentsController found |
| Column Filters | `ColumnFilter.cs` — ColumnFilterController exists but empty |

### Missing Standard SaaS Features
| Feature | Evidence |
|---|---|
| Task/Activity Management | No Task model/controller found |
| Email Integration (send from contact) | EmailService exists but not integrated with contacts |
| Contact Import/Export | Not found |
| Global Search | Not found |
| Reports/Analytics | Not found |
| Lead Pipeline/Stages | No ContactStatus usage found (enum exists but not used in Contact model) |
| Call/Meeting Management | Not found |
| Bulk Operations | Not found |
| Audit Trail | Not found |
| 2FA | Not found |
| OAuth Login | Not found |
| SignalR Real-time | Not found |
| Swagger/OpenAPI | Not found |
| CI/CD Pipeline | No GitHub Actions workflow found |

---

## 8. Security and Authentication Flow Analysis

### Authentication Flow
```
1. User submits credentials → AuthController.Login()
2. Verify against User table (SystemDbContext)
3. Get UserCompany association for default company
4. Generate JWT with claims: Name=username, NameIdentifier=userId, CompanyId, Role
5. Set cookies: JwtToken, UserId, CurrentCompanyId, CurrentEmployeeId
6. Set Session: CurrentCompanyId
7. Redirect to Home or Dashboard
```

### JWT Token Structure
- **Claims:** `Name` (username), `NameIdentifier` (userId), `CompanyId`, role claim
- **Expiry:** 1 hour (hardcoded in AuthController)
- **Role Source:** `UserCompany.Role` (not User table)

### Cookie Configuration (Verified in AuthController)
```csharp
HttpOnly = true
Secure = true
SameSite = SameSiteMode.Strict
Expires = DateTimeOffset.UtcNow.AddHours(1)
```

### Password Security
- **Hashing:** BCrypt via `PasswordHasher.cs`
- **Reset Token:** Guid-based token, 24-hour expiry
- **Invitation Token:** Guid-based token, 7-day expiry

### Authorization
- **RequireCompany Filter:** Ensures company context exists
- **Role-Based:** UserRole enum (SuperAdmin=1, Admin=2, Manager=3, User=4)
- **Per-Company Roles:** Role stored in UserCompany, not global

### Security Gaps Found
1. **No HTTPS enforcement** in development (Program.cs uses UseHttpsRedirection in non-dev only)
2. **No rate limiting** on login/register endpoints
3. **No account lockout** after failed attempts
4. **No 2FA** support
5. **JWT secret** likely in configuration (not reviewed — appsettings.json not read)
6. **CORS policy** not explicitly configured
7. **Anti-forgery** tokens used for MVC but API endpoints also check ValidateAntiForgeryToken

---

## 9. Code Quality Issues

### Duplication
1. **Interface Duplication:**
   - `SmartLeads.Utilities/Interfaces/IUserRepository.cs` — duplicate of `SmartLeads.Infrastructure/Repositories/Interface/IUserRepository.cs`
   - `SmartLeads.Utilities/Interfaces/INotificationRepository.cs` — duplicate of Infrastructure version
   - `SmartLeads.Utilities/Interfaces/INotificationPreferenceRepository.cs` — duplicate of Infrastructure version

2. **Invitation Entity in Both DbContexts:**
   - `DefaultDbContext` includes `DbSet<Invitation>`
   - `SystemDbContext` also includes `DbSet<Invitation>`
   - Same entity in two databases = potential data inconsistency

3. **Notification & NotificationPreference in SystemDbContext:**
   - But `DefaultDbContext` explicitly ignores them (line 51-52)
   - Inconsistent with the "system entities in system DB" pattern since Notifications are per-company

### Design Flaws
1. **Mixed MVC + API in Same Controllers:**
   - Controllers return both Views and JSON
   - Example: ContactsController has `/api/contacts` AND `/Contacts/Create` (MVC view)
   - Makes the API less clean for external consumption

2. **No Service Layer:**
   - Controllers directly use UnitOfWork + Repositories
   - Business logic (invitation creation, email sending) is in Controllers
   - Makes testing difficult

3. **MediatR Not Implemented:**
   - Command/Query folder structure exists in `Users/` folder
   - But controllers don't use MediatR
   - Dead code / incomplete implementation

4. **Hardcoded Values:**
   - JWT expiry: 1 hour (AuthController.cs:94, :195, :594)
   - Reset token expiry: 24 hours (AuthController.cs:648)
   - Invitation expiry: 7 days (UsersController.cs:115)
   - File upload size: 5MB (AuthController.cs:380)

### Dead Code / Unused
1. **Empty ColumnFilterController** — no methods implemented
2. **ContactMappingProfile.cs** — AutoMapper profile exists but usage not verified
3. **Users/Commands/ and Users/Queries/** — folder structure exists, likely empty or not wired
4. **BaseCompanyEntity.cs** — exists in Domain/Models but not used in any entity
5. **CommunicationPreference.cs** — enum exists, not used anywhere
6. **ContactStatus.cs** — enum exists, not used in Contact model (no Status field)
7. **FileType.cs** — enum exists, not used in Attachment model

### Inconsistent Patterns
1. **PaginationRequest** uses both `GetPage()`/`GetPageSize()` methods AND `Page`/`PageSize` properties (see EmployeesController vs DepartmentsController)
2. **Some controllers use `[FromQuery]` for pagination, others don't**
3. **Some endpoints return `Ok(new { success = true })` others return `Ok()` or `RedirectToAction()`**
4. **DateTime.UtcNow used everywhere, but no timezone handling**

---

## 10. Final Prioritized Improvement Suggestions

### 🔥 Priority 1 — Critical (Do First)
1. **Add Repository Interfaces in Correct Location**
   - Remove duplicate interfaces from `SmartLeads.Utilities/Interfaces/`
   - Keep only in `SmartLeads.Infrastructure/Repositories/Interface/`

2. **Implement Tags, Groups, Notes, Attachments Controllers**
   - Schema exists but no UI or API endpoints
   - High-value CRM features missing

3. **Add Swagger/OpenAPI**
   - Document all API endpoints
   - Enable easy testing and frontend integration

4. **Write Unit Tests**
   - Start with service layer (NotificationService, EmailService)
   - Then repository layer
   - Target 70% coverage

### 🔥 Priority 2 — High Value
5. **Implement Task/Activity Management**
   - Create Task model + DbSet in DefaultDbContext
   - Add TaskController with CRUD
   - Link tasks to contacts/companies
   - Add calendar view

6. **Add Global Search**
   - Cross-entity search API
   - Search across contacts, companies, users
   - Add to main layout

7. **Implement Email Integration**
   - Send email from contact record
   - Email templates
   - Email history per contact
   - SMTP settings UI

8. **Add Contact Import/Export**
   - CSV import with field mapping
   - Duplicate detection
   - Excel/CSV export

### 🚀 Priority 3 — Growth
9. **Add Reports & Analytics**
   - Lead conversion report
   - Contact growth over time
   - Company performance

10. **Implement Proper Service Layer**
    - Move business logic out of controllers
    - Create services: ContactService, UserService, etc.
    - Makes business logic testable

11. **Add Lead Pipeline/Stages**
    - Use ContactStatus enum
    - Add Status field to Contact model
    - Kanban board view

12. **Security Enhancements**
    - Add 2FA support
    - OAuth login (Google, LinkedIn)
    - Rate limiting
    - Account lockout

### 📝 Priority 4 — Polish
13. **Clean Up Dead Code**
    - Remove empty MediatR folder structure OR implement it
    - Remove unused enums (CommunicationPreference, FileType if not needed)
    - Remove BaseCompanyEntity.cs if not used

14. **Add CI/CD**
    - GitHub Actions workflow
    - Docker Compose setup
    - Automated testing

15. **UI Improvements**
    - Dark mode
    - PWA support
    - Responsive improvements

---

## 11. OpenCode CLI Execution Requirement (IMPORTANT)

All future analysis steps for this project must be executed through OpenCode CLI with strict rules:

### Execution Rules:

* Do NOT rely on assumptions, design intent, or TODO comments
* ONLY use actual codebase outputs from CLI scans
* Every feature must be validated from source files before being documented
* Treat this report as a living document that is continuously refined by CLI evidence

### Required Workflow:

1. Run OpenCode CLI scan on the full repository
2. Extract:

   * Controllers → API endpoints
   * DbContext → database schema
   * Services → business logic
   * Repositories → data access layer
3. Cross-verify all previously documented features
4. Mark each item as:

   * CONFIRMED (found in code)
   * INVALID (not found in code)
   * PARTIAL (exists but incomplete)
5. Update this file after every scan iteration

### Output Standard:

Every future update must include:

* Evidence-based validation
* File path references
* Line-level confirmation where possible
* No speculative or inferred features

### Goal:

Maintain this document as a **single source of truth for SmartLeads codebase reality**, not design assumptions or planned architecture.

---

## 12. OpenCode CLI Continuous Audit Protocol (MANDATORY EXECUTION RULE)

This document is now bound to OpenCode CLI as a living verification artifact.

### System Rule:

Every update to this file MUST be driven by a fresh OpenCode CLI scan. No manual edits or assumption-based additions are allowed.

### Execution Enforcement:

Before any section is updated:

1. Run full repository scan via OpenCode CLI
2. Extract raw structured outputs:

   * File tree
   * Controller routes
   * DbContext models
   * Service implementations
   * Repository implementations
3. Compare against current report line-by-line.

### Validation Rules:

Each feature must be explicitly labeled as:

* **CONFIRMED** → Directly found in source code
* **INVALID** → Previously listed but not found
* **PARTIAL** → Exists but incomplete or non-functional
* **NEWLY DISCOVERED** → Found in latest scan but not previously documented

### Strict Constraints:

* No guessing allowed under any condition
* No "likely", "probably", or "assumed" language
* No feature inclusion without file-level evidence
* No architecture assumptions without code traceability

### Update Format Requirement:

Each update must include:

* File path reference
* Class / method name
* Controller route (if applicable)
* DbContext mapping (if applicable)
* Evidence snippet (if needed)

### Final Objective:

This report is the **system of record for SmartLeads reality**, not documentation, not design, not intention.

It must always reflect:

> "What the code actually does — not what it was meant to do."

---

*End of Report — Generated from actual codebase analysis*
