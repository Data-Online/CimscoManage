using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using InvoiceDataUpload.DataTarget;
using InvoiceDataUpload.DataSource;
using InvoiceDataUpload.DataMaster;
using InvoiceDataUpload.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace InvoiceDataUpload
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("syncdata")] string message, TextWriter log)
        {

            // Source of truth MasterDatabase (CimscoNZ) for tables:
            //      EnergyPoints
            //      EnergySuppliers
            //      Groups
            //      Sites
            //      Customers
            // This data replicated to CimscoIDE and CimscoPortal

            // Data entry source (CimscoIDE) --> Data target (CimscoPortal)
            // sInvoiceSummaries
            //      sEnergyCharges
            //      sNetworkCharges
            //      sOtherCharges

            CopyMasterData(log);
            SyncInvoincesFromSource(message, log);
            log.WriteLine(message);
        }

        private static void SyncInvoincesFromSource(string message, TextWriter log)
        {
            int _span;
            int _invoiceId;
            log.WriteLine("Start of invoice data sync\n");
            ExtractParametersFromMessage(message, out _span, out _invoiceId);
            try
            {
                log.Write(CopyNewInvoicesToTarget(_span, _invoiceId));
            }
            catch (Exception ex)
            {
                log.Write(ex);
            }
            CalculatePercentageChange();
        }

        private static void CopyMasterData(TextWriter log)
        {
            // Master data ==> Portal (Add/Delete)
            //             ==> IDE    (Add/Delete)
            log.WriteLine("Start of master data sync\n");
            try
            {
                SyncMasterData(log);
            }
            catch (Exception ex) { log.Write(ex); }
            try
            {
                RemoveDeletedMasterData(log);
            }
            catch (Exception ex) { log.Write(ex); }

        }

        private static void RemoveDeletedMasterData(TextWriter log)
        {
            using (CimscoNZEntities _masterDataContext = new CimscoNZEntities())
            {
                RemoveDeletedSitesFromIDE_and_Portal(_masterDataContext);
                RemoveDeletedGroupsFromIDE_and_Portal(_masterDataContext);
                RemoveDeletedCustomersFromIDE_and_Portal(_masterDataContext);
                RemoveDeletedEnergyPointsFromIDE_and_Portal(_masterDataContext);
                RemoveDeletedEnergySuppliersFromIDE_and_Portal(_masterDataContext);
            }
        }

        private static void SyncMasterData(TextWriter log)
        {
            using (CimscoNZEntities _masterDataContext = new CimscoNZEntities())
            {
                SyncSitesToIDE_and_Portal(_masterDataContext);
                SyncGroupsToIDE_and_Portal(_masterDataContext);
                SyncCustomersToIDE_and_Portal(_masterDataContext);
                SyncEnergyPointsToIDE_and_Portal(_masterDataContext);
                log.Write(SyncEnergySuppliersToIDE_and_Portal(_masterDataContext));

                //List<CustomerGroupSiteModel> _siteCustomerGroupHierachy = new List<CustomerGroupSiteModel>();
                //_siteCustomerGroupHierachy = _masterDataContext.Sites.ProjectTo<CustomerGroupSiteModel>().ToList();
            }
        }

        #region Copy / Delete functions for data sync

        #region Energy Suppliers
        private static string SyncEnergySuppliersToIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            int _updates = 0;
            int _adds = 0;
            string _dataType = "Energy Suppliers";
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                //var _sourceSiteIdList = _sourceSites.Select(s => s.SiteId).ToList();
                using (var _transaction1 = _ideDataContext.Database.BeginTransaction())
                using (var _transaction2 = _portalDataContext.Database.BeginTransaction())
                {
                    var _alreadyInTarget1 = (from _targetIds in _ideDataContext.sEnergySuppliers
                                             select _targetIds.SupplierId).ToList();
                    var _alreadyInTarget2 = (from _targetIds in _portalDataContext.EnergySuppliers
                                             select _targetIds.SupplierId).ToList();

                    var _sourceMasterData = (from _sourceRecords in _masterDataContext.EnergySuppliers
                                             select _sourceRecords.SupplierId).ToList();


                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergySuppliers] ON");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergySuppliers] ON");
                    foreach (int _id in _sourceMasterData)
                    {
                        sEnergySupplier _newRecord1 = _masterDataContext.EnergySuppliers.Where(s => s.SupplierId == _id).ProjectTo<sEnergySupplier>().FirstOrDefault();

                        if (_alreadyInTarget1.Contains(_id))
                        {
                            var _targetRecord = _ideDataContext.sEnergySuppliers.Where(s => s.SupplierId == _id).FirstOrDefault();
                            _ideDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord1);
                            _ideDataContext.SaveChanges();
                            _updates++;
                        }
                        else
                        {
                            _ideDataContext.sEnergySuppliers.Add(_newRecord1);
                            _ideDataContext.SaveChanges();
                            _adds++;
                        }

                        InvoiceDataUpload.DataTarget.EnergySupplier _newRecord2 = _masterDataContext.EnergySuppliers.Where(s => s.SupplierId == _id).ProjectTo<InvoiceDataUpload.DataTarget.EnergySupplier>().FirstOrDefault();
                        if (_alreadyInTarget2.Contains(_id))
                        {
                            var _targetRecord = _portalDataContext.EnergySuppliers.Where(s => s.SupplierId == _id).FirstOrDefault();
                            _portalDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord2);
                            _portalDataContext.SaveChanges();
                            _updates++;
                        }
                        else
                        {
                            _portalDataContext.EnergySuppliers.Add(_newRecord2);
                            _portalDataContext.SaveChanges();
                            _adds++;
                        }

                    }
                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergySuppliers] OFF");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergySuppliers] OFF");

                    _transaction1.Commit();
                    _transaction2.Commit();
                }
            }
            return String.Format("{2}: Additions : {0}, Updates : {1}", _adds.ToString(), _updates.ToString(), _dataType);
        }
        private static void RemoveDeletedEnergySuppliersFromIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                using (var transaction1 = _portalDataContext.Database.BeginTransaction())
                using (var transaction2 = _ideDataContext.Database.BeginTransaction())
                {
                    var _recordsInSource = (from _targetIds in _masterDataContext.EnergySuppliers
                                            select _targetIds.SupplierId).ToList();

                    var _deleteFromTarget1 = (from _sourceRecords in _portalDataContext.EnergySuppliers
                                              where !_recordsInSource.Contains(_sourceRecords.SupplierId)
                                              select _sourceRecords.SupplierId).ToList();

                    var _deleteFromTarget2 = (from _sourceRecords in _ideDataContext.sEnergySuppliers
                                              where !_recordsInSource.Contains(_sourceRecords.SupplierId)
                                              select _sourceRecords.SupplierId).ToList();
                    if (_deleteFromTarget1.Count != _deleteFromTarget2.Count)
                    {
                        // Log event, as these should match
                    }

                    if (_deleteFromTarget1.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget1)
                        {
                            var _removeRecord = new InvoiceDataUpload.DataTarget.EnergySupplier { SupplierId = _id };
                            _portalDataContext.EnergySuppliers.Attach(_removeRecord);
                            _portalDataContext.EnergySuppliers.Remove(_removeRecord);
                        }

                        _portalDataContext.SaveChanges();
                    }
                    if (_deleteFromTarget2.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget2)
                        {
                            var _removeRecord = new sEnergySupplier { SupplierId = _id };
                            _ideDataContext.sEnergySuppliers.Attach(_removeRecord);
                            _ideDataContext.sEnergySuppliers.Remove(_removeRecord);
                        }
                        _ideDataContext.SaveChanges();
                    }
                    transaction1.Commit();
                    transaction2.Commit();
                }
            }
        }
        #endregion Energy suppliers

        #region Energy Points
        private static void SyncEnergyPointsToIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                //var _sourceSiteIdList = _sourceSites.Select(s => s.SiteId).ToList();
                using (var _transaction1 = _ideDataContext.Database.BeginTransaction())
                using (var _transaction2 = _portalDataContext.Database.BeginTransaction())
                {
                    var _alreadyInTarget1 = (from _targetIds in _ideDataContext.sEnergyPoints
                                             select _targetIds.EnergyPointId).ToList();
                    var _alreadyInTarget2 = (from _targetIds in _portalDataContext.EnergyPoints
                                             select _targetIds.EnergyPointId).ToList();

                    var _sourceMasterData = (from _sourceRecords in _masterDataContext.EnergyPoints
                                             select _sourceRecords.EnergyPointId).ToList();


                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergyPoints] ON");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergyPoints] ON");
                    foreach (int _id in _sourceMasterData)
                    {
                        sEnergyPoint _newRecord1 = _masterDataContext.EnergyPoints.Where(s => s.EnergyPointId == _id).ProjectTo<sEnergyPoint>().FirstOrDefault();

                        if (_alreadyInTarget1.Contains(_id))
                        {
                            var _targetRecord = _ideDataContext.sEnergyPoints.Where(s => s.EnergyPointId == _id).FirstOrDefault();
                            _ideDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }
                        else
                        {
                            _ideDataContext.sEnergyPoints.Add(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }

                        InvoiceDataUpload.DataTarget.EnergyPoint _newRecord2 = _masterDataContext.EnergyPoints.Where(s => s.EnergyPointId == _id).ProjectTo<InvoiceDataUpload.DataTarget.EnergyPoint>().FirstOrDefault();
                        if (_alreadyInTarget2.Contains(_id))
                        {
                            var _targetRecord = _portalDataContext.EnergyPoints.Where(s => s.EnergyPointId == _id).FirstOrDefault();
                            _portalDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }
                        else
                        {
                            _portalDataContext.EnergyPoints.Add(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }

                    }
                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergyPoints] OFF");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[EnergyPoints] OFF");

                    _transaction1.Commit();
                    _transaction2.Commit();
                }
            }
        }


        private static void RemoveDeletedEnergyPointsFromIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                using (var transaction1 = _portalDataContext.Database.BeginTransaction())
                using (var transaction2 = _ideDataContext.Database.BeginTransaction())
                {
                    var _recordsInSource = (from _targetIds in _masterDataContext.EnergyPoints
                                            select _targetIds.EnergyPointId).ToList();

                    var _deleteFromTarget1 = (from _sourceRecords in _portalDataContext.EnergyPoints
                                              where !_recordsInSource.Contains(_sourceRecords.EnergyPointId)
                                              select _sourceRecords.EnergyPointId).ToList();

                    var _deleteFromTarget2 = (from _sourceRecords in _ideDataContext.sEnergyPoints
                                              where !_recordsInSource.Contains(_sourceRecords.EnergyPointId)
                                              select _sourceRecords.EnergyPointId).ToList();
                    if (_deleteFromTarget1.Count != _deleteFromTarget2.Count)
                    {
                        // Log event, as these should match
                    }

                    if (_deleteFromTarget1.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget1)
                        {
                            var _removeRecord = new InvoiceDataUpload.DataTarget.EnergyPoint { EnergyPointId = _id };
                            _portalDataContext.EnergyPoints.Attach(_removeRecord);
                            _portalDataContext.EnergyPoints.Remove(_removeRecord);
                        }

                        _portalDataContext.SaveChanges();
                    }
                    if (_deleteFromTarget2.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget2)
                        {
                            var _removeRecord = new sEnergyPoint { EnergyPointId = _id };
                            _ideDataContext.sEnergyPoints.Attach(_removeRecord);
                            _ideDataContext.sEnergyPoints.Remove(_removeRecord);
                        }
                        _ideDataContext.SaveChanges();
                    }
                    transaction1.Commit();
                    transaction2.Commit();
                }
            }
        }
        #endregion Energy points

        #region Customers
        private static void SyncCustomersToIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                //var _sourceSiteIdList = _sourceSites.Select(s => s.SiteId).ToList();
                using (var _transaction1 = _ideDataContext.Database.BeginTransaction())
                using (var _transaction2 = _portalDataContext.Database.BeginTransaction())
                {
                    var _alreadyInTarget1 = (from _targetIds in _ideDataContext.sCustomers
                                             select _targetIds.CustomerId).ToList();
                    var _alreadyInTarget2 = (from _targetIds in _portalDataContext.Customers
                                             select _targetIds.CustomerId).ToList();

                    var _sourceMasterData = (from _sourceRecords in _masterDataContext.Customers
                                             select _sourceRecords.CustomerId).ToList();


                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] ON");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] ON");
                    foreach (int _id in _sourceMasterData)
                    {
                        sCustomer _newRecord1 = _masterDataContext.Customers.Where(s => s.CustomerId == _id).ProjectTo<sCustomer>().FirstOrDefault();

                        if (_alreadyInTarget1.Contains(_id))
                        {
                            var _targetRecord = _ideDataContext.sCustomers.Where(s => s.CustomerId == _id).FirstOrDefault();
                            _ideDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }
                        else
                        {
                            _ideDataContext.sCustomers.Add(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }

                        InvoiceDataUpload.DataTarget.Customer _newRecord2 = _masterDataContext.Customers.Where(s => s.CustomerId == _id).ProjectTo<InvoiceDataUpload.DataTarget.Customer>().FirstOrDefault();
                        if (_alreadyInTarget2.Contains(_id))
                        {
                            var _targetRecord = _portalDataContext.Customers.Where(s => s.CustomerId == _id).FirstOrDefault();
                            _portalDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }
                        else
                        {
                            _portalDataContext.Customers.Add(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }

                    }
                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] OFF");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] OFF");

                    _transaction1.Commit();
                    _transaction2.Commit();
                }
            }
        }
        private static void RemoveDeletedCustomersFromIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                using (var transaction1 = _portalDataContext.Database.BeginTransaction())
                using (var transaction2 = _ideDataContext.Database.BeginTransaction())
                {
                    var _recordsInSource = (from _targetIds in _masterDataContext.Customers
                                            select _targetIds.CustomerId).ToList();

                    var _deleteFromTarget1 = (from _sourceRecords in _portalDataContext.Customers
                                              where !_recordsInSource.Contains(_sourceRecords.CustomerId)
                                              select _sourceRecords.CustomerId).ToList();

                    var _deleteFromTarget2 = (from _sourceRecords in _ideDataContext.sCustomers
                                              where !_recordsInSource.Contains(_sourceRecords.CustomerId)
                                              select _sourceRecords.CustomerId).ToList();
                    if (_deleteFromTarget1.Count != _deleteFromTarget2.Count)
                    {
                        // Log event, as these should match
                    }

                    if (_deleteFromTarget1.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget1)
                        {
                            var _removeRecord = new InvoiceDataUpload.DataTarget.Customer { CustomerId = _id };
                            _portalDataContext.Customers.Attach(_removeRecord);
                            _portalDataContext.Customers.Remove(_removeRecord);
                        }

                        _portalDataContext.SaveChanges();
                    }
                    if (_deleteFromTarget2.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget2)
                        {
                            var _removeRecord = new sCustomer { CustomerId = _id };
                            _ideDataContext.sCustomers.Attach(_removeRecord);
                            _ideDataContext.sCustomers.Remove(_removeRecord);
                        }
                        _ideDataContext.SaveChanges();
                    }
                    transaction1.Commit();
                    transaction2.Commit();
                }
            }
        }
        #endregion Customers

        #region Groups
        private static void SyncGroupsToIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                //var _sourceSiteIdList = _sourceSites.Select(s => s.SiteId).ToList();
                using (var _transaction1 = _ideDataContext.Database.BeginTransaction())
                using (var _transaction2 = _portalDataContext.Database.BeginTransaction())
                {
                    var _alreadyInTarget1 = (from _targetIds in _ideDataContext.sGroups
                                             select _targetIds.GroupId).ToList();
                    var _alreadyInTarget2 = (from _targetIds in _portalDataContext.Groups
                                             select _targetIds.GroupId).ToList();

                    var _sourceMasterData = (from _sourceRecords in _masterDataContext.Groups
                                             select _sourceRecords.GroupId).ToList();


                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Groups] ON");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Groups] ON");
                    foreach (int _id in _sourceMasterData)
                    {
                        sGroup _newRecord1 = _masterDataContext.Groups.Where(s => s.GroupId == _id).ProjectTo<sGroup>().FirstOrDefault();

                        if (_alreadyInTarget1.Contains(_id))
                        {
                            var _targetRecord = _ideDataContext.sGroups.Where(s => s.GroupId == _id).FirstOrDefault();
                            _ideDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }
                        else
                        {
                            _ideDataContext.sGroups.Add(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }

                        InvoiceDataUpload.DataTarget.Group _newRecord2 = _masterDataContext.Groups.Where(s => s.GroupId == _id).ProjectTo<InvoiceDataUpload.DataTarget.Group>().FirstOrDefault();
                        if (_alreadyInTarget2.Contains(_id))
                        {
                            var _targetRecord = _portalDataContext.Groups.Where(s => s.GroupId == _id).FirstOrDefault();
                            _portalDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }
                        else
                        {
                            _portalDataContext.Groups.Add(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }

                    }
                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Groups] OFF");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Groups] OFF");

                    _transaction1.Commit();
                    _transaction2.Commit();
                }
            }
        }
        private static void RemoveDeletedGroupsFromIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                using (var transaction1 = _portalDataContext.Database.BeginTransaction())
                using (var transaction2 = _ideDataContext.Database.BeginTransaction())
                {
                    var _recordsInSource = (from _targetIds in _masterDataContext.Groups
                                            select _targetIds.GroupId).ToList();

                    var _deleteFromTarget1 = (from _sourceRecords in _portalDataContext.Groups
                                              where !_recordsInSource.Contains(_sourceRecords.GroupId)
                                              select _sourceRecords.GroupId).ToList();

                    var _deleteFromTarget2 = (from _sourceRecords in _ideDataContext.sGroups
                                              where !_recordsInSource.Contains(_sourceRecords.GroupId)
                                              select _sourceRecords.GroupId).ToList();
                    if (_deleteFromTarget1.Count != _deleteFromTarget2.Count)
                    {
                        // Log event, as these should match
                    }

                    if (_deleteFromTarget1.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget1)
                        {
                            var _removeRecord = new InvoiceDataUpload.DataTarget.Group { GroupId = _id };
                            _portalDataContext.Groups.Attach(_removeRecord);
                            _portalDataContext.Groups.Remove(_removeRecord);
                        }

                        _portalDataContext.SaveChanges();
                    }
                    if (_deleteFromTarget2.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget2)
                        {
                            var _removeRecord = new sGroup { GroupId = _id };
                            _ideDataContext.sGroups.Attach(_removeRecord);
                            _ideDataContext.sGroups.Remove(_removeRecord);
                        }
                        _ideDataContext.SaveChanges();
                    }
                    transaction1.Commit();
                    transaction2.Commit();
                }
            }
        }
        #endregion Groups

        #region Sites
        private static void RemoveDeletedSitesFromIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                using (var transaction1 = _portalDataContext.Database.BeginTransaction())
                using (var transaction2 = _ideDataContext.Database.BeginTransaction())
                {
                    var _recordsInSource = (from _targetIds in _masterDataContext.Sites
                                            select _targetIds.SiteId).ToList();

                    var _deleteFromTarget1 = (from _sourceRecords in _portalDataContext.Sites
                                              where !_recordsInSource.Contains(_sourceRecords.SiteId)
                                              select _sourceRecords.SiteId).ToList();

                    var _deleteFromTarget2 = (from _sourceRecords in _ideDataContext.sSites
                                              where !_recordsInSource.Contains(_sourceRecords.SiteId)
                                              select _sourceRecords.SiteId).ToList();
                    if (_deleteFromTarget1.Count != _deleteFromTarget2.Count)
                    {
                        // Log event, as these should match
                    }

                    if (_deleteFromTarget1.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget1)
                        {
                            var _removeRecord = new InvoiceDataUpload.DataTarget.Site { SiteId = _id };
                            _portalDataContext.Sites.Attach(_removeRecord);
                            _portalDataContext.Sites.Remove(_removeRecord);
                        }

                        _portalDataContext.SaveChanges();
                    }
                    if (_deleteFromTarget2.Count > 0)
                    {
                        foreach (int _id in _deleteFromTarget2)
                        {
                            var _removeRecord = new sSite { SiteId = _id };
                            _ideDataContext.sSites.Attach(_removeRecord);
                            _ideDataContext.sSites.Remove(_removeRecord);
                        }
                        _ideDataContext.SaveChanges();
                    }
                    transaction1.Commit();
                    transaction2.Commit();
                }
            }
        }
        private static void SyncSitesToIDE_and_Portal(CimscoNZEntities _masterDataContext)
        {
            using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
            using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
            {
                //var _sourceSiteIdList = _sourceSites.Select(s => s.SiteId).ToList();
                using (var _transaction1 = _ideDataContext.Database.BeginTransaction())
                using (var _transaction2 = _portalDataContext.Database.BeginTransaction())
                {
                    var _alreadyInTarget1 = (from _targetIds in _ideDataContext.sSites
                                             select _targetIds.SiteId).ToList();
                    var _alreadyInTarget2 = (from _targetIds in _portalDataContext.Sites
                                             select _targetIds.SiteId).ToList();

                    var _sourceMasterData = (from _sourceRecords in _masterDataContext.Sites
                                             select _sourceRecords.SiteId).ToList();


                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Sites] ON");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Sites] ON");
                    foreach (int _id in _sourceMasterData)
                    {
                        sSite _newRecord1 = _masterDataContext.Sites.Where(s => s.SiteId == _id).ProjectTo<sSite>().FirstOrDefault();

                        if (_alreadyInTarget1.Contains(_id))
                        {
                            var _targetRecord = _ideDataContext.sSites.Where(s => s.SiteId == _id).FirstOrDefault();
                            _ideDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }
                        else
                        {
                            _ideDataContext.sSites.Add(_newRecord1);
                            _ideDataContext.SaveChanges();
                        }

                        InvoiceDataUpload.DataTarget.Site _newRecord2 = _masterDataContext.Sites.Where(s => s.SiteId == _id).ProjectTo<InvoiceDataUpload.DataTarget.Site>().FirstOrDefault();
                        if (_alreadyInTarget2.Contains(_id))
                        {
                            var _targetRecord = _portalDataContext.Sites.Where(s => s.SiteId == _id).FirstOrDefault();
                            _portalDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }
                        else
                        {
                            _portalDataContext.Sites.Add(_newRecord2);
                            _portalDataContext.SaveChanges();
                        }

                    }
                    _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Sites] OFF");
                    _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Sites] OFF");

                    _transaction1.Commit();
                    _transaction2.Commit();
                }
            }
        }
        #endregion Sites

        //#region Cities
        //private static void SyncCitiesToIDE_and_Portal(CimscoNZEntities _masterDataContext)
        //{
        //    using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
        //    using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
        //    {
        //        //var _sourceSiteIdList = _sourceSites.Select(s => s.SiteId).ToList();
        //        using (var _transaction1 = _ideDataContext.Database.BeginTransaction())
        //        using (var _transaction2 = _portalDataContext.Database.BeginTransaction())
        //        {
        //            var _alreadyInTarget1 = (from _targetIds in _ideDataContext.sCustomers
        //                                     select _targetIds.CustomerId).ToList();
        //            var _alreadyInTarget2 = (from _targetIds in _portalDataContext.Customers
        //                                     select _targetIds.CustomerId).ToList();

        //            var _sourceMasterData = (from _sourceRecords in _masterDataContext.Customers
        //                                     select _sourceRecords.CustomerId).ToList();


        //            _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] ON");
        //            _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] ON");
        //            foreach (int _id in _sourceMasterData)
        //            {
        //                sCustomer _newRecord1 = _masterDataContext.Customers.Where(s => s.CustomerId == _id).ProjectTo<sCustomer>().FirstOrDefault();

        //                if (_alreadyInTarget1.Contains(_id))
        //                {
        //                    var _targetRecord = _ideDataContext.sCustomers.Where(s => s.CustomerId == _id).FirstOrDefault();
        //                    _ideDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord1);
        //                    _ideDataContext.SaveChanges();
        //                }
        //                else
        //                {
        //                    _ideDataContext.sCustomers.Add(_newRecord1);
        //                    _ideDataContext.SaveChanges();
        //                }

        //                InvoiceDataUpload.DataTarget.Customer _newRecord2 = _masterDataContext.Customers.Where(s => s.CustomerId == _id).ProjectTo<InvoiceDataUpload.DataTarget.Customer>().FirstOrDefault();
        //                if (_alreadyInTarget2.Contains(_id))
        //                {
        //                    var _targetRecord = _portalDataContext.Customers.Where(s => s.CustomerId == _id).FirstOrDefault();
        //                    _portalDataContext.Entry(_targetRecord).CurrentValues.SetValues(_newRecord2);
        //                    _portalDataContext.SaveChanges();
        //                }
        //                else
        //                {
        //                    _portalDataContext.Customers.Add(_newRecord2);
        //                    _portalDataContext.SaveChanges();
        //                }

        //            }
        //            _ideDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] OFF");
        //            _portalDataContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Customers] OFF");

        //            _transaction1.Commit();
        //            _transaction2.Commit();
        //        }
        //    }
        //}
        //private static void RemoveDeletedCustomersFromIDE_and_Portal(CimscoNZEntities _masterDataContext)
        //{
        //    using (CimscoIDE_dbEntities _ideDataContext = new CimscoIDE_dbEntities())
        //    using (CimscoPortalEntities _portalDataContext = new CimscoPortalEntities())
        //    {
        //        using (var transaction1 = _portalDataContext.Database.BeginTransaction())
        //        using (var transaction2 = _ideDataContext.Database.BeginTransaction())
        //        {
        //            var _recordsInSource = (from _targetIds in _masterDataContext.Customers
        //                                    select _targetIds.CustomerId).ToList();

        //            var _deleteFromTarget1 = (from _sourceRecords in _portalDataContext.Customers
        //                                      where !_recordsInSource.Contains(_sourceRecords.CustomerId)
        //                                      select _sourceRecords.CustomerId).ToList();

        //            var _deleteFromTarget2 = (from _sourceRecords in _ideDataContext.sCustomers
        //                                      where !_recordsInSource.Contains(_sourceRecords.CustomerId)
        //                                      select _sourceRecords.CustomerId).ToList();
        //            if (_deleteFromTarget1.Count != _deleteFromTarget2.Count)
        //            {
        //                // Log event, as these should match
        //            }

        //            if (_deleteFromTarget1.Count > 0)
        //            {
        //                foreach (int _id in _deleteFromTarget1)
        //                {
        //                    var _removeRecord = new InvoiceDataUpload.DataTarget.Customer { CustomerId = _id };
        //                    _portalDataContext.Customers.Attach(_removeRecord);
        //                    _portalDataContext.Customers.Remove(_removeRecord);
        //                }

        //                _portalDataContext.SaveChanges();
        //            }
        //            if (_deleteFromTarget2.Count > 0)
        //            {
        //                foreach (int _id in _deleteFromTarget2)
        //                {
        //                    var _removeRecord = new sCustomer { CustomerId = _id };
        //                    _ideDataContext.sCustomers.Attach(_removeRecord);
        //                    _ideDataContext.sCustomers.Remove(_removeRecord);
        //                }
        //                _ideDataContext.SaveChanges();
        //            }
        //            transaction1.Commit();
        //            transaction2.Commit();
        //        }
        //    }
        //}
        //#endregion Cities

        #endregion

        private static void CalculatePercentageChange()
        {
            using (CimscoPortalEntities _targetContext = new CimscoPortalEntities())
            {
                //IFormatProvider culture = new System.Globalization.CultureInfo("en-EN", true);
                decimal _lastTotal;
                decimal _currentTotal;
                decimal _percentChange;
                DateTime _lastInvoiceDate; // = new DateTime(1970, 1, 1); // Convert.ToDateTime("01/01/2000"); ; 
                DateTime _invoiceMonth;
                foreach (var _energyPointId in _targetContext.EnergyPoints.Select(i => i.EnergyPointId))
                {
                    _lastTotal = 0.00M;
                    _currentTotal = 0.00M;
                    _lastInvoiceDate = new DateTime(1970, 1, 1);
                    foreach (var _invoice in _targetContext.InvoiceSummaries.Where(i => i.EnergyPointId == _energyPointId).OrderBy(i => i.InvoiceDate))
                    {
                        _invoiceMonth = new DateTime(_invoice.InvoiceDate.Year, _invoice.InvoiceDate.Month, 1);
                        if (_invoiceMonth.AddMonths(-1) == _lastInvoiceDate)
                        {
                            _currentTotal = _invoice.InvoiceTotal;
                            _percentChange = 0.0M;
                            if (_lastTotal > 0.00M)
                            {
                                _percentChange = ((_currentTotal - _lastTotal) / _lastTotal) * 100;
                            }
                        }
                        else
                        {
                            _percentChange = -999.0M;
                        }
                        _invoice.PercentageChange = _percentChange;
                        _lastTotal = _currentTotal;
                        _lastInvoiceDate = _invoiceMonth;
                    }
                }
                _targetContext.SaveChanges();
            }
        }


        #region Private functions

        private static void ExtractParametersFromMessage(string message, out int _span, out int _invoiceId)
        {
            _span = 90;
            _invoiceId = 0;

            string[] _scopeCmds = message.Replace(" ", string.Empty).Split(';');
            foreach (string _scopePair in _scopeCmds)
            {
                string[] _cmdParts = _scopePair.Split(':');
                if (_cmdParts[0] == "span" && _cmdParts.Length > 1)
                { Int32.TryParse(_cmdParts[1], out _span); }
                else if (_cmdParts[0] == "id" && _cmdParts.Length > 1)
                { Int32.TryParse(_cmdParts[1], out _invoiceId); }
            }
        }

        private static string CopyNewInvoicesToTarget(int span, int invoiceId)
        {
            int _adds = 0;
            // IEnumerable<DataSource.sInvoiceSummary> _all = _sourceContext.sInvoiceSummaries.Where(s => s.EnergyCharge.BD0004 > 0.0M);

            CimscoPortalEntities _targetContext = new CimscoPortalEntities();
            CimscoIDE_dbEntities _sourceContext = new CimscoIDE_dbEntities();
            // IEnumerable<DataTarget.InvoiceSummary> _all2 = _targetContext.InvoiceSummaries.Where(s => s.EnergyCharge.BD0004 > 0.0M);

            DateTime _cutOffDate;
            switch (span)
            {
                case 0: _cutOffDate = new DateTime(2000, 1, 1);
                    break;
                default: _cutOffDate = DateTime.Now.AddDays(span * -1);
                    break;
            };

            var _newInvoices = (from _source in _sourceContext.sInvoiceSummaries
                                where _source.InvoiceDate > _cutOffDate & _source.CheckedOk == true
                                select _source.InvoiceSummaryId).ToList();

            var _invoicesAlreadyInTarget = (from _target in _targetContext.InvoiceSummaries
                                            where _newInvoices.Contains(_target.InvoiceId)
                                            select _target.InvoiceId).ToList();

            foreach (int _id in _newInvoices)
            {
                if (!_invoicesAlreadyInTarget.Contains(_id))
                {
                        InvoiceSummary _new = _sourceContext.sInvoiceSummaries.Where(s => s.InvoiceSummaryId == _id)
                                                .ProjectTo<InvoiceSummary>().FirstOrDefault();
                        _targetContext.InvoiceSummaries.Add(_new);
                        _targetContext.SaveChanges();
                        _adds++;
                }
            }
            return String.Format("Invoices copied : {0}\n", _adds.ToString());
        }


        #endregion Private functions
    }
}
