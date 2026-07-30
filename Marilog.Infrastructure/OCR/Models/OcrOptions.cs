namespace Marilog.Infrastructure.OCR.Models
{
    public sealed class OcrOptions
    {
        /// <summary>لغات OCR — مثال: "eng+ara" أو "ara"</summary>
        public string Languages { get; init; } = "eng+ara";

        /// <summary>مسار tessdata (يتم تحديده تلقائياً إذا ترك فارغاً)</summary>
        public string? TessDataPath { get; init; }

        /// <summary>DPI للتحويل من PDF إلى صورة (كلما ارتفع، دقة OCR أعلى)</summary>
        public int RenderDpi { get; init; } = 300;

        /// <summary>حد أدنى لثقة الكلمة (0–100). الكلمات دون الحد لا تُضاف</summary>
        public float MinConfidence { get; init; } = 30f;

        /// <summary>تصحيح ميلان الصفحة قبل OCR</summary>
        public bool Deskew { get; init; } = true;

        /// <summary>حفظ نسخة احتياطية من الملف الأصلي</summary>
        public bool KeepOriginalBackup { get; init; } = false;

        public int BatchSize { get; init; } = 4;
        public int MaxDegreeOfParallelism { get; init; } = 2;
    }
}
