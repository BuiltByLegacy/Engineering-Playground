using System.Globalization;
using EngineeringPlayground.Flow.Engineering;

namespace EngineeringPlayground.Flow.Showcases
{
    public static class FlowReferenceEstimateFormatter
    {
        public static string Format(FlowShowcaseReferenceAssumption assumption)
        {
            var result = assumption.Evaluate();
            var flowLMin = result.VolumetricFlowM3PerS * 60000.0;
            var dynamicKPa = result.DynamicPressurePa / 1000.0;
            var majorKPa = result.MajorLossPa / 1000.0;
            var minorKPa = result.MinorLossPa / 1000.0;
            var totalKPa = result.TotalLossPa / 1000.0;
            var diameterMm = assumption.DiameterM * 1000.0;

            return string.Join("\n", new[]
            {
                "REFERENCE ESTIMATE — CLASSICAL 1D",
                $"Assumptions: {assumption.FluidLabel} · {assumption.GeometryLabel} · Dₕ {F(diameterMm, 0)} mm · L {F(assumption.LengthM, 1)} m · V {F(assumption.VelocityMPerS, 2)} m/s",
                $"Q {F(flowLMin, 1)} L/min · Re {F(result.ReynoldsNumber, 0)} · {result.Regime} · Darcy f {F(result.FrictionFactor, 3)}",
                $"Dynamic pressure {F(dynamicKPa, 2)} kPa · Major loss {F(majorKPa, 2)} kPa · Minor loss {F(minorKPa, 2)} kPa · Total ΔP {F(totalKPa, 2)} kPa",
                assumption.FidelityNote,
                "Reference estimate from stated assumptions; it is not a conversion of lattice units and is not a professional engineering result."
            });
        }

        private static string F(double value, int decimals) =>
            value.ToString($"F{decimals}", CultureInfo.InvariantCulture);
    }
}
