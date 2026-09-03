using EngineeringPlayground.Flow.Pipes;
using EngineeringPlayground.Flow.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class PipeFlowTests
    {
        [Test]
        public void MovingHandle_ChangesRouteLength()
        {
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            var before=path.RouteLength;
            path.MovePoint(1,new Vector2(.34f,.60f));
            Assert.That(path.RouteLength,Is.GreaterThan(before));
        }

        [Test]
        public void MovingHandle_CannotMakeBendRadiusWorseBelowGuardrail()
        {
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(2),.075f);
            var before=path.MinimumBendRadius;
            path.MovePoint(2,new Vector2(.50f,.90f));
            Assert.That(path.MinimumBendRadius,Is.GreaterThanOrEqualTo(before-0.0002f));
        }

        [Test]
        public void PipeMask_OpensCenterlineAndClosesEnvironment()
        {
            var solver=new D2Q9LbmSolver(160,90);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            var mask=PipeSolverAdapter.BuildSolidMask(solver,path);

            var centerY=Mathf.RoundToInt(.5f*(solver.Height-1));
            Assert.That(mask[centerY*solver.Width+solver.Width/2],Is.False,"Pipe centerline should be fluid.");
            Assert.That(mask[2*solver.Width+solver.Width/2],Is.True,"Environment outside the pipe should be solid.");
        }

        [Test]
        public void SupersampledPipeMask_HasConsistentStraightPipeDiameter()
        {
            var solver=new D2Q9LbmSolver(160,90);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            var mask=PipeSolverAdapter.BuildSolidMask(solver,path);

            var min=int.MaxValue;var max=0;
            for(var x=12;x<solver.Width-12;x+=8)
            {
                var open=0;
                for(var y=1;y<solver.Height-1;y++) if(!mask[y*solver.Width+x]) open++;
                min=Mathf.Min(min,open);max=Mathf.Max(max,open);
            }
            Assert.That(min,Is.GreaterThanOrEqualTo(14),"High-resolution pipe should have enough fluid cells across its diameter.");
            Assert.That(max-min,Is.LessThanOrEqualTo(2),"A straight pipe should not have a visibly stair-stepped changing diameter.");
        }

        [Test]
        public void PipeMask_ConstrainsInletCrossSection()
        {
            var solver=new D2Q9LbmSolver(160,90);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            PipeSolverAdapter.Apply(solver,path);

            var centerY=Mathf.RoundToInt(.5f*(solver.Height-1));
            Assert.That(solver.Solid[centerY*solver.Width+1],Is.False);
            Assert.That(solver.Solid[2*solver.Width+1],Is.True);
        }

        [Test]
        public void StraightPipe_RemainsFiniteAndDeliversOutletFlowAtProductionResolution()
        {
            var solver=new D2Q9LbmSolver(160,90);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            PipeSolverAdapter.Apply(solver,path);
            solver.Step(900);

            Assert.That(solver.IsFinite(),Is.True);
            Assert.That(solver.MeanOutletSpeed(),Is.GreaterThan(0.0));
        }

        [Test]
        public void SmoothDetour_DefaultPreset_RemainsFiniteAndDeliversVisibleOutletFlow()
        {
            var solver=new D2Q9LbmSolver(160,90,1.82,0.05);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(2),.075f);
            var mask=PipeSolverAdapter.BuildSolidMask(solver,path);

            AddCircularObstacle(mask,solver,.52f,.50f,.055f);
            solver.ApplySolidMask(mask,true);
            solver.Step(1500);

            Assert.That(solver.IsFinite(),Is.True,"Level 2 default route must remain numerically stable.");
            Assert.That(solver.MeanOutletSpeed(),Is.GreaterThan(0.006),"Default Smooth Detour must deliver measurable flow to OUT rather than visually stalling upstream.");
        }

        private static void AddCircularObstacle(bool[] mask,D2Q9LbmSolver solver,float nx,float ny,float normalizedRadius)
        {
            var cx=Mathf.RoundToInt(nx*(solver.Width-1));
            var cy=Mathf.RoundToInt(ny*(solver.Height-1));
            var radius=Mathf.Max(2,Mathf.RoundToInt(normalizedRadius*solver.Height));
            for(var y=1;y<solver.Height-1;y++)for(var x=1;x<solver.Width-1;x++)
            {
                var dx=x-cx;var dy=y-cy;
                if(dx*dx+dy*dy<=radius*radius)mask[y*solver.Width+x]=true;
            }
        }
    }
}
