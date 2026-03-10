using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Dashboard
{
    [DefaultClassOptions]
    [NavigationItem("Reports")]
    [DefaultProperty("Title")]
    [Table("DashboardData")]
    public class SecureDashboardData : DashboardData
    {
    }
}
