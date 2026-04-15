using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Company : Entity
    {
        public string CompanyName { get; private set; } = null!;
        public int? CountryId { get; private set; }
        public Country? Country { get; private set; }
        public string? ContactName { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? Address { get; private set; }
        public string? RegistrationNumber { get; private set; }

        private readonly List<Vessel> _vessels = new();
        public IReadOnlyCollection<Vessel> Vessels => _vessels.AsReadOnly();

        private Company() { }
        public static Company Create(string? registrationNumber, string companyName, int? countryId,
            string? contactName = null, string? email = null,
            string? phone = null,  string? address = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companyName);

            return new Company
            {
                CompanyName = companyName,
                CountryId = countryId,
                ContactName = contactName,
                Email = email,
                Phone = phone,
                Address = address
            };
        }

        public void Update(string companyName, int? countryId,
            string? contactName = null, string? email = null,
            string? phone = null, string? address = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companyName);

            CompanyName = companyName;
            CountryId = countryId;
            ContactName = contactName;
            Email = email;
            Phone = phone;
            Address = address;
            Touch();
        }
    }
}
