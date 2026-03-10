using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;

namespace XafSecureDash.Module.BusinessObjects.Dashboard
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [DefaultProperty("DisplayName")]
    [Table("DashboardRoleAssignment")]
    [ImageName("BO_Security_Permission_Object")]
    public class DashboardRoleAssignment : BaseObject
    {
        [Column("DashboardID")]
        public virtual Guid? DashboardID { get; set; }

        [ImmediatePostData]
        [ForeignKey("DashboardID")]
        public virtual SecureDashboardData Dashboard { get; set; }

        [Column("RoleID")]
        public virtual Guid? RoleID { get; set; }

        [ImmediatePostData]
        [ForeignKey("RoleID")]
        public virtual PermissionPolicyRole Role { get; set; }

        [ModelDefault("RowCount", "3")]
        public virtual string Notes { get; set; }

        [ModelDefault("AllowEdit", "False")]
        public virtual DateTime? AssignedDate { get; set; }

        [ModelDefault("AllowEdit", "False")]
        public virtual string AssignedBy { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                var dashboardTitle = Dashboard?.Title ?? "(No Dashboard)";
                var roleName = Role?.Name ?? "(No Role)";
                return $"{dashboardTitle} - {roleName}";
            }
        }

        public override void OnCreated()
        {
            base.OnCreated();
            AssignedDate = DateTime.Now;

            var objectSpace = ((IObjectSpaceLink)this).ObjectSpace;
            if (objectSpace != null)
            {
                var securityStrategy = objectSpace.ServiceProvider?.GetService(typeof(DevExpress.ExpressApp.Security.ISecurityStrategyBase)) as DevExpress.ExpressApp.Security.ISecurityStrategyBase;
                if (securityStrategy?.User is ApplicationUser appUser)
                {
                    AssignedBy = appUser.UserName;
                }
            }
        }
    }
}
