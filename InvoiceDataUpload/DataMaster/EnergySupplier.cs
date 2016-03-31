//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InvoiceDataUpload.DataMaster
{
    using System;
    using System.Collections.Generic;
    
    public partial class EnergySupplier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EnergySupplier()
        {
            this.EnergyPoints = new HashSet<EnergyPoint>();
        }
    
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierLogo { get; set; }
        public Nullable<bool> SupplyElectricity { get; set; }
        public Nullable<bool> SupplyGas { get; set; }
        public Nullable<bool> SupplyOther { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Phone { get; set; }
        public string Contact { get; set; }
        public string eMail { get; set; }
        public decimal NetworkVariableNBDRate { get; set; }
        public decimal NetworkVariableBDRate { get; set; }
        public bool Enabled { get; set; }
        public Nullable<int> LogoHeight { get; set; }
        public Nullable<int> LogoWidth { get; set; }
        public Nullable<decimal> NetworkDemandRate { get; set; }
        public Nullable<decimal> NetworkCapacityRate { get; set; }
        public Nullable<decimal> NetworkFixedRate { get; set; }
        public Nullable<decimal> NetworkVariableRate { get; set; }
        public Nullable<decimal> OtherAdministrationCharge { get; set; }
        public Nullable<decimal> OtherCTMonthlyFee { get; set; }
        public Nullable<decimal> OtherDataReconciliationCharge { get; set; }
        public Nullable<decimal> OtherECCALevyCharge { get; set; }
        public Nullable<decimal> OtherMeterRentalCharge { get; set; }
        public decimal EnergyServicesRateNBD { get; set; }
        public decimal EnergyServicesRateBD { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EnergyPoint> EnergyPoints { get; set; }
    }
}