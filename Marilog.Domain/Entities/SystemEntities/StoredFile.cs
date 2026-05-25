using Marilog.Domain.Common;
using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class StoredFile : Entity
    {
        public EntityType? EntityType { get; private set; }
        public int? EntityId { get; private set; }
        public string OriginalFileName { get; private set; } = null!;// from user machine
        public string StoredFileName { get; private set; } = null!; //saved as GUID
        public string RelativePath { get; private set; } = null!; 
        public string ContentType { get; private set; } = null!;
        public string? Content {  get; private set; } = null;
        public long Size { get; private set; }
        private readonly List<Tag> _tags = [];
        public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();
        public string Checksum { get; private set; } = null!; // optional but useful is the file exist or not


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
            Touch();
        }

        public void UpdateContentFromOCR(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;
            Content = content;
            Touch();
        }


        //---Tags-------------------------------------------------------------------
        public void AddTag(string name, string color)
        {
            _tags.Add(Tag.Create(name, color, Id));
        }

        public void RemoveTag(int tagId)
        {
            var tag = _tags.FirstOrDefault(t => t.Id == tagId);
            if (tag is not null) _tags.Remove(tag);
        }

    }
}