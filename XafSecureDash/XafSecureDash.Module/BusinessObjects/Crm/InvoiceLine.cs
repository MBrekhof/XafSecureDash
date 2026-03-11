using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    [DefaultProperty(nameof(Description))]
    public class InvoiceLine : BaseObject
    {
        public virtual Invoice Invoice { get; set; }

        public virtual Product Product { get; set; }

        [StringLength(300)]
        public virtual string Description { get; set; }

        public virtual int Quantity { get; set; }

        public virtual decimal UnitPrice { get; set; }

        public virtual decimal LineTotal { get; set; }
    }
}
