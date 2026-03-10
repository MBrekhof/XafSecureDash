using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using XafSecureDash.Module.BusinessObjects;
using XafSecureDash.Module.BusinessObjects.Dashboard;

namespace XafSecureDash.Module.Controllers
{
    public class DashboardSecurityController : ObjectViewController<ListView, SecureDashboardData>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            ApplyDashboardSecurityFilter();
        }

        private void ApplyDashboardSecurityFilter()
        {
            var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
            if (currentUser == null)
            {
                View.CollectionSource.Criteria["DashboardSecurity"] = CriteriaOperator.Parse("1=0");
                return;
            }

            var isAdmin = currentUser.Roles.OfType<PermissionPolicyRole>().Any(r => r.IsAdministrative);
            if (isAdmin)
            {
                View.CollectionSource.Criteria.Remove("DashboardSecurity");
                return;
            }

            var userRoleIds = currentUser.Roles.OfType<PermissionPolicyRole>().Select(r => r.ID).ToList();
            var dashboardIdsWithAnyAssignments = GetDashboardIdsWithAssignments();

            if (!dashboardIdsWithAnyAssignments.Any())
            {
                View.CollectionSource.Criteria.Remove("DashboardSecurity");
                return;
            }

            if (!userRoleIds.Any())
            {
                var idsString = string.Join(", ", dashboardIdsWithAnyAssignments.Select(id => $"'{id}'"));
                View.CollectionSource.Criteria["DashboardSecurity"] = CriteriaOperator.Parse($"Not ID In ({idsString})");
                return;
            }

            var accessibleDashboardIds = GetAccessibleDashboardIds(userRoleIds);
            var restrictedIds = dashboardIdsWithAnyAssignments.Except(accessibleDashboardIds).ToList();

            if (restrictedIds.Any())
            {
                var idsString = string.Join(", ", restrictedIds.Select(id => $"'{id}'"));
                View.CollectionSource.Criteria["DashboardSecurity"] = CriteriaOperator.Parse($"Not ID In ({idsString})");
            }
            else
            {
                View.CollectionSource.Criteria.Remove("DashboardSecurity");
            }
        }

        private List<Guid> GetDashboardIdsWithAssignments()
        {
            using var os = Application.CreateObjectSpace(typeof(DashboardRoleAssignment));
            var assignments = os.GetObjects<DashboardRoleAssignment>();
            return assignments
                .Where(a => a.Dashboard != null)
                .Select(a => a.Dashboard.ID)
                .Distinct()
                .ToList();
        }

        private List<Guid> GetAccessibleDashboardIds(List<Guid> userRoleIds)
        {
            using var os = Application.CreateObjectSpace(typeof(DashboardRoleAssignment));
            var assignments = os.GetObjects<DashboardRoleAssignment>();
            return assignments
                .Where(a => a.Dashboard != null && a.Role != null && userRoleIds.Contains(a.Role.ID))
                .Select(a => a.Dashboard.ID)
                .Distinct()
                .ToList();
        }

        protected override void OnDeactivated()
        {
            if (View?.CollectionSource?.Criteria != null)
            {
                View.CollectionSource.Criteria.Remove("DashboardSecurity");
            }
            base.OnDeactivated();
        }
    }
}
