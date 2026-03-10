# TODO — Secure Dashboard POC

## Status Legend
- [ ] Not started
- [x] Done
- [~] In progress

## Phase 1: Core Entities
- [x] Create `SecureDashboardData` entity (extends `DashboardData`, maps to `DashboardData` table)
- [x] Create `DashboardRoleAssignment` entity (links dashboards to roles via FK)
- [x] Register both entities in `XafSecureDashEFCoreDbContext`
- [x] Register `SecureDashboardData` as `DashboardDataType` in Blazor `Startup.cs`
- [x] Enable `AllowExecutingCustomSql` on dashboard configurator in Blazor `Startup.cs`

## Phase 2: Security Controller
- [x] Create `DashboardSecurityController` (ObjectViewController on ListView of SecureDashboardData)
- [x] Implement filter logic: admins see all, users see unrestricted + role-matched dashboards
- [ ] Test with Admin user (should see everything)
- [ ] Test with Default-role user (should see only permitted dashboards)

## Phase 3: Seed Data
- [x] Add sample dashboards in `Updater.cs` (Public Overview, User Dashboard, Manager Dashboard)
- [x] Add sample role assignments in `Updater.cs`
- [x] Create a second non-admin role ("Manager") to demonstrate differential access
- [x] Add dashboard read permissions to Default and Manager roles

## Phase 4: Polish
- [x] Verify navigation item placement (Reports group)
- [x] DashboardRoleAssignment in Administration nav group (admin manages)
- [x] Add role permissions for SecureDashboardData and DashboardRoleAssignment (read for Default/Manager)
- [ ] Manual testing with all 3 users (Admin, User, Manager)
