using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    /// <summary>
    /// Company branch / representative office.
    /// Used to identify where a cash payroll was collected.
    /// </summary>
    public class Office : Entity
    {
        public string OfficeName { get; private set; } = null!;
        public string City { get; private set; } = null!;
        public int CountryId { get; private set; }
        public Country Country { get; private set; } = null!;
        public string? Address { get; private set; }
        public string? Phone { get; private set; }
        public string? ContactName { get; private set; }

        private Office() { }

        public static Office Create(
            string officeName,
            string city,
            int countryId,
            string? address = null,
            string? phone = null,
            string? contactName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(officeName);
            ArgumentException.ThrowIfNullOrWhiteSpace(city);
            if (countryId <= 0) throw new ArgumentException("Invalid CountryId.");

            return new Office
            {
                OfficeName = officeName,
                City = city,
                CountryId = countryId,
                Address = address,
                Phone = phone,
                ContactName = contactName
            };
        }

        public void Update(string officeName, string city, int countryId,
            string? address = null, string? phone = null, string? contactName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(officeName);
            ArgumentException.ThrowIfNullOrWhiteSpace(city);
            if (countryId <= 0) throw new ArgumentException("Invalid CountryId.");

            OfficeName = officeName;
            City = city;
            CountryId = countryId;
            Address = address;
            Phone = phone;
            ContactName = contactName;
            Touch();
        }
    }
}