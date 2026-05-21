using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.OCR
{
    public sealed record OcrRequest(
    int DocumentId,
    string FilePath
);
}
