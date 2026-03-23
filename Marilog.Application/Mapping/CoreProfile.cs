using AutoMapper;
using Marilog.Application.DTOs;
using Marilog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Mapping
{
    public class CoreProfile : Profile
    {
        public CoreProfile()
        {
            // Company
            CreateMap<Company, CompanyResponse>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.CompanyID))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.CompanyName));

            // Person
            CreateMap<Person, PersonResponse>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.PersonID))
                .ForMember(d => d.NationalityCountryName, o => o.MapFrom(s => s.NationalityCountry!.CountryName))
                .ForMember(d => d.IsPassportExpired, o => o.MapFrom(s => s.PassportExpiry.HasValue && s.PassportExpiry.Value < DateOnly.FromDateTime(DateTime.UtcNow)));

            // Vessel
            CreateMap<Vessel, VesselResponse>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.VesselID))
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company!.CompanyName))
                .ForMember(d => d.FlagCountryName, o => o.MapFrom(s => s.FlagCountry!.CountryName));
        }
    }
}
