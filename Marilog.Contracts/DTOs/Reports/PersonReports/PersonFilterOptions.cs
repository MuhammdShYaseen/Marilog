using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Reports.PersonReports
{
    public class PersonFilterOptions
    {
        // ── General ──
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public List<int>? NationalityIds { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        // ── Passport ──
        public bool? HasValidPassport { get; set; }
        public int? PassportExpiringWithinDays { get; set; }

        // ── Certificates ──
        public List<string>? CertificateNames { get; set; }
        public bool RequireAllCertificates { get; set; } = true;   // true = لازم يكون معه كلهم، false = أي واحدة
        public bool OnlyValidCertificates { get; set; } = true;    // تجاهل الشهادات المنتهية
        public List<PersonCertificateType>? CertificateTypes { get; set; }

        // ── Sea Services ──
        public int? MinSeaServicesCount { get; set; }
        public int? MaxSeaServicesCount { get; set; }
        public List<int>? RankIds { get; set; }
        public int? MinExperienceInRankMonths { get; set; }
        public decimal? MinVesselSizeInMT { get; set; }
        public int? MinTotalExperienceMonths { get; set; }

        // ── Bank ──
        public bool? HasBankAccount { get; set; }
    }
}