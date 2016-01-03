using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using InvoiceDataUpload.DataTarget;
using InvoiceDataUpload.DataSource;
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
            CimscoIDE_dbEntities _sourceContext = new CimscoIDE_dbEntities();
           // IEnumerable<DataSource.sInvoiceSummary> _all = _sourceContext.sInvoiceSummaries.Where(s => s.EnergyCharge.BD0004 > 0.0M);

            CimscoPortalEntities _targetContext = new CimscoPortalEntities();
           // IEnumerable<DataTarget.InvoiceSummary> _all2 = _targetContext.InvoiceSummaries.Where(s => s.EnergyCharge.BD0004 > 0.0M);

            DateTime _cutOffDate = DateTime.Now.AddDays(-90);

            var _newInvoices = (from _source in _sourceContext.sInvoiceSummaries
                                where _source.InvoiceDate > _cutOffDate
                                select _source.InvoiceSummaryId).ToList();

            var _invoicesAlreadyInTarget = (from _target in _targetContext.InvoiceSummaries
                                            where _newInvoices.Contains(_target.InvoiceId)
                                            select _target.InvoiceId).ToList();

            foreach (int _id in _newInvoices)
            {
                if (!_invoicesAlreadyInTarget.Contains(_id))
                {
                    try
                    {
                        //_repository.Groups.Where(s => s.Users.Any(w => w.Email == userId)).Project().To<SiteHierarchyViewModel>().FirstOrDefault();
                        InvoiceSummary _new = _sourceContext.sInvoiceSummaries.Where(s => s.InvoiceSummaryId == _id)
                                                .ProjectTo<InvoiceSummary>().FirstOrDefault();
                        _targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[InvoiceSummaries] ON");
                        _targetContext.InvoiceSummaries.Add(_new);
                        _targetContext.SaveChanges();
                        _targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[InvoiceSummaries] OFF");
                    }
                    catch (Exception ex)
                    { }
                }

            }
            log.WriteLine(message);
        }
    }
}
