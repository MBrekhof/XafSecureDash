# Session Handoff

## Current State
Dashboard infrastructure fully working on both Blazor and WinForms. Custom SQL dashboards with CRM data operational. Security filtering (DashboardSecurityController) implemented but not yet tested with non-admin users.

## What Was Done This Session
- Added Serilog logging (bootstrap logger + file sink with daily rolling)
- Added `DashboardConnectionStringsProvider` to Blazor Startup.cs for Custom SQL data sources
- Created `DashboardSettingsHelper.razor` + `DashboardCustomSqlQueryController` for Custom SQL in Blazor
- Switched from LocalDB to Docker SQL Server (`localhost,1433`, sa/YourStrongPassw0rd)
- Removed `UseDeferredDeletion` from DbContext (was causing GCRecord filtering issues)
- Removed seed dashboards from Updater.cs (empty dashboards caused confusion)
- Fixed `DashboardSecurityController` to handle missing `INonSecuredObjectSpaceFactory` on WinForms (GetService instead of GetRequiredService)
- Added diagnostic logging to `DashboardSecurityController`
- Updated WinForms `DashboardDataType` to use `SecureDashboardData`
- Updated WinForms `App.config` to use Docker SQL Server with dashboard connection strings
- Changed sa password to `YourStrongPassw0rd` (removed `!` which caused XPO parsing issues)

## Key Technical Details
- **Dashboard Custom SQL**: Requires `XpoProvider=MSSqlServer` prefix in connection string. Standard ADO.NET strings won't work.
- **Connection name resolution**: Dashboard XML stores connection names (e.g. `localhost_Connection`). Both `App.config` and `appsettings.json` must have matching entries.
- **GCRecord**: With `UseDeferredDeletion` removed, GCRecord column in existing tables is harmless but not used.
- **WinForms + INonSecuredObjectSpaceFactory**: Not registered in WinForms DI. Controller gracefully skips security filtering when unavailable.
- **Dashboard SQL query for CRM data** (no GCRecord filter needed):
  ```sql
  SELECT c.Name AS CompanyName, c.City, c.Country, i.InvoiceNumber, i.InvoiceDate, i.DueDate, i.Status, i.TotalAmount, i.TaxAmount
  FROM Companies c LEFT JOIN Invoices i ON i.CompanyID = c.ID
  ORDER BY c.Name, i.InvoiceDate
  ```

## What's Next
- Test dashboard security filtering with non-admin users (Admin, User, Manager)
- Create role assignments and verify visibility rules
- This is the core purpose of the POC — everything else was infrastructure

## Blockers
None.
