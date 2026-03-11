using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Dashboards.Blazor.Components;
using DevExpress.Persistent.Base;
using XafSecureDash.Blazor.Server.Components;

namespace XafSecureDash.Blazor.Server.Controllers
{
    public class DashboardCustomSqlQueryController : ObjectViewController<DetailView, IDashboardData>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            View.CustomizeViewItemControl<BlazorDashboardViewerViewItem>(this, CustomizeDashboardViewerViewItem);
        }

        void CustomizeDashboardViewerViewItem(BlazorDashboardViewerViewItem dashboardViewerViewItem)
        {
            dashboardViewerViewItem.ComponentModel.ChildContent =
                DashboardSettingsHelper.Create(dashboardViewerViewItem.ComponentModel.ChildContent);
        }
    }
}
