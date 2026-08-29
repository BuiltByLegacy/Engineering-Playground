using System;
using System.Collections.Generic;
using EngineeringPlayground.Core.Content;
using Newtonsoft.Json.Linq;

namespace EngineeringPlayground.Flow.Challenges
{
    public readonly struct FlowChallengeMetrics
    {
        public FlowChallengeMetrics(double outletSpeed, double pressureLoss, double meanVorticity, int addedSolidCells)
        {
            OutletSpeed = outletSpeed;
            PressureLoss = pressureLoss;
            MeanVorticity = meanVorticity;
            AddedSolidCells = Math.Max(0, addedSolidCells);
        }

        public double OutletSpeed { get; }
        public double PressureLoss { get; }
        public double MeanVorticity { get; }
        public int AddedSolidCells { get; }
    }

    public sealed class FlowChallengeResult
    {
        public double Score { get; set; }
        public bool Passed { get; set; }
        public string Grade { get; set; } = "D";
        public int Stars { get; set; }
        public IReadOnlyDictionary<string, double> DimensionScores { get; set; } = new Dictionary<string, double>();
        public IReadOnlyList<string> Feedback { get; set; } = Array.Empty<string>();
    }

    public static class FlowChallengeScorer
    {
        public static FlowChallengeResult Evaluate(ChallengeDefinition challenge, FlowChallengeMetrics metrics)
        {
            if (challenge == null) throw new ArgumentNullException(nameof(challenge));

            var targets = challenge.DomainConfig["targets"] as JObject ?? new JObject();
            var targetOutlet = ReadDouble(targets, "outlet_speed", Math.Max(1e-9, ReadDouble(challenge.SuccessConditions, "min_outlet_speed", 0.06)));
            var targetPressure = ReadDouble(targets, "max_pressure_loss", Math.Max(1e-9, ReadDouble(challenge.SuccessConditions, "max_pressure_loss", 0.02)));
            var targetVorticity = ReadDouble(targets, "max_vorticity", 0.01);
            var materialBudget = Math.Max(1.0, ReadDouble(challenge.Constraints, "max_added_solid_cells", 1.0));

            var dimensions = new Dictionary<string, double>
            {
                ["flow"] = Clamp100(100.0 * SafeRatio(metrics.OutletSpeed, targetOutlet)),
                ["pressure"] = Clamp100(100.0 * (1.0 - SafeRatio(metrics.PressureLoss, targetPressure))),
                ["turbulence"] = Clamp100(100.0 * (1.0 - SafeRatio(metrics.MeanVorticity, targetVorticity))),
                ["material"] = Clamp100(100.0 * (1.0 - metrics.AddedSolidCells / materialBudget))
            };

            var weighted = 0.0;
            var totalWeight = 0.0;
            foreach (var property in challenge.ScoringWeights.Properties())
            {
                if (!dimensions.TryGetValue(property.Name, out var dimensionScore))
                    continue;
                var weight = property.Value.Value<double>();
                if (weight <= 0.0 || !double.IsFinite(weight))
                    continue;
                weighted += dimensionScore * weight;
                totalWeight += weight;
            }

            var score = totalWeight > 0.0 ? weighted / totalWeight : 0.0;
            score = Math.Round(Clamp100(score), 1);

            var minOutlet = ReadDouble(challenge.SuccessConditions, "min_outlet_speed", double.NegativeInfinity);
            var maxPressure = ReadDouble(challenge.SuccessConditions, "max_pressure_loss", double.PositiveInfinity);
            var maxVorticity = ReadDouble(challenge.SuccessConditions, "max_vorticity", double.PositiveInfinity);
            var minimumScore = ReadDouble(challenge.SuccessConditions, "minimum_score", 0.0);

            var passed = metrics.OutletSpeed >= minOutlet
                         && metrics.PressureLoss <= maxPressure
                         && metrics.MeanVorticity <= maxVorticity
                         && score >= minimumScore;

            var feedback = BuildFeedback(dimensions);
            var grade = Grade(score);
            var stars = passed ? StarsForScore(challenge, score) : 0;

            return new FlowChallengeResult
            {
                Score = score,
                Passed = passed,
                Grade = grade,
                Stars = stars,
                DimensionScores = dimensions,
                Feedback = feedback
            };
        }

        private static IReadOnlyList<string> BuildFeedback(IReadOnlyDictionary<string, double> dimensions)
        {
            var feedback = new List<string>();
            var weakestName = string.Empty;
            var weakestScore = double.PositiveInfinity;
            foreach (var pair in dimensions)
            {
                if (pair.Value < weakestScore)
                {
                    weakestName = pair.Key;
                    weakestScore = pair.Value;
                }
            }

            feedback.Add($"Flow {Label(dimensions["flow"])} · Pressure {Label(dimensions["pressure"])} · Swirl {Label(dimensions["turbulence"])}");
            feedback.Add(weakestName switch
            {
                "flow" => "Try opening or smoothing the flow path.",
                "pressure" => "Reduce sharp restrictions and abrupt turns.",
                "turbulence" => "Smooth transitions and reduce sudden direction changes.",
                "material" => "Trim geometry that is not helping the design.",
                _ => "Keep iterating on the weakest part of the design."
            });
            return feedback;
        }

        private static int StarsForScore(ChallengeDefinition challenge, double score)
        {
            if (challenge.Rewards["target_scores"] is not JArray targets || targets.Count == 0)
                return 1;

            var stars = 0;
            for (var i = 0; i < Math.Min(3, targets.Count); i++)
            {
                var target = targets[i];
                if (target != null && score >= target.Value<double>())
                    stars = i + 1;
            }
            return Math.Max(1, stars);
        }

        private static string Grade(double score) => score switch
        {
            >= 90.0 => "S",
            >= 80.0 => "A",
            >= 70.0 => "B",
            >= 60.0 => "C",
            _ => "D"
        };

        private static string Label(double score) => score switch
        {
            >= 85.0 => "GREAT",
            >= 70.0 => "GOOD",
            >= 50.0 => "OK",
            _ => "LOW"
        };

        private static double ReadDouble(JObject source, string key, double fallback)
        {
            var token = source[key];
            if (token == null || token.Type == JTokenType.Null)
                return fallback;
            var value = token.Value<double>();
            return double.IsFinite(value) ? value : fallback;
        }

        private static double SafeRatio(double numerator, double denominator) =>
            denominator > 1e-12 ? numerator / denominator : 0.0;

        private static double Clamp100(double value) => Math.Clamp(value, 0.0, 100.0);
    }
}
