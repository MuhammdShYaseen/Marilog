using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.AiEntities
{

    public class PdfAnalysisJob : Entity
    {
        public string OriginalFileName { get; private set; } = default!;
        public string StoredFilePath { get; private set; } = default!;
        public DocumentType DocumentType { get; private set; } = null!;
        public int DocumentTypeId { get; private set; }
        public int AiProviderId { get; private set; }
        public AiProvider AiProvider { get; private set; } = default!;
        public JobStatus Status { get; private set; }
        public string? ExtractedText { get; private set; }
        public string? RawAiResponse { get; private set; }       // raw JSON from AI
        public string? ErrorMessage { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private PdfAnalysisJob() { }

        public static PdfAnalysisJob Create(
            string originalFileName,
            string storedFilePath,
            int documentTypeId,
            int aiProviderId)
        {
            return new PdfAnalysisJob
            {
                OriginalFileName = originalFileName,
                StoredFilePath = storedFilePath,
                DocumentTypeId = documentTypeId,
                AiProviderId = aiProviderId,
                Status = JobStatus.Pending
            };
        }

        public void MarkProcessing() => Status = JobStatus.Processing;

        public void MarkCompleted(string extractedText, string rawAiResponse)
        {
            ExtractedText = extractedText;
            RawAiResponse = rawAiResponse;
            Status = JobStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        public void MarkFailed(string errorMessage)
        {
            ErrorMessage = errorMessage;
            Status = JobStatus.Failed;
            CompletedAt = DateTime.UtcNow;
        }
    }
}
