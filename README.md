# Engineering Playground

**Build it. Test it. Make it better.**

Engineering Playground is a mobile-first engineering game where players learn by building, simulating, breaking, and improving systems instead of reading through a traditional lesson first.

The long-term product is **one mobile game with multiple modular playgrounds**. The first playable release is **Flow Lab**.

## MVP: Flow Lab

Flow Lab now has three player-facing modes:

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
  flow/challenges/
    001_make_it_flow.json
src/
  app/
    main.gd
  core/
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
    lbm_solver.gd
docs/
```

## Current implementation

The current playable slice now covers GitHub issues #2 through #10 at an implementation level:

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
- first data-driven challenge: **Make It Flow**

## Core game loop

> **Challenge → Design → Run → Observe → Score → Learn → Modify → Retry**

Passing a challenge records completion and stars, updates the best result, and unlocks concepts attached to that challenge. The first challenge currently unlocks **Flow Rate**, **Restriction**, and **Vortices & Recirculation**.

### Stars

- **1 star:** score 60+
- **2 stars:** score 75+
- **3 stars:** score 90+

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

Each concept has both:

- **Explorer wording** — short, intuitive language
- **Engineer wording** — technical terminology and engineering meaning

The player can switch presentation mode without duplicating challenge content.

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

No account is required for basic sandbox use. Save/share/community architecture can be added later without making Sandbox a separate code path.

Current component limitation: discrete placeable **inlets/outlets, valves, and pumps/fans** are still future Flow-editor work. The MVP sandbox currently uses the solver's fixed inlet/outlet channel boundaries plus free geometry editing.

## Scoring philosophy

Engineering Playground rewards **tradeoffs**, not a single maximum value. Flow challenges can weight delivery, pressure retention, turbulence/recirculation, material usage, balance, cost, complexity, and packaging.

Current solver values are educational/gameplay proxies and are not presented as validated engineering analysis.

## Current controls

Top toolbar:

- **CHALLENGE / SANDBOX / LEARN** — switch product modes
- **DRAW / ERASE / PAN** — edit or navigate the flow workspace
- **UNDO / REDO / RESET** — edit history and reset current mode

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

The prototype opens directly into the first Flow Lab challenge. Mobile export configuration and real-device performance are still being hardened.

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

Next major content slice: **#11 launch challenge pack authoring/tuning**, while the discrete component work required for valves, pumps/fans, and placeable inlet/outlet geometry should be completed before calling the sandbox feature fully finished.
