using System;

namespace EngineeringPlayground.Flow.Engineering
{
    public enum FlowRegime
    {
        Invalid,
        Laminar,
        Transitional,
        Turbulent
    }

    public readonly struct FlowReferenceResult
    {
        public FlowReferenceResult(
            double areaM2,
            double volumetricFlowM3PerS,
            double massFlowKgPerS,
            double dynamicPressurePa,
            double reynoldsNumber,
            FlowRegime regime,
            double frictionFactor,
            double majorLossPa,
            double minorLossPa)
        {
            AreaM2 = areaM2;
            VolumetricFlowM3PerS = volumetricFlowM3PerS;
            MassFlowKgPerS = massFlowKgPerS;
            DynamicPressurePa = dynamicPressurePa;
            ReynoldsNumber = reynoldsNumber;
            Regime = regime;
            FrictionFactor = frictionFactor;
            MajorLossPa = majorLossPa;
            MinorLossPa = minorLossPa;
        }

        public double AreaM2 { get; }
        public double VolumetricFlowM3PerS { get; }
        public double MassFlowKgPerS { get; }
        public double DynamicPressurePa { get; }
        public double ReynoldsNumber { get; }
        public FlowRegime Regime { get; }
        public double FrictionFactor { get; }
        public double MajorLossPa { get; }
        public double MinorLossPa { get; }
        public double TotalLossPa => MajorLossPa + MinorLossPa;
    }

    public static class FlowEngineeringReferenceModel
    {
        public const double LaminarUpperReynolds = 2300.0;
        public const double TurbulentLowerReynolds = 4000.0;

        public static double CircularArea(double diameterM)
        {
            RequirePositive(diameterM, nameof(diameterM));
            return Math.PI * diameterM * diameterM / 4.0;
        }

        public static double HydraulicDiameterRectangular(double widthM, double heightM)
        {
            RequirePositive(widthM, nameof(widthM));
            RequirePositive(heightM, nameof(heightM));
            return 2.0 * widthM * heightM / (widthM + heightM);
        }

        public static double VolumetricFlow(double areaM2, double velocityMPerS)
        {
            RequirePositive(areaM2, nameof(areaM2));
            RequireNonNegative(velocityMPerS, nameof(velocityMPerS));
            return areaM2 * velocityMPerS;
        }

        public static double MassFlow(double densityKgPerM3, double areaM2, double velocityMPerS)
        {
            RequirePositive(densityKgPerM3, nameof(densityKgPerM3));
            return densityKgPerM3 * VolumetricFlow(areaM2, velocityMPerS);
        }

        public static double DynamicPressure(double densityKgPerM3, double velocityMPerS)
        {
            RequirePositive(densityKgPerM3, nameof(densityKgPerM3));
            RequireNonNegative(velocityMPerS, nameof(velocityMPerS));
            return 0.5 * densityKgPerM3 * velocityMPerS * velocityMPerS;
        }

        public static double ReynoldsNumber(
            double densityKgPerM3,
            double velocityMPerS,
            double hydraulicDiameterM,
            double dynamicViscosityPaS)
        {
            RequirePositive(densityKgPerM3, nameof(densityKgPerM3));
            RequireNonNegative(velocityMPerS, nameof(velocityMPerS));
            RequirePositive(hydraulicDiameterM, nameof(hydraulicDiameterM));
            RequirePositive(dynamicViscosityPaS, nameof(dynamicViscosityPaS));
            return densityKgPerM3 * velocityMPerS * hydraulicDiameterM / dynamicViscosityPaS;
        }

        public static FlowRegime ClassifyRegime(double reynoldsNumber)
        {
            if (!double.IsFinite(reynoldsNumber) || reynoldsNumber < 0.0)
                return FlowRegime.Invalid;
            if (reynoldsNumber < LaminarUpperReynolds)
                return FlowRegime.Laminar;
            if (reynoldsNumber < TurbulentLowerReynolds)
                return FlowRegime.Transitional;
            return FlowRegime.Turbulent;
        }

        public static double DarcyFrictionFactor(double reynoldsNumber, double relativeRoughness)
        {
            RequirePositive(reynoldsNumber, nameof(reynoldsNumber));
            RequireNonNegative(relativeRoughness, nameof(relativeRoughness));

            if (reynoldsNumber < LaminarUpperReynolds)
                return 64.0 / reynoldsNumber;

            // Haaland explicit approximation to the Colebrook relation.
            var term = Math.Pow(relativeRoughness / 3.7, 1.11) + 6.9 / reynoldsNumber;
            var inverseSqrtF = -1.8 * Math.Log10(term);
            return 1.0 / (inverseSqrtF * inverseSqrtF);
        }

        public static double DarcyWeisbachPressureLoss(
            double frictionFactor,
            double lengthM,
            double hydraulicDiameterM,
            double densityKgPerM3,
            double velocityMPerS)
        {
            RequireNonNegative(frictionFactor, nameof(frictionFactor));
            RequireNonNegative(lengthM, nameof(lengthM));
            RequirePositive(hydraulicDiameterM, nameof(hydraulicDiameterM));
            return frictionFactor * (lengthM / hydraulicDiameterM) * DynamicPressure(densityKgPerM3, velocityMPerS);
        }

        public static double MinorPressureLoss(double lossCoefficientK, double densityKgPerM3, double velocityMPerS)
        {
            RequireNonNegative(lossCoefficientK, nameof(lossCoefficientK));
            return lossCoefficientK * DynamicPressure(densityKgPerM3, velocityMPerS);
        }

        public static double BernoulliStaticPressureChangeNoLoss(
            double densityKgPerM3,
            double upstreamVelocityMPerS,
            double downstreamVelocityMPerS,
            double upstreamElevationM = 0.0,
            double downstreamElevationM = 0.0,
            double gravityMPerS2 = 9.80665)
        {
            RequirePositive(densityKgPerM3, nameof(densityKgPerM3));
            RequireNonNegative(upstreamVelocityMPerS, nameof(upstreamVelocityMPerS));
            RequireNonNegative(downstreamVelocityMPerS, nameof(downstreamVelocityMPerS));
            RequirePositive(gravityMPerS2, nameof(gravityMPerS2));

            var kinetic = 0.5 * densityKgPerM3 *
                          (upstreamVelocityMPerS * upstreamVelocityMPerS - downstreamVelocityMPerS * downstreamVelocityMPerS);
            var elevation = densityKgPerM3 * gravityMPerS2 *
                            (upstreamElevationM - downstreamElevationM);
            return kinetic + elevation;
        }

        public static FlowReferenceResult EvaluateCircularRun(
            double diameterM,
            double lengthM,
            double velocityMPerS,
            double densityKgPerM3,
            double dynamicViscosityPaS,
            double absoluteRoughnessM,
            double aggregateLossCoefficientK)
        {
            RequirePositive(diameterM, nameof(diameterM));
            RequireNonNegative(lengthM, nameof(lengthM));
            RequireNonNegative(absoluteRoughnessM, nameof(absoluteRoughnessM));
            RequireNonNegative(aggregateLossCoefficientK, nameof(aggregateLossCoefficientK));

            var area = CircularArea(diameterM);
            var q = VolumetricFlow(area, velocityMPerS);
            var massFlow = MassFlow(densityKgPerM3, area, velocityMPerS);
            var dynamicPressure = DynamicPressure(densityKgPerM3, velocityMPerS);
            var reynolds = ReynoldsNumber(densityKgPerM3, velocityMPerS, diameterM, dynamicViscosityPaS);
            var relativeRoughness = absoluteRoughnessM / diameterM;
            var frictionFactor = reynolds > 0.0 ? DarcyFrictionFactor(reynolds, relativeRoughness) : 0.0;
            var major = DarcyWeisbachPressureLoss(frictionFactor, lengthM, diameterM, densityKgPerM3, velocityMPerS);
            var minor = MinorPressureLoss(aggregateLossCoefficientK, densityKgPerM3, velocityMPerS);

            return new FlowReferenceResult(
                area,
                q,
                massFlow,
                dynamicPressure,
                reynolds,
                ClassifyRegime(reynolds),
                frictionFactor,
                major,
                minor);
        }

        private static void RequirePositive(double value, string name)
        {
            if (!double.IsFinite(value) || value <= 0.0)
                throw new ArgumentOutOfRangeException(name, value, "Value must be finite and greater than zero.");
        }

        private static void RequireNonNegative(double value, string name)
        {
            if (!double.IsFinite(value) || value < 0.0)
                throw new ArgumentOutOfRangeException(name, value, "Value must be finite and non-negative.");
        }
    }
}
