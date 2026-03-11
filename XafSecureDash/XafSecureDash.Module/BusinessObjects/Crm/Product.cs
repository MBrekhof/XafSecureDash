using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    [DefaultClassOptions]
    [NavigationItem("Products")]
    [DefaultProperty(nameof(Name))]
    [ImageName("BO_Product")]
    public class Product : BaseObject
    {
        [StringLength(200)]
        public virtual string Name { get; set; }

        [StringLength(50)]
        public virtual string SKU { get; set; }

        [StringLength(100)]
        public virtual string Category { get; set; }

        public virtual decimal UnitPrice { get; set; }

        public virtual decimal Cost { get; set; }

        public virtual int StockQuantity { get; set; }

        [StringLength(500)]
        public virtual string Description { get; set; }

        public virtual bool IsActive { get; set; } = true;

        public virtual DateTime CreatedDate { get; set; }
    }
}
