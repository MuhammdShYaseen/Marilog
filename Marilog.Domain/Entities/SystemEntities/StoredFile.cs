using Marilog.Domain.Common;
using Marilog.Domain.Events;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class StoredFile : Entity
    {
        public EntityType EntityType { get; private set; } = EntityType.NONE;
        public int? EntityId { get; private set; }
        public int? FolderId { get; private set; } // null = root
        public string OriginalFileName { get; private set; } = null!;// from user machine
        public string StoredFileName { get; private set; } = null!; //saved as GUID this GUID Come from File its self couse of it has GUID property inheret from Entity;
        public string RelativePath { get; private set; } = null!;
        public string? ThumbnailRelativePath { get; private set; }
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
            EntityType entityType,
            int? entityId,
            int? folderId = null)
        {
            var storedfile = new StoredFile
            {
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                RelativePath = relativePath,
                ContentType = contentType,
                Size = size,
                Checksum = checksum,
                EntityId = entityId,
                EntityType = entityType,
                FolderId = folderId
            };
            storedfile.AddDomainEvent(new StoredFileOcrRequestedEvent(storedfile.Guid, storedfile.RelativePath));
            return storedfile;
        }

        public void UpdateEntityLink(EntityType entityType, int? entityId)
        {
            // إذا كان نفس الرابط → لا شيء يتغير
            if (EntityType == entityType && EntityId == entityId)
                return;

            // تحديث الربط فقط (تصحيح خطأ المستخدم)
            EntityType = entityType;
            EntityId = entityId;
            FolderId = null; // المجلد تابع للـ entity القديم
            Touch();
        }

        public void UpdateContent(string content)
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


        //------Thumbnail----------------------------------------------------------------------
        public void SetThumbnail(string? relativePath)
        {
            ThumbnailRelativePath = relativePath;
        }


        //---Folder------------------------------------------------------------------------------

        public void MoveToFolder(int? folderId)
        {
            if (FolderId == folderId)
                return;

            FolderId = folderId;
            Touch();
        }

    }
}