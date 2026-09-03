using System;
using EngineeringPlayground.Flow.Pipes;

namespace EngineeringPlayground.Flow.Challenges
{
    public readonly struct FlowRunSnapshot
    {
        public FlowRunSnapshot(FlowChallengeMetrics metrics, float routeLength, float curvatureCost, float minimumBendRadius, float radius, double score)
        {
            Metrics = metrics; RouteLength = routeLength; CurvatureCost = curvatureCost; MinimumBendRadius = minimumBendRadius; Radius = radius; Score = score;
        }
        public FlowChallengeMetrics Metrics { get; }
        public float RouteLength { get; }
        public float CurvatureCost { get; }
        public float MinimumBendRadius { get; }
        public float Radius { get; }
        public double Score { get; }
    }

    public sealed class FlowRunComparison
    {
        public double ScoreDelta { get; set; }
        public double OutletSpeedDelta { get; set; }
        public double PressureLossDelta { get; set; }
        public double VorticityDelta { get; set; }
        public double RouteLengthDelta { get; set; }
        public double BendRadiusDelta { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    public static class FlowRunAnalyzer
    {
        public static FlowRunSnapshot Capture(FlowChallengeMetrics metrics, PipePathModel pipe, FlowChallengeResult result)
            => new FlowRunSnapshot(metrics, pipe?.RouteLength ?? 0f, pipe?.CurvatureCost ?? 0f, pipe?.MinimumBendRadius ?? 999f, pipe?.Radius ?? 0f, result?.Score ?? 0d);

        public static FlowRunComparison Compare(FlowRunSnapshot previous, FlowRunSnapshot current)
        {
            var score = current.Score - previous.Score;
            var flow = current.Metrics.OutletSpeed - previous.Metrics.OutletSpeed;
            var loss = current.Metrics.PressureLoss - previous.Metrics.PressureLoss;
            var swirl = current.Metrics.MeanVorticity - previous.Metrics.MeanVorticity;
            var length = current.RouteLength - previous.RouteLength;
            var bend = current.MinimumBendRadius - previous.MinimumBendRadius;
            var summary = Math.Abs(score) < 0.5 ? "About the same overall. Change one design variable and run again." : score > 0 ? $"Improved {score:+0.0;-0.0} points." : $"Dropped {Math.Abs(score):0.0} points.";
            return new FlowRunComparison { ScoreDelta=score,OutletSpeedDelta=flow,PressureLossDelta=loss,VorticityDelta=swirl,RouteLengthDelta=length,BendRadiusDelta=bend,Summary=summary };
        }
    }

    public static class FlowEngineeringInsightEngine
    {
        public static string Explain(FlowRunSnapshot current, FlowRunComparison comparison = null)
        {
            if (comparison != null)
            {
                if (comparison.BendRadiusDelta > .008 && comparison.PressureLossDelta < -0.0002) return "You opened the tightest bend and pressure loss fell. Sweeping bends are easier on the flow.";
                if (comparison.BendRadiusDelta < -.008 && comparison.VorticityDelta > 0.00015) return "The tighter bend created more swirl. Give the turn more room.";
                if (comparison.RouteLengthDelta < -.02 && comparison.PressureLossDelta <= 0) return "You shortened the route without increasing loss. That is a more efficient design.";
                if (comparison.RouteLengthDelta > .02 && comparison.ScoreDelta < 0) return "The longer route used more material without enough flow benefit.";
                if (comparison.OutletSpeedDelta > .001 && comparison.ScoreDelta > 0) return "More of the flow is reaching OUT. Keep the passage smooth while protecting that gain.";
            }
            if (current.MinimumBendRadius < .075f) return "The tightest bend is dominating this design. Increase its radius before adding more route length.";
            if (current.Metrics.MeanVorticity > .012) return "Strong swirl is stealing useful motion. Smooth the sharpest direction change.";
            if (current.Metrics.OutletSpeed < .02) return "Very little flow is reaching OUT. Check for a tight restriction or blocked passage.";
            if (current.RouteLength > 1.25f) return "The route is long. Look for a shorter path that keeps the bends gentle.";
            return "The route is working. Try one deliberate change and compare the next run.";
        }
    }
}
