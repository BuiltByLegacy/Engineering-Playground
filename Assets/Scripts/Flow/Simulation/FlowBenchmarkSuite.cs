using System;
using EngineeringPlayground.Flow.Pipes;
using UnityEngine;

namespace EngineeringPlayground.Flow.Simulation
{
    public sealed class FlowBenchmarkReport
    {
        public bool Stable { get; init; }
        public double StraightOutletSpeed { get; init; }
        public double StraightMassFluxError { get; init; }
        public double ObstacleVorticity { get; init; }
        public double BaselineVorticity { get; init; }
        public double ShortRoutePressureLikeLoss { get; init; }
        public double LongRoutePressureLikeLoss { get; init; }
        public double CoarseOutletSpeed { get; init; }
        public double FineOutletSpeed { get; init; }
        public double GridSensitivity => Math.Abs(FineOutletSpeed-CoarseOutletSpeed)/Math.Max(1e-9,Math.Abs(FineOutletSpeed));
    }

    public static class FlowBenchmarkSuite
    {
        public static FlowBenchmarkReport Run(int iterations=900)
        {
            var straight=RunStraight(160,90,iterations);
            var coarse=RunStraight(96,54,iterations);
            var baselineVorticity=straight.solver.MeanAbsoluteVorticity();

            var obstacle=new D2Q9LbmSolver(160,90,1.82,.05);
            obstacle.AddCircularObstacle(72,45,8);
            obstacle.Step(iterations);

            var shortLoss=RunPipeLoss(new[]{new Vector2(.02f,.5f),new Vector2(.34f,.5f),new Vector2(.66f,.5f),new Vector2(.98f,.5f)},iterations);
            var longLoss=RunPipeLoss(new[]{new Vector2(.02f,.5f),new Vector2(.22f,.66f),new Vector2(.42f,.38f),new Vector2(.63f,.68f),new Vector2(.80f,.44f),new Vector2(.98f,.5f)},iterations);

            return new FlowBenchmarkReport
            {
                Stable=straight.solver.IsFinite()&&coarse.solver.IsFinite()&&obstacle.IsFinite(),
                StraightOutletSpeed=straight.solver.MeanOutletSpeed(),
                StraightMassFluxError=straight.solver.RelativeMassFluxError(),
                BaselineVorticity=baselineVorticity,
                ObstacleVorticity=obstacle.MeanAbsoluteVorticity(),
                ShortRoutePressureLikeLoss=shortLoss,
                LongRoutePressureLikeLoss=longLoss,
                CoarseOutletSpeed=coarse.solver.MeanOutletSpeed(),
                FineOutletSpeed=straight.solver.MeanOutletSpeed()
            };
        }

        private static (D2Q9LbmSolver solver, PipePathModel path) RunStraight(int width,int height,int iterations)
        {
            var solver=new D2Q9LbmSolver(width,height,1.82,.05);
            var path=new PipePathModel();
            path.SetPreset(new[]{new Vector2(.02f,.5f),new Vector2(.34f,.5f),new Vector2(.66f,.5f),new Vector2(.98f,.5f)},.09f);
            PipeSolverAdapter.Apply(solver,path);
            solver.Step(iterations);
            return (solver,path);
        }

        private static double RunPipeLoss(Vector2[] points,int iterations)
        {
            var solver=new D2Q9LbmSolver(160,90,1.82,.05);
            var path=new PipePathModel();path.SetPreset(points,.09f);PipeSolverAdapter.Apply(solver,path);solver.Step(iterations);
            return Math.Abs(solver.MeanDensityAtColumn(1)-solver.MeanDensityAtColumn(solver.Width-2));
        }
    }
}
