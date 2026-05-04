using Marilog.Application.DTOs.Ai;
using Marilog.Application.Interfaces.Ai;
using Marilog.Domain.Entities.AiEntities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Marilog.Application.Services.ApplicationServices.AiServices
{
    /// <summary>
    /// Builds the JSON request payload for any AI provider.
    ///
    /// How it works:
    ///   1. Parses RequestTemplateJson from the provider entity.
    ///   2. Walks the JSON tree replacing string placeholder values with real data.
    ///   3. {{messages}} is replaced with a JsonArray built from context.
    ///   4. {{system}} is replaced with system message content, or removed if absent.
    ///   5. AdditionalParameters are merged last and override any key.
    ///
    /// Supported placeholders:
    ///   {{model}}       {{temperature}}   {{max_tokens}}
    ///   {{stream}}      {{messages}}      {{system}}      {{prompt}}
    /// </summary>
    public sealed class GenericAiRequestBuilder : IAiRequestBuilder
    {
        public string Build(AiProvider provider, AiRequestContext context)
        {
            var root = ParseTemplate(provider.RequestTemplateJson);

            ReplaceCoreTokens(root, provider, context);
            ReplaceContentTokens(root, context);
            ApplyAdditional(root, context);

            return root.ToJsonString();
        }

        // ── Step 1: Core scalar tokens ───────────────────────────

        private void ReplaceCoreTokens(JsonObject root, AiProvider provider, AiRequestContext context)
        {
            var temperature = (context.TemperatureOverride ?? provider.DefaultTemperature)
                                .ToString(CultureInfo.InvariantCulture);
            var maxTokens = (context.MaxTokensOverride ?? provider.MaxOutputTokens).ToString();
            var stream = (context.StreamOverride ?? provider.SupportsStreaming).ToString().ToLower();

            ReplaceScalarToken(root, "{{model}}", provider.ModelIdentifier);
            ReplaceScalarToken(root, "{{temperature}}", temperature);
            ReplaceScalarToken(root, "{{max_tokens}}", maxTokens);
            ReplaceScalarToken(root, "{{stream}}", stream);
        }

        // ── Step 2: Content tokens (messages / system / prompt) ──

        private void ReplaceContentTokens(JsonObject root, AiRequestContext context)
        {
            var messages = ResolveMessages(context);

            ReplaceMessagesToken(root, messages);
            ReplaceSystemToken(root, messages);
            ReplacePromptToken(root, context);
        }

        private List<AiMessage> ResolveMessages(AiRequestContext context)
        {
            if (context.Messages?.Count > 0)
                return context.Messages.ToList();

            if (!string.IsNullOrWhiteSpace(context.Prompt))
                return [new AiMessage { Role = "user", Content = context.Prompt }];

            throw new InvalidOperationException(
                "AiRequestContext must provide either Messages or Prompt.");
        }

        private void ReplaceMessagesToken(JsonObject root, List<AiMessage> messages)
        {
            // Build array excluding system messages (system is handled separately)
            var array = new JsonArray();
            foreach (var m in messages.Where(m => m.Role != "system"))
            {
                array.Add(new JsonObject
                {
                    ["role"] = m.Role,
                    ["content"] = m.Content
                });
            }

            ReplaceNodeToken(root, "{{messages}}", array);
        }

        private void ReplaceSystemToken(JsonObject root, List<AiMessage> messages)
        {
            var systemContent = messages.FirstOrDefault(m => m.Role == "system")?.Content;

            if (systemContent is not null)
                ReplaceScalarToken(root, "{{system}}", systemContent);
            else
                RemoveToken(root, "{{system}}");
        }

        private void ReplacePromptToken(JsonObject root, AiRequestContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.Prompt))
                ReplaceScalarToken(root, "{{prompt}}", context.Prompt);
            else
                RemoveToken(root, "{{prompt}}");
        }

        // ── Step 3: Additional parameters ───────────────────────

        private void ApplyAdditional(JsonObject root, AiRequestContext context)
        {
            if (context.AdditionalParameters is null) return;
            foreach (var kv in context.AdditionalParameters)
                root[kv.Key] = JsonSerializer.SerializeToNode(kv.Value);
        }

        // ── Tree walkers ─────────────────────────────────────────

        /// <summary>Walks the tree replacing a string placeholder with a string value.</summary>
        private void ReplaceScalarToken(JsonObject node, string token, string value)
        {
            foreach (var key in node.Select(k => k.Key).ToList())
            {
                var child = node[key];
                switch (child)
                {
                    case JsonValue v when v.ToString() == token:
                        // Detect type from value — numbers stay numbers, booleans stay booleans
                        node[key] = DetectAndCreate(value);
                        break;
                    case JsonObject obj:
                        ReplaceScalarToken(obj, token, value);
                        break;
                    case JsonArray arr:                          // ← add this
                        ReplaceScalarTokenInArray(arr, token, value);
                        break;
                }
            }
        }

        /// <summary>Walks the tree replacing a string placeholder with a JsonNode (e.g. JsonArray).</summary>
        private void ReplaceNodeToken(JsonObject node, string token, JsonNode replacement)
        {
            foreach (var key in node.Select(k => k.Key).ToList())
            {
                var child = node[key];
                switch (child)
                {
                    case JsonValue v when v.ToString() == token:
                        node[key] = replacement;
                        break;
                    case JsonObject obj:
                        ReplaceNodeToken(obj, token, replacement);
                        break;
                }
            }
        }

        /// <summary>Walks the tree removing any key whose value equals the placeholder.</summary>
        private void RemoveToken(JsonObject node, string token)
        {
            foreach (var key in node.Select(k => k.Key).ToList())
            {
                var child = node[key];
                switch (child)
                {
                    case JsonValue v when v.ToString() == token:
                        node.Remove(key);
                        break;
                    case JsonObject obj:
                        RemoveToken(obj, token);
                        break;
                }
            }
        }

        // ── Helpers ──────────────────────────────────────────────

        /// <summary>
        /// Tries to preserve the correct JSON type when replacing a placeholder.
        /// "true"/"false" → JsonValue bool, numeric strings → JsonValue number, else string.
        /// </summary>
        private static JsonNode DetectAndCreate(string value)
        {
            if (value is "true" or "false")
                return JsonValue.Create(value == "true");

            if (long.TryParse(value, out var l))
                return JsonValue.Create(l);

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return JsonValue.Create(d);

            return JsonValue.Create(value)!;
        }

        private void ReplaceScalarTokenInArray(JsonArray array, string token, string value)
        {
            for (int i = 0; i < array.Count; i++)
            {
                switch (array[i])
                {
                    case JsonValue v when v.ToString() == token:
                        array[i] = DetectAndCreate(value);
                        break;
                    case JsonObject obj:
                        ReplaceScalarToken(obj, token, value);
                        break;
                    case JsonArray nested:
                        ReplaceScalarTokenInArray(nested, token, value);
                        break;
                }
            }
        }

        private static JsonObject ParseTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new InvalidOperationException("RequestTemplateJson is empty.");

            return JsonNode.Parse(template) as JsonObject
                ?? throw new InvalidOperationException("RequestTemplateJson must be a JSON object.");
        }
    }
}
