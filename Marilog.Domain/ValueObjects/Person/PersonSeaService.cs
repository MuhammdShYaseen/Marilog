
namespace Marilog.Domain.ValueObjects.Person
{
    public sealed class PersonSeaService : ValueObject
    {
        public int RankId { get; private set; }
        public int ExperienceInMonths { get; private set; }
        public decimal? VesselSizeInMT { get; private set; }

        private PersonSeaService() { }

        public static PersonSeaService Create(int rankId,
            int experienceInMonths,
            decimal? vesselSizeInMT = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(experienceInMonths);
            ArgumentOutOfRangeException.ThrowIfLessThan(rankId, 1);
            return new PersonSeaService
            {
                RankId = rankId,
                ExperienceInMonths = experienceInMonths,
                VesselSizeInMT = vesselSizeInMT
            };
        }

        public PersonSeaService WithUpdates(int rankId,
            int experienceInMonths,
            decimal? vesselSizeInMT)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(experienceInMonths);
            ArgumentOutOfRangeException.ThrowIfLessThan(rankId, 1);
            return new PersonSeaService
            {
                RankId = rankId,
                ExperienceInMonths = experienceInMonths,
                VesselSizeInMT = vesselSizeInMT
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return RankId;
            yield return ExperienceInMonths;
            yield return VesselSizeInMT;
        }
    }
}
