# SmartLeads — TODO List

## 📊 Project Overview
**SmartLeads** is an ASP.NET Core MVC Multi-Tenant CRM/Lead Management System built with .NET 10.0, PostgreSQL, and Entity Framework Core.

**Tech Stack:** ASP.NET Core MVC, EF Core 10.0.4, PostgreSQL, JWT Auth, BCrypt, MailKit, Bootstrap 5, Sneat Template, Clean Architecture, Repository Pattern, FluentValidation, AutoMapper, Serilog.

---

## ✅ Completed Features
- [x] User Registration & Login with JWT + Refresh Tokens
- [x] Role-Based Access Control (per-company: User, Manager, Admin, SuperAdmin)
- [x] Password Reset with Token
- [x] Email-Based Invitation System
- [x] User Management (CRUD with Employee Info)
- [x] Company Management (CRUD with Parent-Child Hierarchy)
- [x] Contact Management (CRUD with Archival, Ownership, Tagging, Grouping)
- [x] Tag & Group Management
- [x] Contact Notes
- [x] Contact Attachments (File Upload)
- [x] Column Filters
- [x] Multi-Tenant Database (System DB + Company DB)
- [x] Landing Page + Auth Layout + Main Protected Layout
- [x] Soft Delete Support
- [x] Pagination Support
- [x] Docker Support

---

## 🔥 Phase 1: Finish Core MVP (HIGH Priority)

### Search & Filters
- [ ] Implement global search across contacts/companies/users
- [ ] Add column-based filtering on list views
- [ ] Add saved/custom filter presets
- [ ] Implement fuzzy search and full-text search

### Dashboard
- [ ] Add statistics cards (total contacts, leads, tasks, etc.)
- [ ] Show recent contacts and activities
- [ ] Add lead source breakdown chart
- [ ] Add quick action buttons

---

## 🔥 Phase 2: Essential Features (HIGH Priority)

### Task & Activity Management
- [ ] Create Task model and database migration
- [ ] Task CRUD operations (Controller + Views)
- [ ] Task priorities (Low, Medium, High, Urgent)
- [ ] Task due dates and overdue alerts
- [ ] Task assignment to users
- [ ] Task status tracking (Not Started, In Progress, Completed, Cancelled)
- [ ] Calendar view for tasks
- [ ] Link tasks to contacts/companies

### Email Integration
- [ ] Send email from contact record
- [ ] Email templates (welcome, follow-up, etc.)
- [ ] Email history tracking per contact
- [ ] Bulk email sending
- [ ] Email open/click tracking
- [ ] SMTP configuration UI

### Contact Import/Export
- [ ] CSV import wizard with field mapping
- [ ] Duplicate detection during import
- [ ] CSV/Excel export with selected fields
- [ ] vCard export for contacts
- [ ] Import error reporting and rollback

### Notification System
- [x] Create Notification model and database
- [x] In-app notification bell and dropdown
- [x] Email notifications for tasks, invitations, etc.
- [x] Notification preferences per user
- [x] Mark as read/unread functionality
- [x] Notification categories (Task, Email, System, Alert)

### Reports & Analytics
- [ ] Lead conversion report
- [ ] Lead source analytics
- [ ] Contact growth over time chart
- [ ] User activity report
- [ ] Company performance report
- [ ] Export reports to PDF/Excel

### Settings & Configuration
- [ ] Application settings page
- [ ] User preferences (timezone, language, notifications)
- [ ] Company settings (branding, default values)
- [ ] SMTP settings configuration
- [ ] Email signature settings

---

## 🚀 Phase 3: Growth Features (MEDIUM Priority)

### Advanced Contact Features
- [ ] Contact scoring/grading system
- [ ] Lead pipeline stages
- [ ] Custom fields for contacts
- [ ] Contact relationship mapping
- [ ] Contact communication timeline
- [ ] Social media profile fields
- [ ] Contact duplicate detection and merge

### Call & Meeting Management
- [ ] Log calls against contacts
- [ ] Schedule and track meetings
- [ ] Meeting notes and outcomes
- [ ] Calendar integration (Google, Outlook)
- [ ] Meeting reminders

### REST API & Integrations
- [ ] Build public REST API endpoints
- [ ] Swagger/OpenAPI documentation
- [ ] API key authentication
- [ ] Webhook support for external integrations
- [ ] Third-party CRM integrations (Zapier, etc.)

### Mobile & UX Improvements
- [ ] PWA (Progressive Web App) support
- [ ] Improved mobile responsive design
- [ ] Dark mode and theme customization
- [ ] Kanban board view for leads/tasks
- [ ] Drag-and-drop functionality
- [ ] Keyboard shortcuts

### Data Management
- [ ] Data backup and restore
- [ ] GDPR compliance (data export, deletion)
- [ ] Contact deduplication tools
- [ ] Bulk operations (edit, delete, tag)
- [ ] Audit trail for contact changes

---

## 🔐 Security Enhancements

### Authentication & Authorization
- [ ] Two-Factor Authentication (2FA)
- [ ] OAuth/Social Login (Google, LinkedIn)
- [ ] Account lockout after failed login attempts
- [ ] Password strength policy enforcement
- [ ] Session management and device tracking
- [ ] API key management for integrations
- [ ] IP whitelisting for admin access

---

## 🧪 Testing & DevOps (CRITICAL)

### Testing
- [ ] Unit tests for domain logic (xUnit/NUnit)
- [ ] Unit tests for services and utilities
- [ ] Integration tests for API endpoints
- [ ] Integration tests for database operations
- [ ] E2E tests with Playwright/Selenium
- [ ] Achieve minimum 70% code coverage

### DevOps & CI/CD
- [ ] GitHub Actions CI/CD pipeline
- [ ] Docker Compose setup (app + PostgreSQL)
- [ ] Database seeding scripts
- [ ] Environment-specific configurations
- [ ] Health checks and monitoring
- [ ] Log aggregation (Serilog + Seq/ELK)
- [ ] Performance profiling and optimization

---

## 📚 Documentation

- [ ] API documentation (Swagger/Redoc)
- [ ] User manual/guide
- [ ] Developer onboarding documentation
- [ ] Deployment guide
- [ ] Database schema documentation
- [ ] Architecture decision records (ADRs)

---

## 📝 Notes
- **Database:** Two databases — `SmartLeadsSystemDb` (Users, Companies, Roles) and `SmartLeadsDb` (Contacts, Tags, Groups, Tasks, etc.)
- **Architecture:** Clean Architecture with Domain, Infrastructure, Utilities, and Web projects
- **Multi-Tenancy:** Data isolation via `CompanyId` filtering and `UserCompany` junction table for per-company roles
