using InvoiceDataUpload.DataSource;
using InvoiceDataUpload.DataTarget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceDataUpload.Models;

namespace InvoiceDataUpload.App_Start
{
    public class AutoMapperConfig
    {
        public static void InitializeAutomapper()
        {
            AutoMapper.Mapper.CreateMap<sNetworkCharge, NetworkCharge>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<sOtherCharge, OtherCharge>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<sEnergyCharge, EnergyCharge>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<sEnergyPoint, EnergyPoint>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<sSite, Site>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<sGroup, Group>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<sCustomer, Customer>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<sInvoiceSummary, InvoiceSummary>().MaxDepth(1)
                .ForMember(m => m.EnergyPoint, opt => opt.Ignore())
                .ForMember(m => m.Site, opt => opt.Ignore())
                .ForMember(m => m.EnergySupplier, opt => opt.Ignore())
                .ForMember(m => m.InvoiceId, opt => opt.MapFrom(i => i.InvoiceSummaryId))
              ;

            AutoMapper.Mapper.CreateMap<Site, sSite>()
                .ForMember(m => m.Customer, opt => opt.Ignore())
                .ForMember(m => m.Group, opt => opt.Ignore())
                ;
            AutoMapper.Mapper.CreateMap<Customer, sCustomer>()
                ;

            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.Site, CustomerGroupSiteModel>()
                .ForMember(m => m.GroupName, opt => opt.MapFrom(s => s.Group.GroupName))
                .ForMember(m => m.GroupId, opt => opt.MapFrom(s => s.Group.GroupId))
                .ForMember(m => m.CustomerId, opt => opt.MapFrom(s => s.Customer.CustomerId))
                ;

            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.Site, Site>().MaxDepth(1)
                .ForMember(m => m.Customer, opt => opt.Ignore())
                .ForMember(m => m.Group, opt => opt.Ignore())
                .ForMember(m => m.InvoiceSummaries, opt => opt.Ignore())
                ;

            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.Site, sSite>().MaxDepth(1)
                .ForMember(m => m.Customer, opt => opt.Ignore())
                .ForMember(m => m.Group, opt => opt.Ignore())
                ;

            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.Group, Group>().MaxDepth(1)
                .ForMember(m => m.Contact, opt => opt.Ignore());
            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.Group, sGroup>().MaxDepth(1);

            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.Customer, Customer>().MaxDepth(1)
                .ForMember(m => m.Contact, opt => opt.Ignore());;

            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.Customer, sCustomer>().MaxDepth(1);

            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.EnergyPoint, EnergyPoint>().MaxDepth(1)
                .ForMember(m => m.EnergyPointNumber, opt => opt.MapFrom(s => s.ConnectionNumber));
            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.EnergyPoint, sEnergyPoint>().MaxDepth(1)
                .ForMember(m => m.EnergyPointNumber, opt => opt.MapFrom(s => s.ConnectionNumber));
            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.EnergySupplier, EnergySupplier>().MaxDepth(1);
            AutoMapper.Mapper.CreateMap<InvoiceDataUpload.DataMaster.EnergySupplier, sEnergySupplier>().MaxDepth(1);

            AutoMapper.Mapper.CreateMap<sEnergyPoint, sEnergyPoint>().MaxDepth(1)
                .ForMember(m => m.EnergyPointId, opt => opt.Ignore());

        }
    }
}
