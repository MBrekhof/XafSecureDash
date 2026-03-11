using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using System.Collections.ObjectModel;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        Overdue,
        Cancelled
    }

    [DefaultClassOptions]
    [NavigationItem("Sales")]
    [DefaultProperty(nameof(InvoiceNumber))]
    [ImageName("BO_Invoice")]
    public class Invoice : BaseObject
    {
        [StringLength(20)]
        public virtual string InvoiceNumber { get; set; }

        public virtual Company Company { get; set; }

        public virtual Order Order { get; set; }

        public virtual DateTime InvoiceDate { get; set; }

        public virtual DateTime DueDate { get; set; }

        public virtual DateTime? PaidDate { get; set; }

        public virtual InvoiceStatus Status { get; set; }

        public virtual decimal TotalAmount { get; set; }

        public virtual decimal TaxAmount { get; set; }

        [StringLength(500)]
        public virtual string Notes { get; set; }

        public virtual IList<InvoiceLine> Lines { get; set; } = new ObservableCollection<InvoiceLine>();
    }
}
