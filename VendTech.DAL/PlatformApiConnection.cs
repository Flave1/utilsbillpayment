//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VendTech.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class PlatformApiConnection
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PlatformApiConnection()
        {
            this.PlatformPacParams = new HashSet<PlatformPacParam>();
            this.Platforms = new HashSet<Platform>();
            this.Platforms1 = new HashSet<Platform>();
            this.PlatformTransactions = new HashSet<PlatformTransaction>();
        }
    
        public int Id { get; set; }
        public Nullable<int> PlatformApiId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public int PlatformId { get; set; }
    
        public virtual PlatformApi PlatformApi { get; set; }
        public virtual Platform Platform { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PlatformPacParam> PlatformPacParams { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Platform> Platforms { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Platform> Platforms1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PlatformTransaction> PlatformTransactions { get; set; }
    }
}