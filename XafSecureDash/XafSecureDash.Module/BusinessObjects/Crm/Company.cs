using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using System.Collections.ObjectModel;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    [DefaultClassOptions]
    [NavigationItem("CRM")]
    [DefaultProperty(nameof(Name))]
    [ImageName("BO_Company")]
    public class Company : BaseObject
    {
        [StringLength(200)]
        public virtual string Name { get; set; }

        [StringLength(100)]
        public virtual string Industry { get; set; }

        [StringLength(300)]
        public virtual string Address { get; set; }

        [StringLength(100)]
        public virtual string City { get; set; }

        [StringLength(50)]
        public virtual string Country { get; set; }

        [StringLength(20)]
        public virtual string Phone { get; set; }

        [StringLength(200)]
        public virtual string Website { get; set; }

        public virtual int EmployeeCount { get; set; }

        public virtual decimal AnnualRevenue { get; set; }

        public virtual DateTime CreatedDate { get; set; }

        public virtual IList<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();
        public virtual IList<Order> Orders { get; set; } = new ObservableCollection<Order>();
        public virtual IList<Invoice> Invoices { get; set; } = new ObservableCollection<Invoice>();
        public virtual IList<ConsultancyProject> Projects { get; set; } = new ObservableCollection<ConsultancyProject>();
    }
}
