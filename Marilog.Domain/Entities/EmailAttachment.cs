using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    /// <summary>
    /// Owned entity — lives only inside Email aggregate.
    /// </summary>
    public class EmailAttachment : Entity
    {
        public int EmailId { get; private set; }
        public string FileName { get; private set; } = null!;
        public string FilePath { get; private set; } = null!;
        public long FileSizeBytes { get; private set; }

        private EmailAttachment() { }

        internal static EmailAttachment Create(int emailId, string fileName,
            string filePath, long fileSizeBytes)
        {
            if (fileSizeBytes < 0) throw new ArgumentException("FileSizeBytes cannot be negative.");

            return new EmailAttachment
            {
                EmailId = emailId,
                FileName = fileName,
                FilePath = filePath,
                FileSizeBytes = fileSizeBytes
            };
        }
    }
}