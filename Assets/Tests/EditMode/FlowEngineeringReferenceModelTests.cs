using System;
using EngineeringPlayground.Flow.Engineering;
using NUnit.Framework;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class FlowEngineeringReferenceModelTests
    {
        [Test]
        public void CircularArea_OneMeterDiameter_IsPiOverFour()
        {
            Assert.That(
                FlowEngineeringReferenceModel.CircularArea(1.0),
                Is.EqualTo(Math.PI / 4.0).Within(1e-12));
        }

        [Test]
        public void Continuity_DoublesFlowWhenVelocityDoubles()
        {
            const double area = 0.02;
            var q1 = FlowEngineeringReferenceModel.VolumetricFlow(area, 1.0);
            var q2 = FlowEngineeringReferenceModel.VolumetricFlow(area, 2.0);
            Assert.That(q2, Is.EqualTo(2.0 * q1).Within(1e-12));
        }

        [Test]
        public void DynamicPressure_QuadruplesWhenVelocityDoubles()
        {
            const double rho = 998.2;
            var q1 = FlowEngineeringReferenceModel.DynamicPressure(rho, 1.0);
            var q2 = FlowEngineeringReferenceModel.DynamicPressure(rho, 2.0);
            Assert.That(q2, Is.EqualTo(4.0 * q1).Within(1e-9));
        }

        [Test]
        public void ReynoldsNumber_MatchesDefinition()
        {
            var re = FlowEngineeringReferenceModel.ReynoldsNumber(
                densityKgPerM3: 998.2,
                velocityMPerS: 1.5,
                hydraulicDiameterM: 0.019,
                dynamicViscosityPaS: 1.002e-3);

            var expected = 998.2 * 1.5 * 0.019 / 1.002e-3;
            Assert.That(re, Is.EqualTo(expected).Within(1e-6));
            Assert.That(FlowEngineeringReferenceModel.ClassifyRegime(re), Is.EqualTo(FlowRegime.Turbulent));
        }

        [Test]
        public void LaminarFrictionFactor_Uses64OverRe()
        {
            const double re = 1000.0;
            var f = FlowEngineeringReferenceModel.DarcyFrictionFactor(re, 0.0);
            Assert.That(f, Is.EqualTo(0.064).Within(1e-12));
        }

        [Test]
        public void TurbulentFrictionFactor_IsInPlausibleRange()
        {
            var f = FlowEngineeringReferenceModel.DarcyFrictionFactor(100_000.0, 0.0002);
            Assert.That(f, Is.GreaterThan(0.015).And.LessThan(0.04));
        }

        [Test]
        public void DarcyWeisbachLoss_IsLinearWithLength()
        {
            const double f = 0.024;
            const double d = 0.02;
            const double rho = 998.2;
            const double v = 1.5;

            var oneMeter = FlowEngineeringReferenceModel.DarcyWeisbachPressureLoss(f, 1.0, d, rho, v);
            var fiveMeters = FlowEngineeringReferenceModel.DarcyWeisbachPressureLoss(f, 5.0, d, rho, v);

            Assert.That(fiveMeters, Is.EqualTo(5.0 * oneMeter).Within(1e-9));
        }

        [Test]
        public void MinorLoss_IsLinearWithK()
        {
            const double rho = 1.204;
            const double v = 4.0;

            var k1 = FlowEngineeringReferenceModel.MinorPressureLoss(1.0, rho, v);
            var k3 = FlowEngineeringReferenceModel.MinorPressureLoss(3.0, rho, v);

            Assert.That(k3, Is.EqualTo(3.0 * k1).Within(1e-12));
        }

        [Test]
        public void BernoulliNoLoss_PredictsStaticPressureDropAsVelocityRises()
        {
            var deltaP = FlowEngineeringReferenceModel.BernoulliStaticPressureChangeNoLoss(
                densityKgPerM3: 998.2,
                upstreamVelocityMPerS: 1.0,
                downstreamVelocityMPerS: 2.0);

            Assert.That(deltaP, Is.LessThan(0.0));
        }

        [Test]
        public void EvaluateCircularRun_TotalLossEqualsMajorPlusMinor()
        {
            var result = FlowEngineeringReferenceModel.EvaluateCircularRun(
                diameterM: 0.019,
                lengthM: 8.0,
                velocityMPerS: 1.5,
                densityKgPerM3: 998.2,
                dynamicViscosityPaS: 1.002e-3,
                absoluteRoughnessM: 1.5e-6,
                aggregateLossCoefficientK: 2.5);

            Assert.That(result.TotalLossPa, Is.EqualTo(result.MajorLossPa + result.MinorLossPa).Within(1e-9));
            Assert.That(result.VolumetricFlowM3PerS, Is.GreaterThan(0.0));
            Assert.That(result.MassFlowKgPerS, Is.GreaterThan(0.0));
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void InvalidDiameter_Throws(double diameter)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FlowEngineeringReferenceModel.CircularArea(diameter));
        }
    }
}
