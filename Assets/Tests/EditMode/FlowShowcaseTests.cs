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
        public void ShowcaseDefinitions_UseFourDedicatedGeometryPresets()
        {
            var manifest = ContentRepository.LoadFlowShowcases();
            var geometryIds = manifest.Showcases
                .Select(entry => ContentRepository.LoadChallenge(entry.Path).StartingState.Value<string>("geometry"))
                .ToArray();

            Assert.That(geometryIds, Has.None.EqualTo("default_channel"));
            Assert.That(geometryIds.Distinct().Count(), Is.EqualTo(4));
            CollectionAssert.AreEquivalent(FlowShowcaseGeometryPresets.AllIds, geometryIds);
        }

        [Test]
        public void GeometryPresets_AreDistinctAndPreserveOpenInletOutletColumns()
        {
            const int width = 96;
            const int height = 54;
            var masks = FlowShowcaseGeometryPresets.AllIds
                .Select(id => FlowShowcaseGeometryPresets.Build(id, width, height))
                .ToArray();

            for (var i = 0; i < masks.Length; i++)
            {
                var mask = masks[i];
                Assert.That(mask.Count(value => value), Is.GreaterThan(width * 2), FlowShowcaseGeometryPresets.AllIds[i]);

                for (var y = 1; y < height - 1; y++)
                {
                    Assert.That(mask[y * width + 1], Is.False, FlowShowcaseGeometryPresets.AllIds[i]);
                    Assert.That(mask[y * width + width - 2], Is.False, FlowShowcaseGeometryPresets.AllIds[i]);
                }
            }

            for (var a = 0; a < masks.Length; a++)
            for (var b = a + 1; b < masks.Length; b++)
                Assert.That(masks[a].SequenceEqual(masks[b]), Is.False,
                    $"{FlowShowcaseGeometryPresets.AllIds[a]} and {FlowShowcaseGeometryPresets.AllIds[b]} should not share a mask.");
        }

        [Test]
        public void UnknownShowcaseGeometry_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FlowShowcaseGeometryPresets.Build("showcase_typo", 96, 54));
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
