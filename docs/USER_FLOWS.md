# User Flow - Current Steps

## 1. Company Sign Up

1. User goes to `/Auth/Register`
2. Fills: Username, Email, Password, FirstName, LastName
3. System creates User record
4. System redirects to `/UserCompany/CreateCompany`
5. User fills: Company Name, Code, Email, Phone, Address (optional Parent Company)
6. System creates:
   - Company record
   - Employee record for the creator (auto-links to User)
   - UserCompany with SuperAdmin role
7. Redirects to Dashboard

## 2. Employee Create (Manual)

1. Admin goes to `/Employees` page
2. Opens create form
3. Fills: EmployeeId, FirstName, LastName, etc.
4. System creates Employee record under the company
5. Does NOT create User account

## 3. Invite User

1. Admin goes to `/Users` page
2. Opens invite form
3. Fills: Username, Email, Role, Employee details
4. System creates Invitation record with token
5. System sends email with link: `/Invitations/Accept?token=xxx&email=xxx`
6. **NOT IMPLEMENTED**: What happens next - no controller/view to accept and create account

---

**Current Status:**
- Company Sign Up: Done
- Employee Create: Done
- Invite Send: Done
- Invite Accept: Not implemented (controller missing)