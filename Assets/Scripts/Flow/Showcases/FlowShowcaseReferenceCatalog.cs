using System;
using System.Collections.Generic;
using EngineeringPlayground.Flow.Engineering;

namespace EngineeringPlayground.Flow.Showcases
{
    public sealed class FlowShowcaseReferenceAssumption
    {
        public string ShowcaseId { get; set; } = string.Empty;
        public string FluidLabel { get; set; } = string.Empty;
        public string GeometryLabel { get; set; } = string.Empty;
        public double DiameterM { get; set; }
        public double LengthM { get; set; }
        public double VelocityMPerS { get; set; }
        public double DensityKgPerM3 { get; set; }
        public double DynamicViscosityPaS { get; set; }
        public double AbsoluteRoughnessM { get; set; }
        public double AggregateLossCoefficientK { get; set; }
        public string FidelityNote { get; set; } = string.Empty;

        public FlowReferenceResult Evaluate() => FlowEngineeringReferenceModel.EvaluateCircularRun(
            DiameterM,
            LengthM,
            VelocityMPerS,
            DensityKgPerM3,
            DynamicViscosityPaS,
            AbsoluteRoughnessM,
            AggregateLossCoefficientK);
    }

    public static class FlowShowcaseReferenceCatalog
    {
        private static readonly IReadOnlyDictionary<string, FlowShowcaseReferenceAssumption> Entries =
            new Dictionary<string, FlowShowcaseReferenceAssumption>(StringComparer.Ordinal)
            {
                ["plumbing"] = new()
                {
                    ShowcaseId = "plumbing",
                    FluidLabel = "Water near room temperature",
                    GeometryLabel = "19 mm equivalent pipe",
                    DiameterM = 0.019,
                    LengthM = 8.0,
                    VelocityMPerS = 1.50,
                    DensityKgPerM3 = 998.0,
                    DynamicViscosityPaS = 1.002e-3,
                    AbsoluteRoughnessM = 1.5e-6,
                    AggregateLossCoefficientK = 4.0,
                    FidelityNote = "Illustrative single-path water calculation; not a fixture-demand, elevation-head, sizing, or plumbing-code model."
                },
                ["exhaust"] = new()
                {
                    ShowcaseId = "exhaust",
                    FluidLabel = "Simplified gas reference",
                    GeometryLabel = "63.5 mm equivalent tube",
                    DiameterM = 0.0635,
                    LengthM = 3.5,
                    VelocityMPerS = 25.0,
                    DensityKgPerM3 = 0.90,
                    DynamicViscosityPaS = 2.2e-5,
                    AbsoluteRoughnessM = 4.5e-5,
                    AggregateLossCoefficientK = 5.0,
                    FidelityNote = "Steady incompressible teaching estimate only; no exhaust pulses, temperature field, acoustics, scavenging, RPM, catalyst, or power prediction."
                },
                ["hvac"] = new()
                {
                    ShowcaseId = "hvac",
                    FluidLabel = "Air near room conditions",
                    GeometryLabel = "300 mm equivalent hydraulic diameter",
                    DiameterM = 0.300,
                    LengthM = 10.0,
                    VelocityMPerS = 4.0,
                    DensityKgPerM3 = 1.20,
                    DynamicViscosityPaS = 1.81e-5,
                    AbsoluteRoughnessM = 9.0e-5,
                    AggregateLossCoefficientK = 3.0,
                    FidelityNote = "Equivalent single-path duct estimate; not a room-by-room commissioning, fan-curve, leakage, or complete duct-network calculation."
                },
                ["manifold"] = new()
                {
                    ShowcaseId = "manifold",
                    FluidLabel = "Air-like bench reference",
                    GeometryLabel = "50 mm equivalent runner",
                    DiameterM = 0.050,
                    LengthM = 0.80,
                    VelocityMPerS = 12.0,
                    DensityKgPerM3 = 1.20,
                    DynamicViscosityPaS = 1.81e-5,
                    AbsoluteRoughnessM = 4.5e-5,
                    AggregateLossCoefficientK = 2.5,
                    FidelityNote = "Single equivalent-runner teaching estimate; it does not calculate independent outlet flows or flow-bench-equivalent distribution."
                }
            };

        public static FlowShowcaseReferenceAssumption Get(string showcaseId)
        {
            if (!Entries.TryGetValue(showcaseId, out var assumption))
                throw new KeyNotFoundException($"Reference assumptions not found for showcase: {showcaseId}");
            return assumption;
        }

        public static IEnumerable<FlowShowcaseReferenceAssumption> All => Entries.Values;
    }
}
