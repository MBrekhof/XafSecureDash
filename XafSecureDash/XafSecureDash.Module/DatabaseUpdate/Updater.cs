using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EF;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.Extensions.DependencyInjection;
using XafSecureDash.Module.BusinessObjects;
using XafSecureDash.Module.BusinessObjects.Crm;
using XafSecureDash.Module.BusinessObjects.Dashboard;

namespace XafSecureDash.Module.DatabaseUpdate
{
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion)
        {
        }
        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
            //string name = "MyName";
            //EntityObject1 theObject = ObjectSpace.FirstOrDefault<EntityObject1>(u => u.Name == name);
            //if(theObject == null) {
            //    theObject = ObjectSpace.CreateObject<EntityObject1>();
            //    theObject.Name = name;
            //}

            // The code below creates users and roles for testing purposes only.
            // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
            // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
#if !RELEASE
            // If a role doesn't exist in the database, create this role
            var defaultRole = CreateDefaultRole();
            var adminRole = CreateAdminRole();
            var managerRole = CreateManagerRole();

            ObjectSpace.CommitChanges();

            UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();

            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null)
            {
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "User", EmptyPassword, (user) =>
                {
                    user.Roles.Add(defaultRole);
                });
            }

            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null)
            {
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Admin", EmptyPassword, (user) =>
                {
                    user.Roles.Add(adminRole);
                });
            }

            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Manager") == null)
            {
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Manager", EmptyPassword, (user) =>
                {
                    user.Roles.Add(managerRole);
                });
            }

            ObjectSpace.CommitChanges();

            // Seed dashboards for testing
            SeedDashboards(defaultRole, managerRole);

            CrmDataSeeder.Seed(ObjectSpace);
#endif
        }
        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
        }
        PermissionPolicyRole CreateAdminRole()
        {
            PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
            if (adminRole == null)
            {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
                adminRole.IsAdministrative = true;
            }
            return adminRole;
        }
        PermissionPolicyRole CreateManagerRole()
        {
            PermissionPolicyRole managerRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Manager");
            if (managerRole == null)
            {
                managerRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                managerRole.Name = "Manager";

                // Same base permissions as Default
                managerRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                managerRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                managerRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                managerRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
                managerRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                managerRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                // Dashboard read access
                managerRole.AddTypePermissionsRecursively<SecureDashboardData>(SecurityOperations.Read, SecurityPermissionState.Allow);
                managerRole.AddNavigationPermission(@"Application/NavigationItems/Items/Reports/Items/SecureDashboardData_ListView", SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<DashboardRoleAssignment>(SecurityOperations.Read, SecurityPermissionState.Allow);

                // CRM read access
                managerRole.AddTypePermissionsRecursively<Company>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<Contact>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<Product>(SecurityOperations.Read, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<Order>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<OrderLine>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<Invoice>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<InvoiceLine>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<ConsultancyProject>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddTypePermissionsRecursively<TimeEntry>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                managerRole.AddNavigationPermission(@"Application/NavigationItems/Items/CRM", SecurityPermissionState.Allow);
                managerRole.AddNavigationPermission(@"Application/NavigationItems/Items/Sales", SecurityPermissionState.Allow);
                managerRole.AddNavigationPermission(@"Application/NavigationItems/Items/Consultancy", SecurityPermissionState.Allow);
                managerRole.AddNavigationPermission(@"Application/NavigationItems/Items/Products", SecurityPermissionState.Allow);
            }
            return managerRole;
        }

        void SeedDashboards(PermissionPolicyRole defaultRole, PermissionPolicyRole managerRole)
        {
            // Create 3 test dashboards if they don't exist
            var publicDashboard = ObjectSpace.FirstOrDefault<SecureDashboardData>(d => d.Title == "Public Overview");
            if (publicDashboard == null)
            {
                publicDashboard = ObjectSpace.CreateObject<SecureDashboardData>();
                publicDashboard.Title = "Public Overview";
                publicDashboard.Content = @"<Dashboard CurrencyCulture=""en-US""><Title Text=""Public Overview"" /></Dashboard>";
            }

            var userDashboard = ObjectSpace.FirstOrDefault<SecureDashboardData>(d => d.Title == "User Dashboard");
            if (userDashboard == null)
            {
                userDashboard = ObjectSpace.CreateObject<SecureDashboardData>();
                userDashboard.Title = "User Dashboard";
                userDashboard.Content = @"<Dashboard CurrencyCulture=""en-US""><Title Text=""User Dashboard"" /></Dashboard>";
            }

            var managerDashboard = ObjectSpace.FirstOrDefault<SecureDashboardData>(d => d.Title == "Manager Dashboard");
            if (managerDashboard == null)
            {
                managerDashboard = ObjectSpace.CreateObject<SecureDashboardData>();
                managerDashboard.Title = "Manager Dashboard";
                managerDashboard.Content = @"<Dashboard CurrencyCulture=""en-US""><Title Text=""Manager Dashboard"" /></Dashboard>";
            }

            ObjectSpace.CommitChanges();

            // Create role assignments: User Dashboard -> Default, Manager Dashboard -> Manager
            // Public Overview has NO assignments (visible to all)
            var userAssignment = ObjectSpace.FirstOrDefault<DashboardRoleAssignment>(
                a => a.DashboardID == userDashboard.ID && a.RoleID == defaultRole.ID);
            if (userAssignment == null)
            {
                userAssignment = ObjectSpace.CreateObject<DashboardRoleAssignment>();
                userAssignment.Dashboard = ObjectSpace.GetObject(userDashboard);
                userAssignment.Role = ObjectSpace.GetObject(defaultRole);
            }

            var managerAssignment = ObjectSpace.FirstOrDefault<DashboardRoleAssignment>(
                a => a.DashboardID == managerDashboard.ID && a.RoleID == managerRole.ID);
            if (managerAssignment == null)
            {
                managerAssignment = ObjectSpace.CreateObject<DashboardRoleAssignment>();
                managerAssignment.Dashboard = ObjectSpace.GetObject(managerDashboard);
                managerAssignment.Role = ObjectSpace.GetObject(managerRole);
            }

            ObjectSpace.CommitChanges();
        }

        PermissionPolicyRole CreateDefaultRole()
        {
            PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
            if (defaultRole == null)
            {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                // Dashboard read access
                defaultRole.AddTypePermissionsRecursively<SecureDashboardData>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Reports/Items/SecureDashboardData_ListView", SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<DashboardRoleAssignment>(SecurityOperations.Read, SecurityPermissionState.Allow);

                // CRM read access
                defaultRole.AddTypePermissionsRecursively<Company>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<Contact>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<Product>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<Order>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<OrderLine>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<Invoice>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<InvoiceLine>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ConsultancyProject>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<TimeEntry>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/CRM", SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Sales", SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Consultancy", SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Products", SecurityPermissionState.Allow);
            }
            return defaultRole;
        }
    }
}
