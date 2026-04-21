using AutoMapper;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Domain.Entities.SystemEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Mapping
{
    public class FinancialProfile : Profile
    {
        public FinancialProfile()
        {
            // DocumentItem
            CreateMap<DocumentItem, DocumentItemResponse>();

            // Document
            CreateMap<Document, DocumentResponse>()
                .ForMember(d => d.DocTypeName, o => o.MapFrom(s => s.DocType != null ? s.DocType.Name : string.Empty))
                .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.CompanyName : null))
                .ForMember(d => d.BuyerName, o => o.MapFrom(s => s.Buyer != null ? s.Buyer.CompanyName : null))
                .ForMember(d => d.VesselName, o => o.MapFrom(s => s.Vessel != null ? s.Vessel.VesselName : null))
                .ForMember(d => d.PortName, o => o.MapFrom(s => s.Port != null ? s.Port.PortName : null))
                .ForMember(d => d.CurrencyCode, o => o.MapFrom(s => s.Currency != null ? s.Currency.CurrencyCode : string.Empty))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

            // SwiftTransfer
            CreateMap<SwiftTransfer, SwiftTransferResponse>()
                .ForMember(d => d.CurrencyCode, o => o.MapFrom(s => s.Currency != null ? s.Currency.CurrencyCode : string.Empty))
                .ForMember(d => d.SenderCompanyName, o => o.MapFrom(s => s.SenderCompany != null ? s.SenderCompany.CompanyName : null))
                .ForMember(d => d.ReceiverCompanyName, o => o.MapFrom(s => s.ReceiverCompany != null ? s.ReceiverCompany.CompanyName : null));
        }
    }
}
