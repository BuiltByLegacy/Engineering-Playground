# Engineering Playground

**Build it. Test it. Make it better.**

Engineering Playground is a mobile-first engineering game where players learn by building, simulating, breaking, and improving systems instead of reading through a traditional lesson first.

The long-term product is **one mobile game with multiple modular playgrounds**. The first playable release is **Flow Lab**.

## Production engine

**Unity 6.3 LTS + C# is the production stack.**

The repository is currently pinned to **Unity 6000.3.18f1** for the initial production migration. The earlier Godot/GDScript implementation is retained temporarily as prototype/reference material and should not receive new production features.

See:
- [`docs/ADR-002-unity-engine.md`](docs/ADR-002-unity-engine.md)
- [`docs/UNITY_MIGRATION.md`](docs/UNITY_MIGRATION.md)
- [`docs/ADR-001-mobile-stack.md`](docs/ADR-001-mobile-stack.md) — superseded engine decision

## MVP: Flow Lab

Flow Lab is built around four player-facing experiences:

- **Challenge** — solve engineering problems, score the design, improve it, and earn progression.
- **Sandbox** — free experimentation with the same editor, solver, and visualization stack but no objective or score.
- **Learn** — a discovery library that reveals engineering concepts after the player encounters them in gameplay.
- **Showcase** — polished real-world plumbing, exhaust, HVAC, and manifold scenarios.

The MVP deliberately prioritizes **fast, visually understandable iteration** over professional CFD accuracy.

## Product architecture

```text
Engineering Playground
├── Unity Presentation
│   ├── scenes / UI
│   ├── touch input
│   ├── particles / field rendering
│   ├── audio / haptics
│   └── mobile lifecycle
├── Shared Game Core
│   ├── challenge lifecycle
│   ├── campaign catalog
│   ├── progression
│   ├── Learn
│   ├── scoring
│   └── telemetry
└── Engineering Engines
    ├── Flow Lab          ← MVP
    ├── Mechanics Lab
    ├── Definition Lab    ← future MBD/GD&T
    ├── Factory Lab       ← future manufacturing
    ├── Thermal Lab
    └── Power Lab         ← future electrical
```

Numerical truth should live in plain C# where practical, separate from Unity rendering/UI.

## Repository layout

```text
Assets/
  Editor/
    EngineeringContentSync.cs
  Scripts/
    App/
      EngineeringPlaygroundBootstrap.cs
      FlowLabModeController.cs
    Core/
      Content/
        ContentModels.cs
        CampaignCatalog.cs
        ContentRepository.cs
      Learn/
        LearnCatalog.cs
      Progress/
        PlayerProgressStore.cs
    Flow/
      Challenges/
        FlowChallengeHud.cs
        FlowChallengeScorer.cs
        FlowChallengeSession.cs
      Engineering/
        FlowEngineeringReferenceModel.cs
      Runtime/
        FlowLabRuntimeController.cs
        FlowFieldVisualizer.cs
        FlowTouchEditor.cs
      Simulation/
        D2Q9LbmSolver.cs
  Tests/
    EditMode/
      FlowEngineeringReferenceModelTests.cs
      D2Q9LbmSolverTests.cs
      ContentMigrationTests.cs
      FlowChallengeScorerTests.cs
      LearnCatalogTests.cs

Packages/
ProjectSettings/

content/
  flow/
    campaign.json
    showcases.json
    challenges/
    showcases/

docs/
  ADR-002-unity-engine.md
  UNITY_MIGRATION.md
  FLOW_ENGINEERING_VALIDITY.md
  FLOW_BENCHMARKS.md
  FLOW_SHOWCASES.md

# Legacy prototype/reference during Unity migration
project.godot
scenes/
src/**/*.gd
```

## Current Unity production slice

Issue #16 now has a real source-level Unity foundation rather than only an engine decision:

- Unity 6.3 LTS project structure and version pin
- iOS/Android-oriented PlayerSettings source
- Input System, Newtonsoft JSON, Unity Test Framework, and UGUI packages
- preserved JSON campaign/challenge models
- schema validation and duplicate-ID checks
- root-content loading in Editor
- build-time synchronization into `StreamingAssets`
- regression test source asserting 30 campaign challenges and four showcase definitions
- PlayerPrefs-backed local progression, stars, best score/grade, concepts, and Explorer/Engineer preference
- plain-C# D2Q9 solver from #15
- Unity runtime solver controller
- finite-state and low-Mach runtime guardrails
- editable solid masks
- Flow / speed / pressure / vorticity field visualization
- draw / erase / pan interaction
- interpolated brush strokes
- undo / redo
- geometry changes reset the solver state so stale distributions do not leak across resets or modes
- Challenge mode campaign runtime for all 30 authored levels
- challenge scoring, grades, stars, best scores, concept unlocks, PREV/NEXT progression, sequential completion, and chapter star gates
- **Sandbox mode** using the same solver/editor/visualizer with a blank channel, no objectives, and no scoring
- **Learn mode** using the same persisted concept IDs from the prototype
- all nine Learn concepts ported with separate Explorer and Engineer wording
- Learn PREV/NEXT browsing and persisted Explorer/Engineer presentation preference
- Challenge / Sandbox / Learn mode switching in the temporary runtime bootstrap
- separate temporary mode, navigation, and tool bars for migration testing
- EditMode regression-test source for challenge scoring and Learn catalog parity

The bootstrap UI is migration scaffolding, not final product UX. Authored Unity scenes/prefabs should replace it after the first clean import and parity run.

**Important:** source code and test definitions are committed, but a clean Unity import, Test Framework run, mobile build, and device test have not yet been recorded. Do not treat those acceptance items as passed until there is execution evidence.

## Flow Lab modes currently represented in Unity source

### Challenge

The Unity runtime consumes the existing 30-level campaign and preserves authored scoring semantics. Passing a challenge persists completion, best score, best grade, stars, and concept unlocks. NEXT requires the current level to be completed and chapter star gates remain enforced.

### Sandbox

Sandbox starts from a blank channel and keeps the same drawing, erase, pan, undo/redo, field views, reset, and run/pause behavior as Challenge mode. It intentionally has no pass/fail state and no score.

### Learn

Learn reads unlocked concept IDs directly from `PlayerProgressStore`, so Challenge discoveries automatically appear in the library. The nine retained concepts are Flow Rate, Pressure, Velocity, Restriction, Vortices & Recirculation, Pressure Loss, Flow Balance, Bernoulli Principle, and Reynolds Number. The MODE control switches persisted Explorer/Engineer explanation depth.

### Showcase

Showcase remains the major unported player mode. The next parity slice should connect the four existing showcase definitions to the Unity runtime and expose #15 engineering reference math as clearly labeled **Reference estimate** output.

## Engineering-validity foundation

Issue #15 establishes the mathematical credibility layer:

- engine-independent C# reference-math model
- continuity / mass-flow calculations
- dynamic pressure
- Bernoulli reference relationship
- Reynolds number and regime classification
- Darcy-Weisbach major loss
- minor-loss coefficient model
- laminar `64/Re` friction factor
- Haaland turbulent friction-factor approximation
- invalid-input guardrails
- Unity EditMode test source for the equations
- Unity-ready CPU D2Q9 LBM baseline in plain C#
- low-Mach guardrail
- BGK relaxation-parameter validation
- mass-flux error measurement
- finiteness detection
- vorticity and outlet-speed benchmark metrics
- benchmark matrix and validity-tier documentation

See [`docs/FLOW_ENGINEERING_VALIDITY.md`](docs/FLOW_ENGINEERING_VALIDITY.md) and [`docs/FLOW_BENCHMARKS.md`](docs/FLOW_BENCHMARKS.md).

## Launch campaign

The campaign contains **30 authored levels** across five chapters:

1. **Make It Flow** — channels, obstacles, smooth transitions, restriction, wakes.
2. **Pressure** — pressure loss, contractions, expansions, velocity tradeoffs.
3. **Split It** — parallel-path and merge intuition.
4. **Control It** — geometry-based throttling and conditioning before true valve/pump components are added.
5. **Engineer It** — simplified applied plumbing, exhaust, HVAC, manifold, packaging, and capstone problems.

Each level defines success thresholds, scoring weights, concept unlocks, hints, material constraints, replay targets, domain targets, and campaign metadata.

## Applied Flow showcases

Four dedicated showcase scenarios are defined:

1. **Fix the Shower** — residential plumbing.
2. **Build the Exhaust** — automotive exhaust.
3. **Balance the HVAC** — HVAC distribution.
4. **Design the Manifold** — manifold optimization.

The current educational model intentionally does **not** claim true plumbing-network analysis, HVAC commissioning accuracy, exhaust pulse/scavenging performance, acoustic prediction, horsepower effects, or flow-bench-equivalent manifold results.

## Engineering validity model

Flow Lab uses two related but distinct layers.

### Visual simulation

D2Q9 Lattice Boltzmann simulation provides geometry-driven flow behavior, velocity-field response, wakes, restrictions, recirculation/vorticity, and pressure-like qualitative field behavior.

The CPU C# implementation is the correctness baseline. Future Burst/Jobs or compute-shader versions must match benchmark trends before replacing it.

### Engineering reference math

Dimensional results come from explicit assumptions and classical equations, not silent conversion of lattice units.

Reference cards can show fluid assumptions, hydraulic diameter, velocity, flow rate, Reynolds number, flow regime, friction factor, dynamic pressure, major loss, minor loss, and total estimated pressure loss.

Player-facing dimensional values should be labeled **Reference estimate** unless a case has been explicitly calibrated and benchmarked.

## Validity tiers

- **Qualitative** — visual directionality only.
- **Semi-quantitative** — normalized/trend metrics that pass regression testing.
- **Benchmarked quantitative reference** — dimensional classical calculations or explicitly calibrated solver outputs.

## Core game loop

> **Challenge → Design → Run → Observe → Score → Learn → Modify → Retry**

## Flow solver strategy

Production strategy:

1. plain C# CPU solver for correctness and testing
2. profile and optimize data layout
3. Burst/Jobs if beneficial
4. compute shader only when real mobile testing justifies it

The architecture leaves room for different future solver types, including 1D pipe networks, compressible/pulsating flow, thermal coupling, and other engineering domains.

## Mobile direction

- one Unity mobile game
- iOS + Android
- landscape-first engineering workspace
- portrait-capable hub/navigation where useful
- phone and tablet layouts
- offline/local progression first

## MVP exclusions

Not in the current MVP:

- professional/validated CFD
- 3D CFD
- CAD import
- FEA
- full MBD/GD&T
- manufacturing simulation
- electrical simulation
- classroom dashboards
- AI tutor
- multiplayer/community marketplace

## Running the Unity migration locally

1. Install **Unity 6.3 LTS / 6000.3.18f1** in Unity Hub.
2. Add this repository as a Unity project.
3. Allow packages and scripts to import.
4. Run EditMode tests before accepting numerical/content parity.
5. Use **Engineering Playground → Sync Content To StreamingAssets** before manual player-build inspection; normal Unity builds also run the sync automatically.
6. Open/run an empty scene: the temporary bootstrap creates the current Flow Lab migration workspace automatically.
7. Use the CHALLENGE / SANDBOX / LEARN controls to exercise the currently ported modes.

## GitHub

Primary MVP epic: **#1 — Engineering Playground Mobile MVP — Flow Lab**

Major implemented/design slices:

- **#2** Mobile foundation and architecture — original prototype decision superseded by Unity ADR
- **#3** Shared game-core systems and playground contract
- **#4** Flow solver spike
- **#5** Flow visualization
- **#6** Touch-first Flow editor
- **#7** Challenge engine and content schema
- **#8** Flow scoring, feedback, and telemetry
- **#9** Player progression, concept unlocks, and Learn mode
- **#10** Flow Lab sandbox mode
- **#11** 30-level Flow Lab launch campaign
- **#12** Applied Flow showcases
- **#15** Flow engineering validity, reference math, and solver benchmarking
- **#16** Unity migration — production project, Flow Lab parity, and mobile foundation

#16 remains open until Unity import/test evidence, Showcase parity, authored production UI, and real-device validation are complete.
