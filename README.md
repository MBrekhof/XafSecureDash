# XafSecureDash

Proof-of-concept for **role-based dashboard security** in DevExpress XAF (EF Core, Blazor Server).

Out of the box, XAF dashboards are visible to anyone with read permission on `DashboardData`. This project adds per-dashboard role filtering: admins assign roles to dashboards, and users only see what their roles entitle them to.

## How It Works

Three pieces:

1. **`SecureDashboardData`** — Subclass of `DashboardData` registered as the dashboard data type. Maps to the same `DashboardData` table.

2. **`DashboardRoleAssignment`** — Join entity linking a dashboard to a role. Managed by admins through the standard XAF UI. Dashboards with no assignments are visible to all authenticated users.

3. **`DashboardSecurityController`** — `ObjectViewController<ListView, SecureDashboardData>` that filters the dashboard list based on the current user's roles. Admins bypass all filtering.

| User type | Sees |
|---|---|
| Admin (IsAdministrative) | All dashboards |
| User with matching role | Unrestricted + role-matched dashboards |
| User with no matching role | Only unrestricted dashboards |
| Not authenticated | Nothing |

## Custom SQL in Dashboards

Custom SQL is enabled (`AllowExecutingCustomSql = true`). EF Core's secured ObjectSpace is not practical for dashboard data queries — no CTEs, no window functions, no cross-database joins, significant ORM overhead. Dashboard data is fetched via raw SQL, bypassing XAF's row-level security. This is acceptable because:

- This project controls **which dashboards** a user sees, not the data within them
- Dashboard data is typically aggregate/reporting, not row-level sensitive
- The admin designing the dashboard controls the SQL

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB works out of the box)
- DevExpress XAF v25.2 license (packages are v25.2.3)

### Run

```bash
dotnet build XafSecureDash.slnx
dotnet run --project XafSecureDash/XafSecureDash.Blazor.Server
```

On first run, the database is created automatically with seed data:

| User | Password | Role | Dashboards visible |
|---|---|---|---|
| Admin | *(empty)* | Administrators | All 3 |
| User | *(empty)* | Default | Public Overview, User Dashboard |
| Manager | *(empty)* | Manager | Public Overview, Manager Dashboard |

### Connection String

Default in `appsettings.json` points to LocalDB:
```
Data Source=(localdb)\mssqllocaldb;Initial Catalog=XafSecureDash
```

## Project Structure

```
XafSecureDash.Module/                    # Shared platform-agnostic module
  BusinessObjects/Dashboard/
    SecureDashboardData.cs               # Custom DashboardData subclass
    DashboardRoleAssignment.cs           # Dashboard-to-role join entity
  Controllers/
    DashboardSecurityController.cs       # ListView filter by user roles
  DatabaseUpdate/Updater.cs              # Seed data (users, roles, dashboards)

XafSecureDash.Blazor.Server/             # Blazor Server frontend (primary)
XafSecureDash.WebApi/                    # REST/OData API with JWT auth
XafSecureDash.Win/                       # WinForms client
```

## Reference

Implementation pattern adapted from a production system. See `HOW_TO_IMPLEMENT.md` for a step-by-step guide targeted at XAF developers.
