using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Responses
{
    public class StoredFileResponse
    {
        public int Id { get; set; }

        public int? EntityId { get; set; }
        public EntityType? EntityType { get; set; }

        public string OriginalFileName { get; set; } = null!;
        public string StoredFileName { get; set; } = null!;
        public string RelativePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Size { get; set; }

        public string? Content { get; set; } // OCR text (optional exposure)

        public string Checksum { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public IReadOnlyList<TagResponse>? Tags { get; set; }
    }
}
