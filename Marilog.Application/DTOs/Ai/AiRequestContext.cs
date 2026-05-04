using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Marilog.Application.DTOs.Ai
{
    public class AiRequestContext
    {
        /// <summary>Simple single-turn prompt. Used if Messages is empty.</summary>
        public string? Prompt { get; init; }

        /// <summary>Full conversation history including system message.</summary>
        public IReadOnlyList<AiMessage>? Messages { get; init; }

        // Overrides — take priority over provider defaults
        public decimal? TemperatureOverride { get; init; }
        public int? MaxTokensOverride { get; init; }
        public bool? StreamOverride { get; init; }

        /// <summary>
        /// Extra parameters merged into the final payload last,
        /// overriding any previously set key.
        /// </summary>
        public Dictionary<string, object>? AdditionalParameters { get; init; }
    }
}
