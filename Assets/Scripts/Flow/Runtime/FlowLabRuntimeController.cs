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
        public int CurrentPipeLevel { get; private set; } = 1;
        public bool Running => running;
        public bool HasFixedObstacle => CurrentPipeLevel == 2;
        public Vector2 FixedObstacleCenter => new(.52f,.50f);
        public float FixedObstacleRadius => .055f;
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
            CurrentPipeLevel = Mathf.Max(1, level);
            var radius = CurrentPipeLevel switch { 2 => .075f, 4 => .075f, 5 => .11f, _ => .09f };
            PipePath.SetPreset(PipePathPresets.ForLevel(CurrentPipeLevel), radius);
        }

        public void MovePipeHandle(int index, Vector2 normalizedPosition) => PipePath.MovePoint(index, normalizedPosition);
        public void ResetPipePath() => PipePath.ResetToPreset();

        private void ApplyPipePath()
        {
            if(Solver==null||PipePath==null)return;
            var mask=PipeSolverAdapter.BuildSolidMask(Solver,PipePath);
            if(HasFixedObstacle) AddFixedCircularObstacle(mask,FixedObstacleCenter.x,FixedObstacleCenter.y,FixedObstacleRadius);
            Solver.ApplySolidMask(mask,true);
            running=false;
            SolverUpdated?.Invoke();
        }

        private void AddFixedCircularObstacle(bool[] mask,float nx,float ny,float normalizedRadius)
        {
            var cx=Mathf.RoundToInt(nx*(Solver.Width-1));var cy=Mathf.RoundToInt(ny*(Solver.Height-1));var radius=Mathf.Max(2,Mathf.RoundToInt(normalizedRadius*Solver.Height));
            for(var y=1;y<Solver.Height-1;y++)for(var x=1;x<Solver.Width-1;x++){var dx=x-cx;var dy=y-cy;if(dx*dx+dy*dy<=radius*radius)mask[y*Solver.Width+x]=true;}
        }

        public void ClearGeometry(){if(Solver==null)return;Solver.ClearInteriorSolids();Solver.Reset();SolverUpdated?.Invoke();}
        public void RestoreDefaultChallengeGeometry(){ApplyPipePreset(1);}
        public void ApplySolidMask(bool[] mask){if(Solver==null)return;Solver.ApplySolidMask(mask,true);SolverUpdated?.Invoke();}
        public void SetSolid(int x,int y,bool solid){Solver.SetSolid(x,y,solid);Solver.Reset();SolverUpdated?.Invoke();}
        public string GetGuardrailStatus()=>!Solver.IsLowMach()?$"Outside normal low-Mach guardrail (Ma={Solver.InletMachNumber:F3}).":$"Low-Mach guardrail OK (Ma={Solver.InletMachNumber:F3}).";
    }
}
