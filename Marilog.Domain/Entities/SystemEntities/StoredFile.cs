using Marilog.Domain.Common;
using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class StoredFile : Entity
    {
        public EntityType? EntityType { get; private set; } = null!;  // "Document" | "SwiftTransfer" | "Voyage"
        public int? EntityId { get; private set; }
        public string OriginalFileName { get; private set; } = null!;
        public string StoredFileName { get; private set; } = null!;
        public string RelativePath { get; private set; } = null!;
        public string ContentType { get; private set; } = null!;
        public long Size { get; private set; }

        public string Checksum { get; private set; } = null!; // optional but useful

        public DateTime CreatedAtUtc { get; private set; }

        private StoredFile() { }

        public static StoredFile Create(
            string originalFileName,
            string storedFileName,
            string relativePath,
            string contentType,
            long size,
            string checksum,
            EntityType? entityType,
            int? entityId)
        {
            return new StoredFile
            {
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                RelativePath = relativePath,
                ContentType = contentType,
                Size = size,
                Checksum = checksum,
                CreatedAtUtc = DateTime.UtcNow,
                EntityId = entityId,
                EntityType = entityType
            };
        }

        public void UpdateEntityLink(EntityType? entityType, int? entityId)
        {
            // إذا كان نفس الرابط → لا شيء يتغير
            if (EntityType == entityType && EntityId == entityId)
                return;

            // تحديث الربط فقط (تصحيح خطأ المستخدم)
            EntityType = entityType;
            EntityId = entityId;
        }
    }
}