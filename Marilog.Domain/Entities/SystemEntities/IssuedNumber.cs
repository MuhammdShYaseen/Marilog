
using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class IssuedNumber : Entity
    {
        public EntityType EntityType { get; private set; }
        public Guid EntityId { get; private set; }
        public string Code { get; private set; } = null!;
        public int Year { get; private set; }
        public int Sequence { get; private set; }
        public string GeneratedNumber { get; private set; } = null!;
        public string? Description { get; private set; }

        private IssuedNumber() { }

        public static IssuedNumber Create(EntityType entityType,  Guid entityId,
                                          string code, int year,  int sequence,
                                          string generatedNumber, string? description)
        {
            return new IssuedNumber()
            {
                EntityType = entityType,
                EntityId = entityId,
                Code = code,
                Year = year,
                Sequence = sequence,
                GeneratedNumber = generatedNumber,
                Description = description
            };
        }

        public void ChangeSequence(int sequence, string generatedNumber, string? description)
        {
            Sequence = sequence;
            GeneratedNumber = generatedNumber;
            Description = description;
        }
    }
}
