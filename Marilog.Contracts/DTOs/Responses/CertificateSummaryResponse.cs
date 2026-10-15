
namespace Marilog.Contracts.DTOs.Responses
{
    namespace Marilog.Contracts.DTOs.Responses
    {
        public sealed class CertificateSummaryResponse
        {
            public int Total { get; set; }
            public int Valid { get; set; }
            public int ExpiringSoon { get; set; }
            public int Expired { get; set; }
            public int NoExpiry { get; set; }
        }
    }
}
