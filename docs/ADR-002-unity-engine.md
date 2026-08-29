# ADR-002 — Production game engine: Unity

## Status
Accepted. Supersedes the engine decision in ADR-001.

## Context
Engineering Playground is a mobile game with multiple simulation-heavy playgrounds. The early Flow Lab prototype was built in Godot to validate interaction, solver, challenge, scoring, progression, sandbox, and showcase concepts.

The production game will instead be built in Unity.

The product needs:
- iOS and Android delivery
- strong touch-first 2D/2.5D game UX
- mature mobile profiling and build tooling
- C# for shared numerical/gameplay code
- a practical path from CPU numerical baselines to Burst/Jobs or compute-shader acceleration
- robust automated testing for engineering reference math and solver guardrails
- room for future Mechanics, MBD, Manufacturing, Thermal, Electrical, and Project playgrounds

## Decision
Use **Unity + C#** as the production runtime for Engineering Playground.

Numerical cores must remain as engine-independent C# where practical. Unity-specific rendering, scenes, UI, input, audio, haptics, lifecycle, and platform services should wrap those cores rather than be embedded into them.

Flow Lab keeps D2Q9 LBM as the first visual-solver baseline, but the production implementation lives in C# under `Assets/Scripts/Flow/Simulation`.

## Production architecture

```text
Unity App
├── Presentation
│   ├── scenes
│   ├── UI
│   ├── touch input
│   ├── particles / field rendering
│   ├── audio / haptics
│   └── mobile lifecycle
├── Shared Game Core
│   ├── challenge lifecycle
│   ├── campaign
│   ├── progression
│   ├── Learn
│   ├── scoring
│   └── telemetry
└── Engineering Engines
    ├── Flow
    │   ├── D2Q9 visual solver
    │   └── engineering reference math
    ├── Mechanics
    ├── MBD / Product Definition
    ├── Manufacturing
    ├── Thermal
    └── Electrical
```

## Numerical-code rule

Prefer plain C# classes for:
- equations
- solvers
- scoring math
- benchmark calculations
- validation
- deterministic content evaluation

Do not make a numerical class inherit from `MonoBehaviour` unless it genuinely needs Unity lifecycle behavior.

This keeps calculations:
- testable in EditMode
- deterministic
- easier to profile
- easier to move to Burst/Jobs later
- easier to compare against a compute-shader implementation

## Flow strategy

### Visual solver
Use the CPU C# D2Q9 solver as the correctness baseline.

Possible acceleration path:
1. plain C# baseline
2. optimize allocation/data layout
3. Burst/Jobs where beneficial
4. compute shader only if mobile device testing justifies it

Accelerated versions must reproduce benchmark trends and stay within accepted error bands before replacing the baseline.

### Reference math
Keep dimensional engineering calculations independent from lattice units.

Never silently convert LBM density/velocity values into real PSI, Pa, GPM, CFM, horsepower, or temperature.

Dimensional cards come from explicit scenario assumptions and classical equations.

## Existing Godot prototype

The existing `project.godot`, `scenes/`, and `src/*.gd` files are retained temporarily as migration/reference material.

They are **not the production runtime** after this ADR.

New production feature work should go into Unity/C# unless the work is explicitly documenting or extracting behavior from the prototype.

Once Unity has feature parity for the retained systems, the Godot runtime can be archived or removed in a dedicated cleanup task.

## Orientation
- hub/navigation may use portrait where useful
- simulation workspaces remain landscape-first
- tablets should receive denser engineering UI rather than simply stretching phone layouts

## Persistence
Local/offline progression remains the MVP baseline. Cloud identity/sync is a later shared service.

## Testing
Use Unity Test Framework for:
- EditMode numerical/unit tests
- PlayMode integration tests for scenes/input/UI where needed

Engineering equations and solver guardrails should be covered by deterministic EditMode tests.

## Consequences

### Positive
- C# numerical stack
- mature mobile game ecosystem
- strong profiling/tooling
- good path to Burst/Jobs/GPU compute
- easier long-term hiring and multi-developer scaling
- suitable for future 2D/3D engineering playgrounds

### Tradeoffs
- larger engine/runtime footprint than the Godot prototype
- project migration is required
- mobile GPU paths still vary by device
- Unity version/package upgrades require disciplined project management

## Guardrails
1. Unity is the production engine.
2. Keep numerical truth separate from rendering.
3. Do not require GPU compute for MVP correctness.
4. CPU reference solver remains the validation baseline.
5. Profile on real mobile hardware before increasing resolution/features.
6. Do not describe Flow Lab as validated professional CFD.
7. Do not let engine migration silently change scoring or challenge semantics without regression tests.
