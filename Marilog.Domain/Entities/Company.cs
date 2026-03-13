using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities
{
    public class Company : Entity
    {
        public int CompanyID { get; private set; }
        public string CompanyName { get; private set; } = null!;
        public string? Country { get; private set; }
        public string? ContactName { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? Address { get; private set; }

        private readonly List<Vessel> _vessels = new();
        public IReadOnlyCollection<Vessel> Vessels => _vessels.AsReadOnly();


        public static Company Create(string companyName, string? country = null,
            string? contactName = null, string? email = null,
            string? phone = null, string? address = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companyName);

            return new Company
            {
                CompanyName = companyName,
                Country = country,
                ContactName = contactName,
                Email = email,
                Phone = phone,
                Address = address
            };
        }

        public void Update(string companyName, string? country = null,
            string? contactName = null, string? email = null,
            string? phone = null, string? address = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companyName);

            CompanyName = companyName;
            Country = country;
            ContactName = contactName;
            Email = email;
            Phone = phone;
            Address = address;
            Touch();
        }
    }
}
