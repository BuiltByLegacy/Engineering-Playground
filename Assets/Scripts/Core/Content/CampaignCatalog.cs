using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace EngineeringPlayground.Core.Content
{
    public static class CampaignCatalog
    {
        public static CampaignDefinition Parse(string json)
        {
            var campaign = JsonConvert.DeserializeObject<CampaignDefinition>(json);
            if (campaign == null)
                throw new InvalidDataException("Campaign JSON deserialized to null.");
            if (campaign.SchemaVersion != 1)
                throw new InvalidDataException($"Unsupported campaign schema {campaign.SchemaVersion}.");

            var challenges = campaign.Chapters.SelectMany(c => c.Challenges).ToList();
            var duplicate = challenges.GroupBy(c => c.ChallengeId).FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
                throw new InvalidDataException($"Duplicate challenge_id: {duplicate.Key}");

            foreach (var challenge in challenges)
                ValidateChallenge(challenge);

            return campaign;
        }

        public static ChallengeDefinition ParseChallenge(string json)
        {
            var challenge = JsonConvert.DeserializeObject<ChallengeDefinition>(json);
            if (challenge == null)
                throw new InvalidDataException("Challenge JSON deserialized to null.");
            ValidateChallenge(challenge);
            return challenge;
        }

        public static ChallengeDefinition FindChallenge(CampaignDefinition campaign, string challengeId)
        {
            return campaign.Chapters.SelectMany(c => c.Challenges)
                .FirstOrDefault(c => string.Equals(c.ChallengeId, challengeId, StringComparison.Ordinal))
                ?? throw new KeyNotFoundException($"Challenge not found: {challengeId}");
        }

        public static int ChallengeCount(CampaignDefinition campaign) =>
            campaign.Chapters.Sum(c => c.Challenges.Count);

        public static void ValidateChallenge(ChallengeDefinition challenge)
        {
            if (challenge.SchemaVersion != 1)
                throw new InvalidDataException($"{challenge.ChallengeId}: unsupported schema_version {challenge.SchemaVersion}.");
            if (string.IsNullOrWhiteSpace(challenge.ChallengeId))
                throw new InvalidDataException("challenge_id is required.");
            if (string.IsNullOrWhiteSpace(challenge.PlaygroundId))
                throw new InvalidDataException($"{challenge.ChallengeId}: playground_id is required.");
            if (string.IsNullOrWhiteSpace(challenge.Title))
                throw new InvalidDataException($"{challenge.ChallengeId}: title is required.");
            if (!challenge.SuccessConditions.HasValues)
                throw new InvalidDataException($"{challenge.ChallengeId}: success_conditions is required.");
            if (!challenge.ScoringWeights.HasValues)
                throw new InvalidDataException($"{challenge.ChallengeId}: scoring_weights is required.");
        }
    }
}
