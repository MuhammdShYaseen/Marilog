using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities
{
    public class Country : Entity
    {
        public int CountryID { get; private set; }
        public string CountryCode { get; private set; } = null!;
        public string CountryName { get; private set; } =null!;

        private Country() { }
        public static Country Create(string countryCode, string countryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(countryName);

            return new Country
            {
                CountryCode = countryCode.ToUpperInvariant(),
                CountryName = countryName
            };
        }

        public void Update(string countryCode, string countryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(countryName);

            CountryCode = countryCode.ToUpperInvariant();
            CountryName = countryName;
            Touch();
        }
    }
}
