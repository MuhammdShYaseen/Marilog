using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Requests.PersonDTOs
{
    public class UpsertSeaServiceRequest
    {
        public int RankId { get; set; }
        public int ExperienceInMonths { get; set; }
        public decimal? VesselSizeInMT { get; set; }
    }
}
