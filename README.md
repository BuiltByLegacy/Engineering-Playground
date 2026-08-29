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
- **Showcase** — applied plumbing, exhaust, HVAC, and manifold scenarios with scenario-specific solver geometry, contextual packaging overlays, normalized gameplay scoring, and clearly separated classical **Reference estimate** engineering math.

The MVP deliberately prioritizes **fast, visually understandable iteration** over professional CFD accuracy.

## Product architecture

```text
Engineering Playground
├── Unity Presentation
│   ├── scenes / UI
│   ├── touch input
│   ├── particles / field rendering
│   ├── showcase packaging overlays
│   ├── audio / haptics
│   └── mobile lifecycle
├── Shared Game Core
│   ├── challenge lifecycle
│   ├── campaign catalog
│   ├── showcase catalog
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
        ShowcaseCatalog.cs
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
      Showcases/
        FlowShowcaseGeometryPresets.cs
        ShowcasePackagingOverlay.cs
        FlowShowcaseReferenceCatalog.cs
        FlowReferenceEstimateFormatter.cs
        FlowShowcaseSession.cs
      Simulation/
        D2Q9LbmSolver.cs
  Tests/
    EditMode/
      FlowEngineeringReferenceModelTests.cs
      D2Q9LbmSolverTests.cs
      ContentMigrationTests.cs
      FlowChallengeScorerTests.cs
      LearnCatalogTests.cs
      FlowShowcaseTests.cs

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
- **Challenge mode** campaign runtime for all 30 authored levels
- challenge scoring, grades, stars, best scores, concept unlocks, PREV/NEXT progression, sequential completion, and chapter star gates
- **Sandbox mode** using the same solver/editor/visualizer with a blank channel, no objectives, and no scoring
- **Learn mode** with all nine retained concepts, separate Explorer/Engineer wording, browsing, and persisted presentation preference
- **Showcase mode** with PREV/NEXT navigation across Fix the Shower, Build the Exhaust, Balance the HVAC, and Design the Manifold
- four deterministic, resolution-relative Showcase solid-mask presets instead of a shared `default_channel`
- scenario-specific plumbing, vehicle-underbody, HVAC-distribution, and manifold-lane geometry proxies
- context-only UGUI packaging overlays with house, vehicle, room, and manifold cues and scenario labels
- packaging overlays are raycast-free, hidden outside Showcase, and do not participate in solver physics
- unknown Showcase geometry IDs fail instead of silently falling back to the default channel
- standalone showcase challenge parsing and Unity-relative showcase content paths
- normalized Showcase gameplay scoring kept separate from campaign progression
- per-showcase physical assumption sets feeding the #15 classical reference-math engine
- dedicated **Reference estimate** display for flow rate, Reynolds number, flow regime, Darcy friction factor, dynamic pressure, major loss, minor loss, and total estimated pressure loss
- explicit player-facing warning that the dimensional result is **not a conversion of lattice units** and **not a professional engineering result**
- temporary CHALLENGE / SANDBOX / LEARN / SHOWCASE runtime mode switching
- EditMode regression-test source for campaign content, challenge scoring, Learn parity, Showcase parsing/path normalization, geometry-preset uniqueness/boundary safety, and reference-estimate finiteness/guardrails

The bootstrap UI and vector-style packaging overlays are migration scaffolding, not final product art. Authored Unity scenes/prefabs should replace them after the first clean import and parity run.

**Important:** source code and test definitions are committed, but a clean Unity import, Test Framework run, mobile build, and device test have not yet been recorded. Do not treat those acceptance items as passed until there is execution evidence.

## Flow Lab modes currently represented in Unity source

### Challenge

The Unity runtime consumes the existing 30-level campaign and preserves authored scoring semantics. Passing a challenge persists completion, best score, best grade, stars, and concept unlocks. NEXT requires the current level to be completed and chapter star gates remain enforced.

### Sandbox

Sandbox starts from a blank channel and keeps the same drawing, erase, pan, undo/redo, field views, reset, and run/pause behavior as Challenge mode. It intentionally has no pass/fail state and no score.

### Learn

Learn reads unlocked concept IDs directly from `PlayerProgressStore`, so Challenge discoveries automatically appear in the library. The nine retained concepts are Flow Rate, Pressure, Velocity, Restriction, Vortices & Recirculation, Pressure Loss, Flow Balance, Bernoulli Principle, and Reynolds Number. The MODE control switches persisted Explorer/Engineer explanation depth.

### Showcase

Showcase cycles the four applied Flow scenarios with the same solver/editor/visualizer and normalized gameplay scorer. Showcase scores do **not** write into campaign progression.

The four scenarios now start from distinct solver environments:

- **Fix the Shower** — `showcase_residential_plumbing`: staggered house/service-space obstacles create a constrained routing problem, with a house/floor/chase overlay and SUPPLY / SINK / UPSTAIRS SHOWER context.
- **Build the Exhaust** — `showcase_automotive_exhaust`: floor-pan, drivetrain, and rear-suspension obstruction zones create an underbody routing problem, with ENGINE / CHASSIS / REAR EXIT context.
- **Balance the HVAC** — `showcase_hvac_distribution`: symmetric plenum/room-side obstruction zones create a distribution proxy, with AIR HANDLER and ROOM A/B/C context.
- **Design the Manifold** — `showcase_manifold_optimization`: a plenum obstruction plus three separators creates four visible flow passages, with INLET / OUTLET 1–4 context.

The **solver mask** is the actual 2D numerical domain. The packaging overlay is only a visual explanation layer. It remains fixed while the player edits the solid mask and is never treated as a physical CFD boundary.

The HVAC and manifold layouts remain educational proxies. The current LBM implementation still uses one right-side solver outlet boundary; it does not yet measure independent branch flows.

Each Showcase also displays a separate **Reference estimate — Classical 1D** card. The reference card is calculated from explicit, scenario-specific physical assumptions rather than converting the 2D lattice simulation to engineering units. Current cards can show:

- assumed fluid and equivalent geometry
- hydraulic/equivalent diameter
- reference velocity
- volumetric flow
- Reynolds number and regime
- Darcy friction factor
- dynamic pressure
- major Darcy-Weisbach loss
- minor `K` loss
- total estimated pressure loss
- scenario-specific fidelity warning

Current assumption sets are intentionally illustrative:

- **Fix the Shower:** room-temperature water, 19 mm equivalent pipe, single-path estimate only.
- **Build the Exhaust:** simplified steady gas reference in a 63.5 mm equivalent tube; no pulse/scavenging/acoustic/power claims.
- **Balance the HVAC:** room-condition air through a 300 mm equivalent hydraulic diameter; not a full building duct network.
- **Design the Manifold:** air-like single-runner reference; no independent outlet-flow or flow-bench-equivalent prediction.

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

Four dedicated showcase scenarios are defined and represented in the Unity source:

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

Player-facing dimensional values are labeled **Reference estimate** unless a case has been explicitly calibrated and benchmarked.

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
7. Use CHALLENGE / SANDBOX / LEARN / SHOWCASE to exercise the source-level mode parity.
8. In SHOWCASE, confirm each scenario loads a visibly different solid-mask layout and context overlay, then compare the normalized visual/gameplay result with the independently calculated **Reference estimate** card.
9. Do not interpret lattice values as engineering units or packaging overlay graphics as solver boundaries.

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

#16 remains open until clean Unity import/test evidence, authored production UI/art, complete touch/mobile validation, and real-device validation are recorded.
