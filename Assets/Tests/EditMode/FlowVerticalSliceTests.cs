using EngineeringPlayground.Flow.Challenges;
using EngineeringPlayground.Flow.Pipes;
using EngineeringPlayground.Flow.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class FlowVerticalSliceTests
    {
        [Test]
        public void RunComparison_ReportsImprovementAndExplainsGentlerBend()
        {
            var beforeMetrics=new FlowChallengeMetrics(.030,.020,.015,120);
            var afterMetrics=new FlowChallengeMetrics(.040,.014,.009,90);
            var before=new FlowRunSnapshot(beforeMetrics,1.20f,.8f,.05f,.09f,62);
            var after=new FlowRunSnapshot(afterMetrics,1.12f,.5f,.10f,.09f,78);
            var comparison=FlowRunAnalyzer.Compare(before,after);
            Assert.That(comparison.ScoreDelta,Is.EqualTo(16).Within(.001));
            StringAssert.Contains("Improved",comparison.Summary);
            StringAssert.Contains("bend",FlowEngineeringInsightEngine.Explain(after,comparison).ToLowerInvariant());
        }

        [Test]
        public void VariableDiameterProfile_InterpolatesSmoothlyAndResets()
        {
            var path=new PipePathModel();
            var points=PipePathPresets.ForLevel(4);var radii=PipePathPresets.RadiusProfileForLevel(4);
            path.SetPreset(points,radii);
            Assert.That(path.RadiusAt(.5f),Is.LessThan(.07f));
            Assert.That(path.RadiusAt(.05f),Is.GreaterThan(.09f));
            path.SetRadiusAtPoint(2,.12f);
            Assert.That(path.RadiusAt(.5f),Is.GreaterThan(.10f));
            path.ResetToPreset();
            Assert.That(path.RadiusAt(.5f),Is.LessThan(.07f));
        }

        [Test]
        public void VariableDiameterMask_PreservesOpenInletAndRestriction()
        {
            var solver=new D2Q9LbmSolver(160,90,1.82,.05);
            var path=new PipePathModel();path.SetPreset(PipePathPresets.ForLevel(4),PipePathPresets.RadiusProfileForLevel(4));
            var mask=PipeSolverAdapter.BuildSolidMask(solver,path);
            var inletOpen=CountOpen(mask,solver,2);var middleOpen=CountOpen(mask,solver,solver.Width/2);
            Assert.That(inletOpen,Is.GreaterThan(10));
            Assert.That(middleOpen,Is.GreaterThan(4));
            Assert.That(middleOpen,Is.LessThan(inletOpen));
        }

        [Test]
        public void BenchmarkSuite_StaysInsideProductionRegressionEnvelope()
        {
            var report=FlowBenchmarkSuite.Run(650);
            Assert.That(report.Stable,Is.True);
            Assert.That(double.IsFinite(report.StraightOutletSpeed),Is.True);
            Assert.That(report.StraightOutletSpeed,Is.GreaterThan(.005));
            Assert.That(report.StraightMassFluxError,Is.LessThan(.85));
            Assert.That(report.ObstacleVorticity,Is.GreaterThan(report.BaselineVorticity));
            Assert.That(report.GridSensitivity,Is.LessThan(.65));
        }

        private static int CountOpen(bool[] mask,D2Q9LbmSolver solver,int x)
        {
            var count=0;for(var y=1;y<solver.Height-1;y++)if(!mask[y*solver.Width+x])count++;return count;
        }
    }
}
