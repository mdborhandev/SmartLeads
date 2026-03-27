#!/bin/bash
# Test script for per-company roles functionality

BASE_URL="http://localhost:5284"

echo "========================================="
echo "SmartLeads Per-Company Roles Test"
echo "========================================="
echo ""

# Test 1: Check if application is running
echo "Test 1: Checking application status..."
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" ${BASE_URL}/Auth/Login)
if [ "$RESPONSE" == "200" ]; then
    echo "✅ Application is running (HTTP $RESPONSE)"
else
    echo "❌ Application not responding (HTTP $RESPONSE)"
    exit 1
fi
echo ""

# Test 2: Check database migration
echo "Test 2: Checking database migration..."
PGPASSWORD=borhan444 psql -h localhost -U borhanuddin -d SystemDbSmartLeads -c "
SELECT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'usercompanies' AND column_name = 'role'
) as role_column_exists;" 2>/dev/null | grep -q "t" && echo "✅ Role column exists in UserCompanies" || echo "❌ Role column missing"

PGPASSWORD=borhan444 psql -h localhost -U borhanuddin -d SystemDbSmartLeads -c "
SELECT NOT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_name = 'users' AND column_name = 'role'
) as role_column_removed;" 2>/dev/null | grep -q "t" && echo "✅ Role column removed from Users" || echo "❌ Role column still exists in Users"
echo ""

# Test 3: Verify UserCompany table structure
echo "Test 3: Verifying UserCompany table structure..."
PGPASSWORD=borhan444 psql -h localhost -U borhanuddin -d SystemDbSmartLeads -c "\d \"UserCompanies\"" 2>/dev/null | grep -q "Role" && echo "✅ UserCompany has Role column" || echo "❌ UserCompany missing Role column"
echo ""

echo "========================================="
echo "Manual Testing Required:"
echo "========================================="
echo "1. Go to http://localhost:5284/Auth/Register"
echo "2. Register a new user account"
echo "3. Create a new company"
echo "4. Check that user gets SuperAdmin role"
echo "5. Create another company"
echo "6. Verify second company is not set as default"
echo "7. Use company switcher to switch between companies"
echo "8. Verify role badge shows in company dropdown"
echo ""
echo "Database verification query:"
echo "SELECT uc.\"Role\", uc.\"IsDefault\", c.\"Name\" FROM \"UserCompanies\" uc"
echo "JOIN \"Companies\" c ON uc.\"CompanyId\" = c.\"Id\";"
echo "========================================="
