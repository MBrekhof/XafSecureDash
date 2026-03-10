# How to Implement: Role-Based Secure Dashboards in XAF (EF Core)

Targeted at XAF developers. This is the pattern used in production at wlncentral.

## Concept

Out of the box, XAF's `DashboardData` entity is visible to all users with read permission. This POC adds **per-dashboard role filtering**: an admin assigns roles to dashboards, and users only see dashboards matching their roles. Dashboards with no role assignments remain visible to everyone (open by default).

## Architecture (3 pieces)

### 1. `SecureDashboardData` — Custom DashboardData subclass

```csharp
[DefaultClassOptions]
[NavigationItem("Reports")]
[DefaultProperty("Title")]
[Table("DashboardData")]  // Maps to same table as base DashboardData
public class SecureDashboardData : DashboardData { }
```

Why a subclass? So we can set `options.DashboardDataType = typeof(SecureDashboardData)` and attach our `ObjectViewController` specifically to this type without affecting other DashboardData consumers.

### 2. `DashboardRoleAssignment` — Join entity

```csharp
[DefaultClassOptions]
[Table("DashboardRoleAssignment")]
public class DashboardRoleAssignment : BaseObject
{
    public virtual Guid? DashboardID { get; set; }

    [ForeignKey("DashboardID")]
    public virtual SecureDashboardData Dashboard { get; set; }

    public virtual Guid? RoleID { get; set; }

    [ForeignKey("RoleID")]
    public virtual PermissionPolicyRole Role { get; set; }

    public virtual string Notes { get; set; }
    public virtual DateTime? AssignedDate { get; set; }
    public virtual string AssignedBy { get; set; }
}
```

Key points:
- Uses explicit FK properties (`DashboardID`, `RoleID`) — EF Core maps cleanly
- `BaseObject` gives you `ID`, `GCRecord` (deferred deletion), `OptimisticLockField`
- Auto-sets `AssignedDate` and `AssignedBy` in `OnCreated()`
- Admins manage this via standard XAF ListView/DetailView — no custom UI needed

### 3. `DashboardSecurityController` — ListView filter

```csharp
public class DashboardSecurityController : ObjectViewController<ListView, SecureDashboardData>
```

Activated whenever a `SecureDashboardData` ListView is shown. Filter logic:

| User State | What they see |
|---|---|
| Not authenticated | Nothing (`1=0` criteria) |
| Admin (IsAdministrative) | Everything (no filter) |
| Has roles, some match assignments | Unrestricted dashboards + matched dashboards |
| Has roles, none match | Only unrestricted dashboards |
| No roles at all | Only unrestricted dashboards |

"Unrestricted" = a dashboard with zero `DashboardRoleAssignment` records.

The controller queries `DashboardRoleAssignment` via a separate ObjectSpace, computes which dashboard IDs to exclude, and sets `View.CollectionSource.Criteria["DashboardSecurity"]`.

## Wiring It Up

### DbContext — register new DbSets

```csharp
public DbSet<SecureDashboardData> DashboardData { get; set; }  // replaces old DashboardData DbSet
public DbSet<DashboardRoleAssignment> DashboardRoleAssignments { get; set; }
```

### Startup.cs (Blazor) — point dashboard module at custom type

```csharp
.AddDashboards(options =>
{
    options.DashboardDataType = typeof(SecureDashboardData);
    options.SetupDashboardConfigurator = (configurator, serviceProvider) =>
    {
        configurator.AllowExecutingCustomSql = true;
    };
})
```

### Module.cs — export the types

```csharp
AdditionalExportedTypes.Add(typeof(SecureDashboardData));
AdditionalExportedTypes.Add(typeof(DashboardRoleAssignment));
```

### Database

EF Core auto-creates `DashboardRoleAssignment` table on schema update. For production, use the SQL script to add proper indexes and unique constraint on (DashboardID, RoleID).

## Custom SQL in Dashboards

Custom SQL is enabled via `AllowExecutingCustomSql = true` on the dashboard configurator. This is a deliberate choice: EF Core's secured ObjectSpace is not practical for dashboard data — the ORM overhead, lack of CTEs/window functions, and inability to query across databases make it unusable for real-world dashboarding scenarios.

**What this means**: data inside dashboards is fetched via raw SQL, bypassing XAF's Integrated Security row-level filtering. This is acceptable because:
- This POC controls **which dashboards** a user can see (via `DashboardRoleAssignment`), not the data within them
- Dashboard data is typically aggregate/reporting data, not row-level sensitive
- The admin who designs the dashboard controls the SQL and therefore what data is exposed
- If per-user data filtering is needed inside a dashboard, use parameterized SQL with user context

## Testing the POC

1. Run the app, log in as Admin
2. Create a dashboard
3. Create a `DashboardRoleAssignment` linking that dashboard to the "Default" role
4. Log in as User (Default role) — should see the dashboard
5. Create another dashboard, do NOT assign any roles — both users should see it
6. Create a third dashboard, assign it to a role User doesn't have — User should NOT see it
