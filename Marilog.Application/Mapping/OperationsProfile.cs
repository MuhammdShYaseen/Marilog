using AutoMapper;
using Marilog.Application.DTOs.Responses;
using Marilog.Domain.Entities.SystemEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Mapping
{
    public class OperationsProfile : Profile
    {
        public OperationsProfile()
        {
            // CrewContract
            CreateMap<CrewContract, CrewContractResponse>()
                .ForMember(d => d.PersonFullName, o => o.MapFrom(s => s.Person!.FullName))
                .ForMember(d => d.VesselName, o => o.MapFrom(s => s.Vessel!.VesselName))
                .ForMember(d => d.RankName, o => o.MapFrom(s => s.Rank!.RankName))
                .ForMember(d => d.RankDepartment, o => o.MapFrom(s => s.Rank!.Department.ToString()))
                .ForMember(d => d.SignOnPortName, o => o.MapFrom(s => s.SignOnPortNav!.PortName))
                .ForMember(d => d.SignOffPortName, o => o.MapFrom(s => s.SignOffPortNav!.PortName))
                .ForMember(d => d.ContractDurationDays, o => o.Ignore()) // حسابه في الخدمة
                .ForMember(d => d.TotalWageEarned, o => o.Ignore());    // حسابه في الخدمة

            // VoyageStop
            CreateMap<VoyageStop, VoyageStopResponse>()
                .ForMember(d => d.PortName, o => o.MapFrom(s => s.Port!.PortName));

            // Voyage
            CreateMap<Voyage, VoyageResponse>()
                .ForMember(d => d.VesselName, o => o.MapFrom(s => s.Vessel!.VesselName))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.MasterFullName, o => o.MapFrom(s =>
                    s.MasterContract != null && s.MasterContract.Person != null
                        ? s.MasterContract.Person.FullName
                             : null))
                .ForMember(d => d.DeparturePortName, o => o.MapFrom(s => s.DeparturePort!.PortName))
                .ForMember(d => d.ArrivalPortName, o => o.MapFrom(s => s.ArrivalPort!.PortName))
                .ForMember(d => d.Stops, o => o.MapFrom(s => s.Stops));
        }
    }
}
