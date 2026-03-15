using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    public enum Department { DECK, ENGINE, CATERING }

    public class Rank : Entity
    {
        public int RankID { get; private set; }
        public string RankCode { get; private set; } = null!;
        public string RankName { get; private set; } = null!;
        public Department Department { get; private set; }

        private Rank() { }
        public static Rank Create(string rankCode, string rankName, Department department)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rankCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(rankName);

            return new Rank
            {
                RankCode = rankCode.ToUpperInvariant(),
                RankName = rankName,
                Department = department
            };
        }

        public void Update(string rankCode, string rankName, Department department)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rankCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(rankName);

            RankCode = rankCode.ToUpperInvariant();
            RankName = rankName;
            Department = department;
            Touch();
        }
    }
}