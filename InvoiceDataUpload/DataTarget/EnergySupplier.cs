//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InvoiceDataUpload.DataTarget
{
    using System;
    using System.Collections.Generic;
    
    public partial class EnergySupplier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EnergySupplier()
        {
            this.InvoiceSummaries = new HashSet<InvoiceSummary>();
        }
    
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InvoiceSummary> InvoiceSummaries { get; set; }
    }
}