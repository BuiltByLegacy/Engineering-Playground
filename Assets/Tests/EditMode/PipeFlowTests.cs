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
            path.MovePoint(1,new Vector2(.34f,.72f));
            Assert.That(path.RouteLength,Is.GreaterThan(before));
        }

        [Test]
        public void PipeMask_OpensCenterlineAndClosesEnvironment()
        {
            var solver=new D2Q9LbmSolver(96,54);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            var mask=PipeSolverAdapter.BuildSolidMask(solver,path);

            var centerY=Mathf.RoundToInt(.5f*(solver.Height-1));
            Assert.That(mask[centerY*solver.Width+solver.Width/2],Is.False,"Pipe centerline should be fluid.");
            Assert.That(mask[2*solver.Width+solver.Width/2],Is.True,"Environment outside the pipe should be solid.");
        }

        [Test]
        public void PipeMask_ConstrainsInletCrossSection()
        {
            var solver=new D2Q9LbmSolver(96,54);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            PipeSolverAdapter.Apply(solver,path);

            var centerY=Mathf.RoundToInt(.5f*(solver.Height-1));
            Assert.That(solver.Solid[centerY*solver.Width+1],Is.False);
            Assert.That(solver.Solid[2*solver.Width+1],Is.True);
        }

        [Test]
        public void StraightPipe_RemainsFiniteAndDeliversOutletFlow()
        {
            var solver=new D2Q9LbmSolver(96,54);
            var path=new PipePathModel();
            path.SetPreset(PipePathPresets.ForLevel(1),.09f);
            PipeSolverAdapter.Apply(solver,path);
            solver.Step(600);

            Assert.That(solver.IsFinite(),Is.True);
            Assert.That(solver.MeanOutletSpeed(),Is.GreaterThan(0.0));
        }
    }
}
