using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using System.Collections.ObjectModel;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    public enum OrderStatus
    {
        Draft,
        Confirmed,
        InProgress,
        Shipped,
        Delivered,
        Cancelled
    }

    [DefaultClassOptions]
    [NavigationItem("Sales")]
    [DefaultProperty(nameof(OrderNumber))]
    [ImageName("BO_Order")]
    public class Order : BaseObject
    {
        [StringLength(20)]
        public virtual string OrderNumber { get; set; }

        public virtual Company Company { get; set; }

        public virtual Contact Contact { get; set; }

        public virtual DateTime OrderDate { get; set; }

        public virtual DateTime? ShippedDate { get; set; }

        public virtual OrderStatus Status { get; set; }

        [StringLength(500)]
        public virtual string Notes { get; set; }

        public virtual decimal TotalAmount { get; set; }

        public virtual decimal Discount { get; set; }

        public virtual IList<OrderLine> Lines { get; set; } = new ObservableCollection<OrderLine>();
    }
}
