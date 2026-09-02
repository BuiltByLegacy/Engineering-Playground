using System;
using EngineeringPlayground.Flow.Pipes;
using EngineeringPlayground.Flow.Simulation;
using UnityEngine;

namespace EngineeringPlayground.Flow.Runtime
{
    public sealed class FlowLabRuntimeController : MonoBehaviour
    {
        [SerializeField] private int width = 96;
        [SerializeField] private int height = 54;
        [SerializeField] private double relaxationOmega = 1.82;
        [SerializeField] private double inletVelocity = 0.05;
        [SerializeField] private int solverIterationsPerFrame = 2;
        [SerializeField] private bool running = true;

        public D2Q9LbmSolver Solver { get; private set; }
        public PipePathModel PipePath { get; private set; }
        public bool Running => running;
        public event Action SolverUpdated;

        private void Awake()
        {
            Solver = new D2Q9LbmSolver(width, height, relaxationOmega, inletVelocity);
            PipePath = new PipePathModel();
            PipePath.Changed += ApplyPipePath;
            ApplyPipePreset(1);
        }

        private void OnDestroy(){if(PipePath!=null)PipePath.Changed-=ApplyPipePath;}

        private void Update()
        {
            if (!running || Solver == null) return;
            Solver.Step(Math.Max(1, solverIterationsPerFrame));
            if (!Solver.IsFinite()) { running = false; Debug.LogError("Flow solver entered a non-finite state and was paused."); return; }
            SolverUpdated?.Invoke();
        }

        public void SetRunning(bool value) => running = value;
        public void ToggleRunning() => running = !running;
        public void ResetSimulation(){Solver.Reset();SolverUpdated?.Invoke();}

        public void ApplyPipePreset(int level)
        {
            var radius = level==4 ? .075f : level==5 ? .11f : .09f;
            PipePath.SetPreset(PipePathPresets.ForLevel(level), radius);
            ApplyPipePath();
        }

        public void MovePipeHandle(int index, Vector2 normalizedPosition) => PipePath.MovePoint(index, normalizedPosition);
        public void ResetPipePath(){PipePath.ResetToPreset();ApplyPipePath();}
        private void ApplyPipePath(){if(Solver==null||PipePath==null)return;PipeSolverAdapter.Apply(Solver,PipePath);running=false;SolverUpdated?.Invoke();}

        public void ClearGeometry(){if(Solver==null)return;Solver.ClearInteriorSolids();Solver.Reset();SolverUpdated?.Invoke();}
        public void RestoreDefaultChallengeGeometry(){ApplyPipePreset(1);}
        public void ApplySolidMask(bool[] mask){if(Solver==null)return;Solver.ApplySolidMask(mask,true);SolverUpdated?.Invoke();}
        public void SetSolid(int x,int y,bool solid){Solver.SetSolid(x,y,solid);Solver.Reset();SolverUpdated?.Invoke();}
        public string GetGuardrailStatus()=>!Solver.IsLowMach()?$"Outside normal low-Mach guardrail (Ma={Solver.InletMachNumber:F3}).":$"Low-Mach guardrail OK (Ma={Solver.InletMachNumber:F3}).";
    }
}
