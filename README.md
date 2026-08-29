# Engineering Playground

**Build it. Test it. Make it better.**

Engineering Playground is a mobile-first engineering game where players learn by building, simulating, breaking, and improving systems instead of reading through a traditional lesson first.

The long-term product is **one mobile game with multiple modular playgrounds**. The first playable release is **Flow Lab**.

## Production engine

**Unity + C# is the production stack.**

The earlier Godot/GDScript implementation is retained temporarily as a prototype/reference during migration, but new production feature work should target Unity.

See:
- [`docs/ADR-002-unity-engine.md`](docs/ADR-002-unity-engine.md)
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
  Scripts/
    Flow/
      Engineering/
        FlowEngineeringReferenceModel.cs
      Simulation/
        D2Q9LbmSolver.cs
  Tests/
    EditMode/
      FlowEngineeringReferenceModelTests.cs
      D2Q9LbmSolverTests.cs

content/
  flow/
    campaign.json
    showcases.json
    challenges/
    showcases/

docs/
  ADR-002-unity-engine.md
  FLOW_ENGINEERING_VALIDITY.md
  FLOW_BENCHMARKS.md
  FLOW_SHOWCASES.md

# Legacy prototype/reference during Unity migration
project.godot
scenes/
src/**/*.gd
```

## Current implementation status

### Unity production foundation

Issue #15 now establishes the first Unity/C# engineering-validity foundation:

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
- Unity EditMode test coverage for the equations
- Unity-ready CPU D2Q9 LBM baseline in plain C#
- low-Mach guardrail
- BGK relaxation-parameter validation
- mass-flux error measurement
- finiteness detection
- vorticity and outlet-speed benchmark metrics
- initial solver regression tests
- benchmark matrix and validity-tier documentation

See [`docs/FLOW_ENGINEERING_VALIDITY.md`](docs/FLOW_ENGINEERING_VALIDITY.md) and [`docs/FLOW_BENCHMARKS.md`](docs/FLOW_BENCHMARKS.md).

### Existing gameplay/content prototype

The earlier prototype already demonstrated the product systems and content direction:

- live particle, velocity, pressure, and swirl visualization
- touch draw/erase/pan plus pinch zoom
- undo/redo and reset
- versioned JSON challenge schema
- reusable challenge lifecycle
- multi-objective Flow scoring and engineering feedback
- progression, grades, stars, and concept unlocks
- Challenge / Sandbox / Learn / Showcase concepts
- 30-level launch campaign across five chapters
- four real-world showcase scenarios

These systems now need to migrate into Unity rather than receive additional Godot-specific production work.

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

### 1. Visual simulation

D2Q9 Lattice Boltzmann simulation provides:
- geometry-driven flow behavior
- velocity-field response
- wakes
- restrictions
- recirculation/vorticity
- pressure-like qualitative field behavior

The CPU C# implementation is the correctness baseline. Future Burst/Jobs or compute-shader versions must match benchmark trends before replacing it.

### 2. Engineering reference math

Dimensional results come from explicit assumptions and classical equations, not silent conversion of lattice units.

Reference cards can show:
- fluid assumptions
- hydraulic diameter
- velocity
- flow rate
- Reynolds number
- flow regime
- friction factor
- dynamic pressure
- major loss
- minor loss
- total estimated pressure loss

Player-facing dimensional values should be labeled **Reference estimate** unless a case has been explicitly calibrated and benchmarked.

## Validity tiers

- **Qualitative** — visual directionality only.
- **Semi-quantitative** — normalized/trend metrics that pass regression testing.
- **Benchmarked quantitative reference** — dimensional classical calculations or explicitly calibrated solver outputs.

This distinction is central to the product: the game should look and behave like engineering without pretending to be a certification-grade CFD tool.

## Core game loop

> **Challenge → Design → Run → Observe → Score → Learn → Modify → Retry**

## Flow solver strategy

The first solver remains a **D2Q9 Lattice Boltzmann Method** baseline because it maps naturally to a grid, handles interactive obstacles well, and exposes fields useful for game visualization.

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

## GitHub

Primary MVP epic: **#1 — Engineering Playground Mobile MVP — Flow Lab**

Major implemented/design slices:

- **#2** Mobile foundation and architecture — original prototype decision now superseded by Unity ADR
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
- **#15** Flow engineering validity, reference math, and solver benchmarking — Unity/C# production foundation

The next broad engineering task is migration of the retained prototype systems into the Unity project while preserving their challenge/content semantics and numerical guardrails.
