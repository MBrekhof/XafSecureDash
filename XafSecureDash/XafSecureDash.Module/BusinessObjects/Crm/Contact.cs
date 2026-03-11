using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace XafSecureDash.Module.BusinessObjects.Crm
{
    [DefaultClassOptions]
    [NavigationItem("CRM")]
    [DefaultProperty(nameof(FullName))]
    [ImageName("BO_Contact")]
    public class Contact : BaseObject
    {
        [StringLength(100)]
        public virtual string FirstName { get; set; }

        [StringLength(100)]
        public virtual string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [StringLength(200)]
        public virtual string Email { get; set; }

        [StringLength(20)]
        public virtual string Phone { get; set; }

        [StringLength(100)]
        public virtual string JobTitle { get; set; }

        [StringLength(50)]
        public virtual string Department { get; set; }

        public virtual Company Company { get; set; }

        public virtual DateTime CreatedDate { get; set; }

        public virtual bool IsActive { get; set; } = true;
    }
}
