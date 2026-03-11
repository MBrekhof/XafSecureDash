using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    [DefaultProperty(nameof(Product))]
    public class OrderLine : BaseObject
    {
        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }

        public virtual int Quantity { get; set; }

        public virtual decimal UnitPrice { get; set; }

        public virtual decimal LineTotal { get; set; }
    }
}
