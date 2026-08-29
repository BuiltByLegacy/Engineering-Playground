using System;
using System.Linq;
using EngineeringPlayground.Core.Content;
using EngineeringPlayground.Flow.Showcases;
using NUnit.Framework;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class FlowShowcaseTests
    {
        [Test]
        public void ShowcaseManifest_LoadsFourUniqueUnityRelativeScenarios()
        {
            var manifest = ContentRepository.LoadFlowShowcases();

            Assert.That(manifest.Showcases.Count, Is.EqualTo(4));
            Assert.That(manifest.Showcases.Select(s => s.Id).Distinct().Count(), Is.EqualTo(4));
            CollectionAssert.AreEquivalent(
                new[] { "plumbing", "exhaust", "hvac", "manifold" },
                manifest.Showcases.Select(s => s.Id).ToArray());

            foreach (var entry in manifest.Showcases)
            {
                Assert.That(entry.Path, Does.Not.StartWith("res://"));
                Assert.That(entry.Path, Does.Not.Contain(".."));
                var challenge = ContentRepository.LoadChallenge(entry.Path);
                Assert.That(challenge.PlaygroundId, Is.EqualTo("flow"));
                Assert.That(challenge.ChallengeId, Is.Not.Empty);
            }
        }

        [Test]
        public void AllReferenceAssumptions_ProduceFinitePositiveEngineeringResults()
        {
            foreach (var assumption in FlowShowcaseReferenceCatalog.All)
            {
                var result = assumption.Evaluate();

                Assert.That(double.IsFinite(result.VolumetricFlowM3PerS), Is.True, assumption.ShowcaseId);
                Assert.That(double.IsFinite(result.ReynoldsNumber), Is.True, assumption.ShowcaseId);
                Assert.That(double.IsFinite(result.FrictionFactor), Is.True, assumption.ShowcaseId);
                Assert.That(double.IsFinite(result.TotalLossPa), Is.True, assumption.ShowcaseId);
                Assert.That(result.VolumetricFlowM3PerS, Is.GreaterThan(0.0), assumption.ShowcaseId);
                Assert.That(result.ReynoldsNumber, Is.GreaterThan(0.0), assumption.ShowcaseId);
                Assert.That(result.FrictionFactor, Is.GreaterThan(0.0), assumption.ShowcaseId);
                Assert.That(result.TotalLossPa, Is.GreaterThan(0.0), assumption.ShowcaseId);

                var card = FlowReferenceEstimateFormatter.Format(assumption);
                Assert.That(card, Does.Contain("REFERENCE ESTIMATE"), assumption.ShowcaseId);
                Assert.That(card, Does.Contain("not a conversion of lattice units"), assumption.ShowcaseId);
                Assert.That(card, Does.Contain("not a professional engineering result"), assumption.ShowcaseId);
            }
        }

        [Test]
        public void LegacyShowcasePaths_AreNormalizedWithoutTraversal()
        {
            Assert.That(
                ShowcaseCatalog.NormalizeContentPath("res://content/flow/showcases/001_fix_the_shower.json"),
                Is.EqualTo("flow/showcases/001_fix_the_shower.json"));

            Assert.Throws<System.IO.InvalidDataException>(() =>
                ShowcaseCatalog.NormalizeContentPath("flow/../secret.json"));
        }
    }
}
