# TODO — Secure Dashboard POC

## Status Legend
- [ ] Not started
- [x] Done
- [~] In progress

## Phase 1: Core Entities
- [ ] Create `SecureDashboardData` entity (extends `DashboardData`, maps to `DashboardData` table)
- [ ] Create `DashboardRoleAssignment` entity (links dashboards to roles via FK)
- [ ] Register both entities in `XafSecureDashEFCoreDbContext`
- [ ] Register `SecureDashboardData` as `DashboardDataType` in Blazor `Startup.cs`
- [ ] Enable `AllowExecutingCustomSql` on dashboard configurator in Blazor `Startup.cs`

## Phase 2: Security Controller
- [ ] Create `DashboardSecurityController` (ObjectViewController on ListView of SecureDashboardData)
- [ ] Implement filter logic: admins see all, users see unrestricted + role-matched dashboards
- [ ] Test with Admin user (should see everything)
- [ ] Test with Default-role user (should see only permitted dashboards)

## Phase 3: Seed Data
- [ ] Add sample dashboards in `Updater.cs`
- [ ] Add sample role assignments in `Updater.cs`
- [ ] Create a second non-admin role ("Manager") to demonstrate differential access

## Phase 4: Polish
- [ ] Verify navigation item placement (Reports group)
- [ ] Verify DashboardRoleAssignment is manageable via XAF UI (admin only)
- [ ] Add role permissions for DashboardRoleAssignment (read for Default, full for Admin)
