using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using InvoiceDataUpload.DataSource;
using InvoiceDataUpload.DataTarget;

namespace InvoiceDataUpload
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            InitializeAutomapper();
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        private static void InitializeAutomapper()
        {
            AutoMapper.Mapper.CreateMap<sNetworkCharge, NetworkCharge>();
            AutoMapper.Mapper.CreateMap<sOtherCharge, OtherCharge>();
            AutoMapper.Mapper.CreateMap<sEnergyCharge, EnergyCharge>();
            AutoMapper.Mapper.CreateMap<sInvoiceSummary, InvoiceSummary>()
                .ForMember(m => m.EnergyCharge, opt => opt.Ignore())// .MapFrom(i => i.EnergyCharge))
                .ForMember(m => m.NetworkCharge, opt => opt.Ignore()) //.MapFrom(i => i.NetworkCharge))
                .ForMember(m => m.OtherCharge, opt => opt.Ignore()) //.MapFrom(i => i.OtherCharge))
                ;
        }
    }
}
