# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Session Startup

**At the start of every session, read these files before doing anything else:**
1. `TODO.md` — current task list and progress
2. `SESSION_HANDOFF.md` — what was done last session, what's next, blockers

Also read `HOW_TO_IMPLEMENT.md` for the implementation pattern (role-based secure dashboards).

## Project Goal

POC for **role-based secure dashboards**: users only see dashboards their roles entitle them to. Admins assign roles to dashboards via `DashboardRoleAssignment`. Dashboards with no assignments are visible to all authenticated users. Reference implementation: `C:\Projects\wlncentral` (BusinessObjects/Dashboard/, Controllers/DashboardSecurityController.cs).

## Build & Run

```bash
# Build entire solution
dotnet build XafSecureDash.slnx

# Run Blazor Server app (primary UI)
dotnet run --project XafSecureDash/XafSecureDash.Blazor.Server

# Run WebApi project
dotnet run --project XafSecureDash/XafSecureDash.WebApi

# Run WinForms app
dotnet run --project XafSecureDash/XafSecureDash.Win

# Build configurations: Debug, Release, EasyTest
dotnet build XafSecureDash.slnx -c EasyTest
```

No test projects exist yet. No migrations have been created yet.

## EF Core Migrations

Migrations target the Module project's DbContext. Run from solution root:

```bash
dotnet ef migrations add <Name> --project XafSecureDash/XafSecureDash.Module --startup-project XafSecureDash/XafSecureDash.Blazor.Server
dotnet ef database update --project XafSecureDash/XafSecureDash.Module --startup-project XafSecureDash/XafSecureDash.Blazor.Server
```

## Architecture

This is a DevExpress XAF (eXpressApp Framework) v25.2 application using EF Core on .NET 8. Uses `.slnx` solution format.

### Projects

- **XafSecureDash.Module** — Shared platform-agnostic module. Contains all business objects (EF Core entities in `BusinessObjects/`), the `XafSecureDashEFCoreDbContext`, database seed logic (`DatabaseUpdate/Updater.cs`), and module registration (`Module.cs`). All other projects reference this.
- **XafSecureDash.Blazor.Server** — Blazor Server frontend. Cookie authentication, DevExpress Fluent theme. Registers XAF modules (Dashboards, Reports, Office, Notifications, etc.) in `Startup.cs`.
- **XafSecureDash.WebApi** — REST/OData Web API with JWT bearer authentication and Swagger UI. Custom controllers in `API/` folder (Reports, Security/Auth). OData route prefix: `api/odata`.
- **XafSecureDash.Win** — WinForms desktop client.

### Key Patterns

- **Security**: XAF Integrated Security with `PermissionPolicyRole` and custom `ApplicationUser` (extends `PermissionPolicyUser` with OAuth/lockout support). Blazor uses cookie auth; WebApi uses JWT.
- **DbContext**: `XafSecureDashEFCoreDbContext` uses optimistic locking (`UseOptimisticLock`) and change tracking with `ChangingAndChangedNotificationsWithOriginalValues`. No deferred deletion.
- **Database**: SQL Server in Docker (`localhost,1433`, sa/YourStrongPassw0rd). Connection string key: `ConnectionString` in appsettings.json. Dashboard Custom SQL uses `DashboardSqlConnection` (requires `XpoProvider=MSSqlServer` prefix).
- **Logging**: Serilog with daily rolling file sink in `logs/` directory.
- **Seed data**: Debug/EasyTest builds create "Admin" (Administrators role) and "User" (Default role) accounts with empty passwords. Controlled by `#if !RELEASE` in `Updater.cs`.
- **XAF Modules enabled**: ConditionalAppearance, Dashboards, FileAttachments, Notifications, Office, PivotGrid, Charts, Reports, TreeListEditors, Validation, ViewVariants.

### DevExpress Version

All DevExpress packages are pinned to **25.2.3**. Keep versions synchronized when updating.
