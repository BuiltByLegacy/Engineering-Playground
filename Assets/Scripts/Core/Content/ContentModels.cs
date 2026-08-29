using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EngineeringPlayground.Core.Content
{
    public sealed class CampaignDefinition
    {
        [JsonProperty("schema_version")] public int SchemaVersion { get; set; }
        [JsonProperty("campaign_id")] public string CampaignId { get; set; } = string.Empty;
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("chapters")] public List<ChapterDefinition> Chapters { get; set; } = new();
    }

    public sealed class ChapterDefinition
    {
        [JsonProperty("chapter_id")] public string ChapterId { get; set; } = string.Empty;
        [JsonProperty("chapter_number")] public int ChapterNumber { get; set; }
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("unlock_stars")] public int UnlockStars { get; set; }
        [JsonProperty("challenges")] public List<ChallengeDefinition> Challenges { get; set; } = new();
    }

    public sealed class ChallengeDefinition
    {
        [JsonProperty("schema_version")] public int SchemaVersion { get; set; }
        [JsonProperty("challenge_id")] public string ChallengeId { get; set; } = string.Empty;
        [JsonProperty("playground_id")] public string PlaygroundId { get; set; } = string.Empty;
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("description")] public string Description { get; set; } = string.Empty;
        [JsonProperty("difficulty")] public int Difficulty { get; set; }
        [JsonProperty("presentation_mode")] public string PresentationMode { get; set; } = "explorer";
        [JsonProperty("starting_state")] public JObject StartingState { get; set; } = new();
        [JsonProperty("allowed_tools")] public List<string> AllowedTools { get; set; } = new();
        [JsonProperty("constraints")] public JObject Constraints { get; set; } = new();
        [JsonProperty("success_conditions")] public JObject SuccessConditions { get; set; } = new();
        [JsonProperty("scoring_weights")] public JObject ScoringWeights { get; set; } = new();
        [JsonProperty("concept_unlocks")] public List<string> ConceptUnlocks { get; set; } = new();
        [JsonProperty("hints")] public List<string> Hints { get; set; } = new();
        [JsonProperty("rewards")] public JObject Rewards { get; set; } = new();
        [JsonProperty("domain_config")] public JObject DomainConfig { get; set; } = new();
        [JsonProperty("campaign")] public JObject Campaign { get; set; } = new();
    }
}
