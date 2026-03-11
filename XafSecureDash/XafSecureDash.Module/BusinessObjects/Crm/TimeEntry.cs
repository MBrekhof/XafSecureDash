using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    [DefaultClassOptions]
    [NavigationItem("Consultancy")]
    [DefaultProperty(nameof(Description))]
    [ImageName("BO_Audit_ChangeHistory")]
    public class TimeEntry : BaseObject
    {
        public virtual ConsultancyProject Project { get; set; }

        public virtual Contact Consultant { get; set; }

        public virtual DateTime Date { get; set; }

        public virtual decimal Hours { get; set; }

        [StringLength(500)]
        public virtual string Description { get; set; }

        public virtual bool IsBillable { get; set; } = true;

        public virtual bool IsInvoiced { get; set; }
    }
}
