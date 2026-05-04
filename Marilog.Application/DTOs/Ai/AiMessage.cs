using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Ai
{
    public sealed class AiMessage
    {
        public string Role { get; init; } = default!; // system | user | assistant
        public string Content { get; init; } = default!;
    }
}
