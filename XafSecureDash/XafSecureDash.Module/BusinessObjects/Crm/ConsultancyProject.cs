using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using System.Collections.ObjectModel;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    public enum ProjectStatus
    {
        Proposal,
        Active,
        OnHold,
        Completed,
        Cancelled
    }

    [DefaultClassOptions]
    [NavigationItem("Consultancy")]
    [DefaultProperty(nameof(Name))]
    [ImageName("BO_Task")]
    public class ConsultancyProject : BaseObject
    {
        [StringLength(200)]
        public virtual string Name { get; set; }

        [StringLength(50)]
        public virtual string ProjectCode { get; set; }

        public virtual Company Company { get; set; }

        public virtual Contact PrimaryContact { get; set; }

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

        public virtual ProjectStatus Status { get; set; }

        public virtual decimal BudgetAmount { get; set; }

        public virtual decimal HourlyRate { get; set; }

        public virtual int EstimatedHours { get; set; }

        [StringLength(1000)]
        public virtual string Description { get; set; }

        public virtual IList<TimeEntry> TimeEntries { get; set; } = new ObservableCollection<TimeEntry>();
    }
}
