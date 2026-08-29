using EngineeringPlayground.Flow.Simulation;
using NUnit.Framework;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class D2Q9LbmSolverTests
    {
        [Test]
        public void DefaultInletVelocity_RespectsLowMachGuardrail()
        {
            var solver = new D2Q9LbmSolver(96, 54);
            Assert.That(solver.InletMachNumber, Is.LessThanOrEqualTo(0.1));
            Assert.That(solver.IsLowMach(), Is.True);
        }

        [Test]
        public void InvalidOmega_IsRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new D2Q9LbmSolver(96, 54, relaxationOmega: 2.0));
        }

        [Test]
        public void StraightChannel_RemainsFinite()
        {
            var solver = new D2Q9LbmSolver(96, 54);
            solver.ClearInteriorSolids();
            solver.Step(500);
            Assert.That(solver.IsFinite(), Is.True);
            Assert.That(solver.MeanOutletSpeed(), Is.GreaterThan(0.0));
        }

        [Test]
        public void CylinderObstacle_IncreasesVorticityRelativeToStraightChannel()
        {
            var straight = new D2Q9LbmSolver(96, 54);
            straight.ClearInteriorSolids();
            straight.Step(800);
            var straightVorticity = straight.MeanAbsoluteVorticity();

            var obstacle = new D2Q9LbmSolver(96, 54);
            obstacle.ClearInteriorSolids();
            obstacle.AddCircularObstacle(50, 27, 6);
            obstacle.Step(800);
            var obstacleVorticity = obstacle.MeanAbsoluteVorticity();

            Assert.That(obstacle.IsFinite(), Is.True);
            Assert.That(obstacleVorticity, Is.GreaterThan(straightVorticity));
        }

        [Test]
        public void MassFluxError_CanBeMeasuredForBenchmarking()
        {
            var solver = new D2Q9LbmSolver(96, 54);
            solver.ClearInteriorSolids();
            solver.Step(800);

            var error = solver.RelativeMassFluxError();
            Assert.That(error, Is.GreaterThanOrEqualTo(0.0));
            Assert.That(double.IsFinite(error), Is.True);
        }

        [Test]
        public void HigherInletVelocity_ProducesHigherOutletSpeedInSameGeometry()
        {
            var slow = new D2Q9LbmSolver(96, 54, inletVelocity: 0.03);
            slow.ClearInteriorSolids();
            slow.Step(600);

            var fast = new D2Q9LbmSolver(96, 54, inletVelocity: 0.06);
            fast.ClearInteriorSolids();
            fast.Step(600);

            Assert.That(fast.MeanOutletSpeed(), Is.GreaterThan(slow.MeanOutletSpeed()));
        }
    }
}
