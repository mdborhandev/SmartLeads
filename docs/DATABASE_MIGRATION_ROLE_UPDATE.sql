-- ============================================================
-- Migration Script: Add Role to UserCompany and Remove from User
-- ============================================================
-- This script updates the database schema to support per-company roles
-- Run this in pgAdmin on your database

-- STEP 1: Add Role column to UserCompanies table
ALTER TABLE "UserCompanies" 
ADD COLUMN "Role" VARCHAR(50) NOT NULL DEFAULT 'User';

-- STEP 2: Update existing UserCompanies to have SuperAdmin role 
-- (for users who created the company or are admins)
UPDATE "UserCompanies" uc
SET "Role" = 'SuperAdmin'
WHERE uc."IsDefault" = true 
  AND EXISTS (
      SELECT 1 FROM "Users" u 
      WHERE u."Id" = uc."UserId"
  );

-- STEP 3: Remove Role column from Users table
ALTER TABLE "Users" 
DROP COLUMN "Role";

-- STEP 4: Verify the changes
-- Check UserCompanies roles
SELECT uc."Id", uc."UserId", uc."CompanyId", uc."Role", uc."IsDefault", c."Name" as "CompanyName"
FROM "UserCompanies" uc
JOIN "Companies" c ON uc."CompanyId" = c."Id"
ORDER BY uc."CreatedAt" DESC;

-- Check Users table (Role should be gone)
SELECT "Id", "Username", "Email", "FirstName", "LastName"
FROM "Users"
LIMIT 10;
