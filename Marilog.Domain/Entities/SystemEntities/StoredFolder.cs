using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class StoredFolder : Entity
    {
        public const int NameMaxLength = 200;

        public string Name { get; private set; } = null!;
        public int? ParentFolderId { get; private set; } // null = root
        public EntityType EntityType { get; private set; } = EntityType.NONE;
        public int? EntityId { get; private set; }

        private StoredFolder() { }

        public static StoredFolder Create(string name, int? parentFolderId, EntityType entityType, int? entityId)
        {
            return new StoredFolder
            {
                Name = ValidateName(name),
                ParentFolderId = parentFolderId,
                EntityType = entityType,
                EntityId = entityId
            };
        }

        public void Rename(string name)
        {
            var validated = ValidateName(name);
            if (Name == validated)
                return;

            Name = validated;
            Touch();
        }

        public void MoveTo(int? parentFolderId)
        {
            if (ParentFolderId == parentFolderId)
                return;

            if (parentFolderId == Id)
                throw new ArgumentException("A folder can't be moved into itself.", nameof(parentFolderId));

            ParentFolderId = parentFolderId;
            Touch();
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Folder name is required.", nameof(name));

            var trimmed = name.Trim();
            if (trimmed.Length > NameMaxLength)
                throw new ArgumentException($"Folder name can't exceed {NameMaxLength} characters.", nameof(name));

            return trimmed;
        }
    }
}