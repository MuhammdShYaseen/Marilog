using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Requests.StoregFileDTOs
{
    public class UpdateOcrContentRequest
    {
        public string Content { get; init; } = null!;
    }
}
