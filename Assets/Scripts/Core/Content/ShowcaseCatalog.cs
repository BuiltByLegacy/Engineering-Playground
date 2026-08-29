using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace EngineeringPlayground.Core.Content
{
    public sealed class ShowcaseManifest
    {
        [JsonProperty("schema_version")] public int SchemaVersion { get; set; }
        [JsonProperty("showcases")] public List<ShowcaseEntry> Showcases { get; set; } = new();
    }

    public sealed class ShowcaseEntry
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("path")] public string Path { get; set; } = string.Empty;
        [JsonProperty("theme")] public string Theme { get; set; } = string.Empty;
    }

    public static class ShowcaseCatalog
    {
        private const string LegacyPrefix = "res://content/";

        public static ShowcaseManifest Parse(string json)
        {
            var manifest = JsonConvert.DeserializeObject<ShowcaseManifest>(json)
                           ?? throw new InvalidDataException("Showcase manifest deserialized to null.");
            if (manifest.SchemaVersion != 1)
                throw new InvalidDataException($"Unsupported showcase schema {manifest.SchemaVersion}.");
            if (manifest.Showcases.Count == 0)
                throw new InvalidDataException("Showcase manifest must contain at least one showcase.");

            var duplicate = manifest.Showcases
                .GroupBy(s => s.Id, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
                throw new InvalidDataException($"Duplicate showcase id: {duplicate.Key}");

            foreach (var entry in manifest.Showcases)
            {
                if (string.IsNullOrWhiteSpace(entry.Id))
                    throw new InvalidDataException("Showcase id is required.");
                if (string.IsNullOrWhiteSpace(entry.Title))
                    throw new InvalidDataException($"{entry.Id}: title is required.");
                if (string.IsNullOrWhiteSpace(entry.Path))
                    throw new InvalidDataException($"{entry.Id}: path is required.");
                entry.Path = NormalizeContentPath(entry.Path);
            }

            return manifest;
        }

        public static string NormalizeContentPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Content path is required.", nameof(path));

            var normalized = path.Replace('\\', '/').Trim();
            if (normalized.StartsWith(LegacyPrefix, StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(LegacyPrefix.Length);
            while (normalized.StartsWith("/", StringComparison.Ordinal))
                normalized = normalized.Substring(1);
            if (normalized.Contains(".."))
                throw new InvalidDataException("Showcase content path cannot traverse directories.");
            return normalized;
        }
    }
}
