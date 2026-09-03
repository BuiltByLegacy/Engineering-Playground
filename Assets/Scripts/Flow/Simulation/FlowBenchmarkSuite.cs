using System;
using EngineeringPlayground.Flow.Pipes;
using UnityEngine;

namespace EngineeringPlayground.Flow.Simulation
{
    public sealed class FlowBenchmarkReport
    {
        public bool Stable { get; set; }
        public double StraightOutletSpeed { get; set; }
        public double StraightMassFluxError { get; set; }
        public double ObstacleVorticity { get; set; }
        public double BaselineVorticity { get; set; }
        public double ShortRoutePressureLikeLoss { get; set; }
        public double LongRoutePressureLikeLoss { get; set; }
        public double CoarseOutletSpeed { get; set; }
        public double FineOutletSpeed { get; set; }
        public double GridSensitivity => Math.Abs(FineOutletSpeed-CoarseOutletSpeed)/Math.Max(1e-9,Math.Abs(FineOutletSpeed));
    }

    public static class FlowBenchmarkSuite
    {
        public static FlowBenchmarkReport Run(int iterations=900)
        {
            // Pipe benchmarks stay pipe-to-pipe: conservation, outlet speed, route loss, and grid sensitivity.
            var straight=RunStraight(160,90,iterations);
            var coarse=RunStraight(96,54,iterations);

            // Wake/vorticity is a separate like-for-like open-channel benchmark. The previous
            // definition incorrectly compared a pipe-confined baseline with an open-channel
            // obstacle case, so geometry/boundary-condition effects polluted the signal.
            var openBaseline=RunOpenChannel(160,90,iterations,false);
            var obstacle=RunOpenChannel(160,90,iterations,true);

            var shortLoss=RunPipeLoss(new[]{new Vector2(.02f,.5f),new Vector2(.34f,.5f),new Vector2(.66f,.5f),new Vector2(.98f,.5f)},iterations);
            var longLoss=RunPipeLoss(new[]{new Vector2(.02f,.5f),new Vector2(.22f,.66f),new Vector2(.42f,.38f),new Vector2(.63f,.68f),new Vector2(.80f,.44f),new Vector2(.98f,.5f)},iterations);
            return new FlowBenchmarkReport {
                Stable=straight.solver.IsFinite()&&coarse.solver.IsFinite()&&openBaseline.IsFinite()&&obstacle.IsFinite(),
                StraightOutletSpeed=straight.solver.MeanOutletSpeed(),
                StraightMassFluxError=straight.solver.RelativeMassFluxError(),
                BaselineVorticity=openBaseline.MeanAbsoluteVorticity(),
                ObstacleVorticity=obstacle.MeanAbsoluteVorticity(),
                ShortRoutePressureLikeLoss=shortLoss,
                LongRoutePressureLikeLoss=longLoss,
                CoarseOutletSpeed=coarse.solver.MeanOutletSpeed(),
                FineOutletSpeed=straight.solver.MeanOutletSpeed()
            };
        }

        private static D2Q9LbmSolver RunOpenChannel(int width,int height,int iterations,bool withObstacle)
        {
            var solver=new D2Q9LbmSolver(width,height,1.82,.05);
            if(withObstacle) solver.AddCircularObstacle((int)(width*.45),(int)(height*.5),Math.Max(3,(int)(height*.09)));
            solver.Step(iterations);
            return solver;
        }

        private static (D2Q9LbmSolver solver, PipePathModel path) RunStraight(int width,int height,int iterations)
        {
            var solver=new D2Q9LbmSolver(width,height,1.82,.05);var path=new PipePathModel();path.SetPreset(new[]{new Vector2(.02f,.5f),new Vector2(.34f,.5f),new Vector2(.66f,.5f),new Vector2(.98f,.5f)},.09f);PipeSolverAdapter.Apply(solver,path);solver.Step(iterations);return (solver,path);
        }
        private static double RunPipeLoss(Vector2[] points,int iterations)
        {
            var solver=new D2Q9LbmSolver(160,90,1.82,.05);var path=new PipePathModel();path.SetPreset(points,.09f);PipeSolverAdapter.Apply(solver,path);solver.Step(iterations);return Math.Abs(solver.MeanDensityAtColumn(1)-solver.MeanDensityAtColumn(solver.Width-2));
        }
    }
}
