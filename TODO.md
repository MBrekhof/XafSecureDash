# TODO — Secure Dashboard POC

## Status Legend
- [ ] Not started
- [x] Done
- [~] In progress

## Phase 1: Core Entities
- [x] Create `SecureDashboardData` entity (extends `DashboardData`, maps to `DashboardData` table)
- [x] Create `DashboardRoleAssignment` entity (links dashboards to roles via FK)
- [x] Register both entities in `XafSecureDashEFCoreDbContext`
- [x] Register `SecureDashboardData` as `DashboardDataType` in Blazor and WinForms

## Phase 2: Security Controller
- [x] Create `DashboardSecurityController` (ObjectViewController on ListView of SecureDashboardData)
- [x] Implement filter logic: admins see all, users see unrestricted + role-matched dashboards
- [x] Add diagnostic logging (Serilog)
- [x] Handle WinForms gracefully (no INonSecuredObjectSpaceFactory)
- [x] Test with Admin user (should see everything)
- [x] Test with Default-role user (should see only permitted dashboards)

## Phase 3: Dashboard Infrastructure
- [x] Add Serilog logging with daily rolling file sink
- [x] Enable Custom SQL data sources (DashboardSettingsHelper + DashboardCustomSqlQueryController)
- [x] Configure DashboardConnectionStringsProvider for Blazor
- [x] Switch to Docker SQL Server (localhost,1433)
- [x] Add CRM seed data (~2,734 records across 9 entities)
- [x] Create working dashboard with Custom SQL query against CRM data

## Phase 3b: Connection String Hardening
- [x] Using same approach as wlncentral: `DashboardConnectionStringsProvider(Configuration)` reads from `ConnectionStrings` in appsettings.json. Add wizard-generated connection names manually as needed. No dynamic event handler required.

## Phase 4: Security Testing
- [x] Create role assignments for dashboards (seeded in Updater.cs)
- [x] Test visibility rules with Admin, User, Manager users (29/29 Playwright tests pass)
- [x] Verify "no assignments = visible to all" behavior
- [x] Verify "with assignments = restricted to matching roles" behavior
