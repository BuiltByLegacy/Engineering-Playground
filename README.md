# Engineering Playground

**Build it. Test it. Make it better.**

Engineering Playground is a mobile-first engineering game where players learn by building, simulating, breaking, and improving systems instead of reading through a traditional lesson first.

The long-term product is **one mobile game with multiple modular playgrounds**. The first playable release is **Flow Lab**.

## MVP: Flow Lab

Flow Lab has three player-facing modes:

- **Challenge** — solve engineering problems, score the design, improve it, and earn progression.
- **Sandbox** — free experimentation with the same editor, solver, and visualization stack but no objective or score.
- **Learn** — a discovery library that reveals engineering concepts after the player encounters them in gameplay.

The MVP deliberately prioritizes **fast, visually understandable iteration** over professional CFD accuracy.

## Product architecture

Engineering Playground is one app, but each discipline owns its domain engine behind shared game contracts.

```text
Engineering Playground
├── Core
│   ├── playground registry
│   ├── challenge lifecycle
│   ├── campaign catalog
│   ├── player progression
│   ├── concept unlocks / Learn catalog
│   ├── scoring / telemetry
│   └── simulation adapter contract
└── Playgrounds
    ├── Flow Lab          ← MVP
    ├── Mechanics Lab     ← future
    ├── Definition Lab    ← future MBD/GD&T
    ├── Factory Lab       ← future manufacturing
    ├── Thermal Lab       ← future
    └── Power Lab         ← future electrical
```

Flow-specific physics remains outside the shared game core.

## Technology

The MVP uses **Godot 4 + GDScript** for native mobile workflow, touch-first 2D interaction, and a future path to GPU compute while preserving a CPU fallback.

See [`docs/ADR-001-mobile-stack.md`](docs/ADR-001-mobile-stack.md).

## Repository layout

```text
project.godot
scenes/
  main.tscn
content/
  flow/
    campaign.json
    showcases.json
    challenges/
      001_make_it_flow.json
    showcases/
      001_fix_the_shower.json
      002_build_the_exhaust.json
      003_balance_the_hvac.json
      004_design_the_manifold.json
src/
  app/
    main.gd
  core/
    campaign_catalog.gd
    challenge_definition.gd
    challenge_engine.gd
    progression_store.gd
    learn_catalog.gd
    run_telemetry.gd
  flow/
    flow_editor.gd
    flow_visualizer.gd
    flow_challenge_evaluator.gd
    flow_playground.gd
    showcase_catalog.gd
    lbm_solver.gd
docs/
  FLOW_SHOWCASES.md
```

## Current implementation

The current playable slice now covers the platform plus the initial 30-level Flow campaign and four applied showcase definitions:

- mobile project shell and landscape simulation target
- shared playground and simulation contracts
- CPU D2Q9 Lattice Boltzmann prototype
- live particle, velocity, pressure, and swirl visualization
- touch draw/erase/pan plus pinch zoom
- undo/redo and reset
- versioned JSON challenge schema
- reusable challenge lifecycle
- multi-objective Flow scoring and engineering feedback
- local privacy-conscious run telemetry
- local personal-best persistence
- challenge completion, grades, and 1–3 star progression
- local presentation preference: Explorer or Engineer
- contextual concept unlocks from challenge completion
- Learn library with discovered concepts
- Challenge / Sandbox / Learn mode switching
- Sandbox clean-scene reset using the same editor/solver/visualizer implementation
- data-driven 30-level launch campaign across five chapters
- sequential level unlocks plus chapter star gates
- per-level replay targets for 1/2/3 stars
- previous/next challenge navigation
- four real-world showcase challenge definitions with explicit fidelity boundaries
- reusable showcase catalog loader and media metadata

## Launch campaign

The launch campaign contains **30 authored levels** across five chapters:

1. **Make It Flow** — channels, obstacles, smooth transitions, restriction, wakes.
2. **Pressure** — pressure loss, contractions, expansions, velocity tradeoffs.
3. **Split It** — parallel-path and merge intuition using the current single-outlet solver as a visual proxy.
4. **Control It** — geometry-based throttling and conditioning until true valve/pump components are implemented.
5. **Engineer It** — simplified applied scenarios for plumbing, exhaust, HVAC, manifold intuition, packaging, and a capstone.

Every level defines its own success thresholds, scoring weights, concept unlocks, hints, material constraints, 1/2/3-star replay targets, domain targets, and campaign/chapter metadata.

Chapter progression is gated by both sequential completion and total stars, so replaying earlier levels can unlock later chapters.

## Applied Flow showcases

Four dedicated showcase challenges sit alongside the main campaign and are structured for screenshots, trailers, store media, and future higher-fidelity upgrades:

1. **Fix the Shower** — residential plumbing. Optimize delivery, pressure loss, and material usage.
2. **Build the Exhaust** — automotive exhaust. Optimize flow, restriction, route length, and packaging.
3. **Balance the HVAC** — HVAC distribution. Explore delivery, symmetry, restriction, and path complexity.
4. **Design the Manifold** — manifold optimization. Explore compact packaging, pressure loss, and visually uniform distribution.

Each showcase contains theme metadata, environment labels, scoring targets, replay thresholds, concept unlocks, and an explicit fidelity note. See [`docs/FLOW_SHOWCASES.md`](docs/FLOW_SHOWCASES.md).

The current solver intentionally does **not** claim true multi-outlet balancing, valve/pump behavior, plumbing code compliance, HVAC commissioning accuracy, exhaust pulse/scavenging performance, or flow-bench-equivalent manifold results.

## Core game loop

> **Challenge → Design → Run → Observe → Score → Learn → Modify → Retry**

Passing a challenge records completion and stars, updates the best result, and unlocks concepts attached to that challenge.

### Stars

Star thresholds are **challenge-specific**. Each level carries three replay target scores rather than relying on one global 60/75/90 rule.

Stars and concept discoveries persist locally without requiring an account.

## Learn mode

Learn mode is intentionally discovery-driven rather than a mandatory lesson sequence.

Initial concept catalog:

- Flow Rate
- Pressure
- Velocity
- Restriction
- Vortices & Recirculation
- Pressure Loss
- Flow Balance
- Bernoulli Principle
- Reynolds Number

Each concept has both Explorer wording and Engineer wording, so the same gameplay can serve different audiences without duplicating challenge content.

## Sandbox mode

Sandbox removes challenge objectives and scoring while reusing the exact same Flow systems.

Current sandbox behavior:

- starts from a clean channel
- draw and erase boundaries
- pan and zoom
- run/pause simulation
- switch Flow / Speed / Pressure / Swirl views
- undo/redo edits
- clear back to a blank scene immediately
- inspect the same live solver behavior used by Challenge mode

No account is required for basic sandbox use.

Current component limitation: discrete placeable **inlets/outlets, valves, and pumps/fans** are still future Flow-editor work. The MVP sandbox currently uses the solver's fixed inlet/outlet channel boundaries plus free geometry editing.

## Scoring philosophy

Engineering Playground rewards **tradeoffs**, not a single maximum value. Current Flow levels use delivery, pressure retention, turbulence/recirculation, and material usage in different combinations.

Current solver values are educational/gameplay proxies and are not presented as validated engineering analysis.

## Current controls

Top toolbar:

- **CHALLENGE / SANDBOX / LEARN** — switch product modes
- **DRAW / ERASE / PAN** — edit or navigate the flow workspace
- **UNDO / REDO / RESET** — edit history and reset current mode

Challenge workspace:

- **PREV / NEXT** — move through unlocked campaign levels
- chapter and level position
- current stars, best score, total stars, and per-level star targets
- lock reason when the next level/chapter is not yet available

Bottom toolbar in Challenge/Sandbox:

- **FLOW / SPEED / PRESS / SWIRL** — visualization modes
- **SCORE** in Challenge — evaluate design
- **CLEAR** in Sandbox — clean blank channel
- **RUN / PAUSE** — simulation control

Learn mode replaces simulation controls with the discovered-concept library and an **EXPLORER / ENGINEER** presentation toggle.

## Running locally

1. Install a stable Godot 4 release.
2. Clone this repository.
3. Open `project.godot` in Godot.
4. Run the project.

The prototype opens directly into Flow Lab Challenge mode. Mobile export configuration and real-device performance are still being hardened.

## Flow solver strategy

The first technical spike uses a **D2Q9 Lattice Boltzmann Method (LBM)** implementation on the CPU because it maps naturally to a grid, supports interactive obstacles, and exposes velocity/density fields useful for game visualization and scoring.

The solver is intentionally an educational/gameplay approximation. It is **not professional CFD**.

The architecture leaves room for CPU fallback, GPU acceleration, dynamic quality scaling, and different domain solvers for later playgrounds.

## MVP exclusions

Not in the current MVP:

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

Implemented slices:

- **#2** Mobile foundation and project architecture
- **#3** Shared game-core systems and playground contract
- **#4** Flow solver spike
- **#5** Flow visualization
- **#6** Touch-first Flow editor
- **#7** Challenge engine and content schema
- **#8** Flow scoring, feedback, and telemetry
- **#9** Player progression, concept unlocks, and Learn mode
- **#10** Flow Lab sandbox mode
- **#11** 30-level Flow Lab launch campaign authoring and campaign progression shell
- **#12** Applied Flow showcase scenarios — plumbing, exhaust, HVAC, manifold

Remaining #11 work is real playtesting and threshold tuning against observed solver behavior on desktop/mobile hardware. Remaining #12 work is the polished in-app showcase-selection/presentation layer plus real-device capture/tuning. Discrete components and higher-fidelity domain solvers remain separate follow-up work.
