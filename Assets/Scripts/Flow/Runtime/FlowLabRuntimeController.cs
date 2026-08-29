using System;
using EngineeringPlayground.Flow.Simulation;
using UnityEngine;

namespace EngineeringPlayground.Flow.Runtime
{
    public sealed class FlowLabRuntimeController : MonoBehaviour
    {
        [SerializeField] private int width = 96;
        [SerializeField] private int height = 54;
        [SerializeField] private double relaxationOmega = 1.82;
        [SerializeField] private double inletVelocity = 0.065;
        [SerializeField] private int solverIterationsPerFrame = 2;
        [SerializeField] private bool running = true;

        public D2Q9LbmSolver Solver { get; private set; }
        public bool Running => running;
        public event Action SolverUpdated;

        private void Awake()
        {
            Solver = new D2Q9LbmSolver(width, height, relaxationOmega, inletVelocity);
            RestoreDefaultChallengeGeometry();
        }

        private void Update()
        {
            if (!running || Solver == null)
                return;

            Solver.Step(Math.Max(1, solverIterationsPerFrame));
            if (!Solver.IsFinite())
            {
                running = false;
                Debug.LogError("Flow solver entered a non-finite state and was paused.");
                return;
            }

            SolverUpdated?.Invoke();
        }

        public void SetRunning(bool value) => running = value;
        public void ToggleRunning() => running = !running;

        public void ResetSimulation()
        {
            Solver.Reset();
            SolverUpdated?.Invoke();
        }

        public void ClearGeometry()
        {
            if (Solver == null)
                return;

            Solver.ClearInteriorSolids();
            Solver.Reset();
            SolverUpdated?.Invoke();
        }

        public void RestoreDefaultChallengeGeometry()
        {
            if (Solver == null)
                return;

            Solver.ClearInteriorSolids();
            Solver.AddCircularObstacle(
                (int)(width * 0.52),
                height / 2,
                Math.Max(3, (int)(Math.Min(width, height) * 0.11)));
            Solver.Reset();
            SolverUpdated?.Invoke();
        }

        public void ApplySolidMask(bool[] mask)
        {
            if (Solver == null)
                return;

            Solver.ApplySolidMask(mask, true);
            SolverUpdated?.Invoke();
        }

        public void SetSolid(int x, int y, bool solid)
        {
            Solver.SetSolid(x, y, solid);
            Solver.Reset();
            SolverUpdated?.Invoke();
        }

        public string GetGuardrailStatus()
        {
            if (!Solver.IsLowMach())
                return $"Outside normal low-Mach guardrail (Ma={Solver.InletMachNumber:F3}).";
            return $"Low-Mach guardrail OK (Ma={Solver.InletMachNumber:F3}).";
        }
    }
}
