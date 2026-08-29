using System.IO;
using System.Linq;
using EngineeringPlayground.Core.Content;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class ContentMigrationTests
    {
        [Test]
        public void FlowCampaign_LoadsAllThirtyChallenges()
        {
            var campaign = ContentRepository.LoadFlowCampaign();
            Assert.That(CampaignCatalog.ChallengeCount(campaign), Is.EqualTo(30));
            Assert.That(campaign.Chapters.Count, Is.EqualTo(5));
            Assert.That(campaign.Chapters.SelectMany(c => c.Challenges).Select(c => c.ChallengeId).Distinct().Count(), Is.EqualTo(30));
        }

        [Test]
        public void FlowCampaign_PreservesFirstChallengeId()
        {
            var campaign = ContentRepository.LoadFlowCampaign();
            var challenge = CampaignCatalog.FindChallenge(campaign, "flow_001_make_it_flow");
            Assert.That(challenge.Title, Is.EqualTo("Make It Flow"));
            Assert.That(challenge.PlaygroundId, Is.EqualTo("flow"));
        }

        [Test]
        public void ShowcaseCatalog_ContainsFourResolvableDefinitions()
        {
            var root = JObject.Parse(ContentRepository.ReadText("flow/showcases.json"));
            var showcases = (JArray)root["showcases"]!;
            Assert.That(showcases.Count, Is.EqualTo(4));

            foreach (var showcase in showcases.OfType<JObject>())
            {
                var legacyPath = showcase.Value<string>("path");
                Assert.That(legacyPath, Is.Not.Null.And.Not.Empty);
                var relative = NormalizeLegacyContentPath(legacyPath!);
                var resolved = ContentRepository.Resolve(relative);
                Assert.That(File.Exists(resolved), Is.True, $"Missing showcase content: {relative}");
            }
        }

        private static string NormalizeLegacyContentPath(string path)
        {
            const string prefix = "res://content/";
            if (path.StartsWith(prefix))
                return path.Substring(prefix.Length);
            return path.TrimStart('/');
        }
    }
}
