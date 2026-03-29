using AutoMapper;
using Marilog.Application.DTOs.Responses;
using Marilog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Mapping
{
    public class PayrollProfile : Profile
    {
        public PayrollProfile()
        {
            // CrewPayrollDisbursement → DisbursementResponse
            CreateMap<CrewPayrollDisbursement, DisbursementResponse>()
                .ForMember(d => d.Method, o => o.MapFrom(s => s.Method.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.VoyageNumber, o => o.MapFrom(s => s.Voyage != null ? s.Voyage.VoyageNumber : null))
                .ForMember(d => d.OfficeName, o => o.MapFrom(s => s.Office != null ? s.Office.OfficeName : null))
                .ForMember(d => d.SwiftReference, o => o.MapFrom(s => s.SwiftTransfer != null ? s.SwiftTransfer.SwiftReference : null));

            // CrewPayroll → CrewPayrollResponse
            CreateMap<CrewPayroll, CrewPayrollResponse>()
                .ForMember(d => d.PersonId, o => o.MapFrom(s => s.Contract != null ? s.Contract.PersonID : 0))
                .ForMember(d => d.PersonFullName, o => o.MapFrom(s => s.Contract != null && s.Contract.Person != null ? s.Contract.Person.FullName : string.Empty))
                .ForMember(d => d.VesselId, o => o.MapFrom(s => s.Contract != null ? s.Contract.VesselID : 0))
                .ForMember(d => d.VesselName, o => o.MapFrom(s => s.Contract != null && s.Contract.Vessel != null ? s.Contract.Vessel.VesselName : string.Empty))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Disbursements, o => o.MapFrom(s => s.Disbursements));
        }
    }
}
