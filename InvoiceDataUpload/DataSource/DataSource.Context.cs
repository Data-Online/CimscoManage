﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InvoiceDataUpload.DataSource
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CimscoIDE_dbEntities : DbContext
    {
        public CimscoIDE_dbEntities()
            : base("name=CimscoIDE_dbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<sCustomer> sCustomers { get; set; }
        public virtual DbSet<sEnergyCharge> sEnergyCharges { get; set; }
        public virtual DbSet<sEnergyPoint> sEnergyPoints { get; set; }
        public virtual DbSet<sEnergySupplier> sEnergySuppliers { get; set; }
        public virtual DbSet<sGroup> sGroups { get; set; }
        public virtual DbSet<sInvoiceSummary> sInvoiceSummaries { get; set; }
        public virtual DbSet<sNetworkCharge> sNetworkCharges { get; set; }
        public virtual DbSet<sOtherCharge> sOtherCharges { get; set; }
        public virtual DbSet<sSite> sSites { get; set; }
    }
}