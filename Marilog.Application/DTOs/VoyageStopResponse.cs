using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class VoyageStopResponse
    {
        public int StopOrder { get; set; }
        public int PortId { get; set; }
        public string PortName { get; set; } = null!;
        public DateOnly ArrivalDate { get; set; }
        public DateOnly DepartureDate { get; set; }
        public string? PurposeOfCall { get; set; }
        public string? Notes { get; set; }
    }
}
