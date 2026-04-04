using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Enumerations
{
    public class ContractType : Enumeration
    {
        public static readonly ContractType CharterParty = new(1, "CharterParty");
        public static readonly ContractType CrewEmployment = new(2, "CrewEmployment");
        public static readonly ContractType Supplier = new(3, "Supplier");
        public static readonly ContractType Agency = new(4, "Agency");

        private ContractType(int id, string name) : base(id, name) { }

        // للتحويل من DB أو API
        public static ContractType FromId(int id) => id switch
        {
            1 => CharterParty,
            2 => CrewEmployment,
            3 => Supplier,
            4 => Agency,
            _ => throw new ArgumentException($"Invalid ContractType id: {id}")
        };

        public static bool TryFromName(string name, out ContractType? result)
        {
            result = name?.ToLower() switch
            {
                "charterparty" => CharterParty,
                "crewemployment" => CrewEmployment,
                "supplier" => Supplier,
                "agency" => Agency,
                _ => null
            };

            return result is not null;
        }
    }
}
