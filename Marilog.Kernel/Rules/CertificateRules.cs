using Marilog.Kernel.Enums;

namespace Marilog.Kernel.Rules
{
    public static class CertificateRules
    {
        public const int ExpiringSoonDays = 90;

        public static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);

        public static CertificateExpiryStatus GetStatus(DateOnly? expiryDate, DateOnly today)
        {
            if (expiryDate is null) return CertificateExpiryStatus.NoExpiry;
            if (expiryDate.Value < today) return CertificateExpiryStatus.Expired;
            if (expiryDate.Value <= today.AddDays(ExpiringSoonDays)) return CertificateExpiryStatus.ExpiringSoon;
            return CertificateExpiryStatus.Valid;
        }
    }
}