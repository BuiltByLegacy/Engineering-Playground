using EngineeringPlayground.Core.Content;
using EngineeringPlayground.Flow.Challenges;
using NUnit.Framework;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class FlowChallengeScorerTests
    {
        [Test]
        public void PerfectMetricsPassAndEarnThreeStarsForFirstChallenge()
        {
            var campaign = ContentRepository.LoadFlowCampaign();
            var challenge = CampaignCatalog.FindChallenge(campaign, "flow_001_make_it_flow");
            var result = FlowChallengeScorer.Evaluate(challenge, new FlowChallengeMetrics(0.060, 0.0, 0.0, 0));

            Assert.That(result.Passed, Is.True);
            Assert.That(result.Score, Is.EqualTo(100.0).Within(0.01));
            Assert.That(result.Grade, Is.EqualTo("S"));
            Assert.That(result.Stars, Is.EqualTo(3));
        }

        [Test]
        public void FailingThresholdsCannotEarnStars()
        {
            var campaign = ContentRepository.LoadFlowCampaign();
            var challenge = CampaignCatalog.FindChallenge(campaign, "flow_001_make_it_flow");
            var result = FlowChallengeScorer.Evaluate(challenge, new FlowChallengeMetrics(0.01, 0.05, 0.03, 260));

            Assert.That(result.Passed, Is.False);
            Assert.That(result.Stars, Is.EqualTo(0));
        }

        [Test]
        public void LaunchCampaignRetainsThirtyLevelsAndFiveChapters()
        {
            var campaign = ContentRepository.LoadFlowCampaign();

            Assert.That(campaign.Chapters.Count, Is.EqualTo(5));
            Assert.That(CampaignCatalog.ChallengeCount(campaign), Is.EqualTo(30));
            Assert.That(campaign.Chapters[0].UnlockStars, Is.EqualTo(0));
            Assert.That(campaign.Chapters[1].UnlockStars, Is.GreaterThan(0));
        }
    }
}
