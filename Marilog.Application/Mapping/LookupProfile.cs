using AutoMapper;
using Marilog.Application.DTOs.Responses;
using Marilog.Domain.Entities;

namespace Marilog.Application.Mapping
{
    public class LookupProfile : Profile
    {
        public LookupProfile()
        {
            // Country
            CreateMap<Country, CountryResponse>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Code, o => o.MapFrom(s => s.CountryCode))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.CountryName));

            // Currency
            CreateMap<Currency, CurrencyResponse>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.CurrencyCode))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.CurrencyName));

            // Port
            CreateMap<Port, PortResponse>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Code, o => o.MapFrom(s => s.PortCode))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.PortName))
                .ForMember(d => d.CountryName, o => o.MapFrom(s => s.Country!.CountryName));

            // Rank
            CreateMap<Rank, RankResponse>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Code, o => o.MapFrom(s => s.RankCode))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.RankName))
                .ForMember(d => d.Department, o => o.MapFrom(s => (int)s.Department));

            // Office
            CreateMap<Office, OfficeResponse>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.OfficeName))
                .ForMember(d => d.CountryName, o => o.MapFrom(s => s.Country!.CountryName));
        }
    }
}
