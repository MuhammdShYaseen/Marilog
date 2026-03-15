using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities
{
    public class VoyageStop
    {
        public int StopID { get; private set; }
        public int VoyageID { get; private set; }
        public int PortID { get; private set; }
        public Port Port { get; private set; } = null!;
        public int StopOrder { get; private set; }
        public DateTime? ArrivalDate { get; private set; }
        public DateTime? DepartureDate { get; private set; }
        public string? PurposeOfCall { get; private set; }
        public string? Notes { get; private set; }

        private VoyageStop() { }
        internal static VoyageStop Create(int voyageId, int portId, int stopOrder,
            DateTime? arrivalDate = null, DateTime? departureDate = null,
            string? purposeOfCall = null, string? notes = null)
        {
            if (portId <= 0) throw new ArgumentException("Invalid PortID.");
            if (stopOrder <= 0) throw new ArgumentException("StopOrder must be positive.");

            return new VoyageStop
            {
                VoyageID = voyageId,
                PortID = portId,
                StopOrder = stopOrder,
                ArrivalDate = arrivalDate,
                DepartureDate = departureDate,
                PurposeOfCall = purposeOfCall,
                Notes = notes
            };
        }

        internal void Update(DateTime? arrivalDate, DateTime? departureDate,
            string? purposeOfCall, string? notes)
        {
            ArrivalDate = arrivalDate;
            DepartureDate = departureDate;
            PurposeOfCall = purposeOfCall;
            Notes = notes;
        }
    }
}
