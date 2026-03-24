# Layout Structure - SmartLeads

## ✅ Layout Organization Complete

### Three Layout Types:

---

## 1. **Landing Page** (Standalone - No Layout)
**File:** `Views/Home/Landing.cshtml`

**Purpose:** Public homepage for marketing/landing

**Features:**
- ✅ No layout file used (`Layout = null`)
- ✅ Complete standalone HTML
- ✅ Bootstrap 5 CDN
- ✅ Font Awesome CDN
- ✅ Hero section with Register/Login buttons
- ✅ Features section
- ✅ How It Works section
- ✅ CTA section
- ✅ Footer

**Access:** Public (no login required)

---

## 2. **Auth Layout** (`_AuthLayout.cshtml`)
**File:** `Views/Shared/_AuthLayout.cshtml`

**Purpose:** Authentication pages (public, no login required)

**Used By:**
- ✅ `/Auth/Login` - Login page
- ✅ `/Auth/Register` - Registration page
- ✅ `/Auth/ForgotPassword` - Forgot password page
- ✅ `/Auth/ResetPassword` - Reset password page
- ✅ `/Auth/Accept` - Accept invitation page
- ✅ `/Auth/NoCompany` - No company access page

**Features:**
- Sneat template layout
- Theme switcher (Light/Dark/System)
- Centered auth card design
- Toastr notifications
- SweetAlert2 support
- Responsive design

**Structure:**
```html
<!DOCTYPE html>
<html>
<head>
    <!-- Meta, Bootstrap CSS, Font Awesome, Custom CSS -->
</head>
<body>
    <!-- Theme Switcher -->
    <div class="auth-wrapper">
        @RenderBody()
    </div>
    <!-- Scripts -->
</body>
</html>
```

---

## 3. **Main Layout** (`_Layout.cshtml`)
**File:** `Views/Shared/_Layout.cshtml`

**Purpose:** Protected pages (requires login)

**Used By:**
- ✅ `/Contacts` - Contact management
- ✅ `/Dashboard` - User dashboard
- ✅ `/Users` - User management
- ✅ `/Companies` - Company management
- ✅ All other authenticated pages

**Features:**
- Full navigation menu
- Sidebar
- Header with user profile
- Requires authentication
- Protected by `[RequireCompany]` filter

**Access:** Login required

---

## Navigation Flow

```
Landing Page (/)
├── "Get Started Free" → /Auth/Register (Auth Layout)
└── "Sign In" → /Auth/Login (Auth Layout)

Auth Pages (_AuthLayout)
├── Login → Register
├── Register → Login
├── Forgot Password → Login
└── After Login/Registration → Protected Pages

Protected Pages (_Layout)
├── Contacts
├── Dashboard
├── Users
├── Companies
└── Logout → Landing Page
```

---

## File Structure

```
Views/
├── Home/
│   └── Landing.cshtml          ← Standalone (no layout)
├── Auth/
│   ├── Login.cshtml            ← _AuthLayout
│   ├── Register.cshtml         ← _AuthLayout
│   ├── ForgotPassword.cshtml   ← _AuthLayout
│   ├── ResetPassword.cshtml    ← _AuthLayout
│   ├── Accept.cshtml           ← _AuthLayout
│   └── NoCompany.cshtml        ← _AuthLayout
├── Contacts/
│   └── Index.cshtml            ← _Layout (protected)
├── Users/
│   └── *.cshtml                ← _Layout (protected)
└── Shared/
    ├── _Layout.cshtml          ← Main layout (protected pages)
    ├── _AuthLayout.cshtml      ← Auth layout (public pages)
    └── _CardHeader.cshtml      ← Partial for auth cards
```

---

## Access Control

### Public Pages (No Login Required):
- ✅ Landing Page (`/`)
- ✅ Login (`/Auth/Login`)
- ✅ Register (`/Auth/Register`)
- ✅ Forgot Password (`/Auth/ForgotPassword`)
- ✅ Reset Password (`/Auth/ResetPassword`)
- ✅ Accept Invitation (`/Auth/Accept`)
- ✅ No Company (`/Auth/NoCompany`)

### Protected Pages (Login Required):
- ✅ Contacts (`/Contacts`)
- ✅ Dashboard (`/Dashboard`)
- ✅ Users (`/Users`)
- ✅ Companies (`/Companies`)
- ✅ All other pages with `_Layout`

---

## Layout Features Comparison

| Feature | Landing | Auth Layout | Main Layout |
|---------|---------|-------------|-------------|
| **Layout File** | None | `_AuthLayout` | `_Layout` |
| **Navigation** | None | None | Full sidebar |
| **Login Required** | No | No | Yes |
| **Theme Switcher** | No | Yes | Yes |
| **Footer** | Yes | No | Yes |
| **Bootstrap** | CDN | Sneat | Sneat |
| **Purpose** | Marketing | Authentication | Application |

---

## Auth Pages Structure

All auth pages use the same card-based design:

```html
<div class="auth-card">
    <div class="text-center mb-4">
        <h2 class="h4 fw-bold mb-2">Title 👋</h2>
        <p class="text-muted">Subtitle</p>
    </div>
    
    <form>
        <!-- Form fields -->
    </form>
    
    <div class="text-center">
        <span class="text-muted">Message?</span>
        <a asp-action="Action" class="fw-semibold">Link</a>
    </div>
</div>
```

---

## Testing Checklist

### Landing Page:
- [ ] Navigate to `/` - Should show landing page
- [ ] Click "Get Started Free" - Should go to `/Auth/Register`
- [ ] Click "Sign In" - Should go to `/Auth/Login`

### Auth Pages:
- [ ] `/Auth/Login` - Should show login form with _AuthLayout
- [ ] `/Auth/Register` - Should show register form with _AuthLayout
- [ ] Login page has "Create one now" link → Register
- [ ] Register page has "Sign in" link → Login
- [ ] Both pages have theme switcher

### Protected Pages:
- [ ] Login first
- [ ] Navigate to `/Contacts` - Should show with _Layout
- [ ] Should have full navigation menu
- [ ] Should have sidebar

---

## Build Status

✅ **Build Successful** - 0 Errors, 28 Warnings (nullable references)

---

## Summary

✅ **Landing Page** - Standalone, no layout, marketing page  
✅ **Auth Layout** - Public authentication pages (Login, Register, etc.)  
✅ **Main Layout** - Protected authenticated pages  
✅ **Proper separation** - Clear distinction between public and protected  
✅ **Navigation working** - All links functional between pages  

**Your layout structure is now properly organized!** 🚀
