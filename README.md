# Engineering Playground

**Build it. Test it. Make it better.**

Engineering Playground is a mobile-first engineering game where players learn by building, simulating, breaking, and improving systems instead of reading through a traditional lesson first.

The long-term product is **one mobile game with multiple modular playgrounds**. The first playable release is **Flow Lab**.

## MVP: Flow Lab

Flow Lab is a touch-first 2D engineering sandbox and challenge game. Players alter geometry, run a fluid simulation, inspect the result, receive engineering feedback, and iterate to improve their design.

Initial challenge families:

- Flow fundamentals
- Pressure and restriction
- Split flow and balancing
- Valves and pumps/fans
- Residential plumbing
- Automotive exhaust routing
- HVAC distribution
- Multi-outlet manifolds

The MVP deliberately prioritizes **fast, visually understandable iteration** over professional CFD accuracy.

## Product architecture

Engineering Playground is one app, but each discipline owns its domain engine behind shared game contracts.

```text
Engineering Playground
├── Core
│   ├── playground registry
│   ├── challenge lifecycle
│   ├── player/session state
│   ├── scoring/progression hooks
│   ├── concept unlocks
│   └── simulation adapter contract
└── Playgrounds
    ├── Flow Lab          ← MVP
    ├── Mechanics Lab     ← future
    ├── Definition Lab    ← future MBD/GD&T
    ├── Factory Lab       ← future manufacturing
    ├── Thermal Lab       ← future
    └── Power Lab         ← future electrical
```

Flow-specific physics must never become a dependency of the shared game core.

## Technology decision

The MVP uses **Godot 4 + GDScript**.

Why:

- Native mobile game workflow for iOS and Android
- Strong 2D and touch support
- Fast iteration for game UX
- RenderingDevice/compute path available for later GPU acceleration on supported devices
- CPU simulation fallback remains possible for compatibility
- No requirement to build a CAD-style UI framework before proving the gameplay loop

See [`docs/ADR-001-mobile-stack.md`](docs/ADR-001-mobile-stack.md).

## Repository layout

```text
project.godot
scenes/
  main.tscn
src/
  app/
  core/
  flow/
docs/
```

`src/core` is domain-neutral. `src/flow` contains the Flow Lab module and solver.

## Current implementation

The foundation covers GitHub issues #2, #3, and #4:

- Mobile project shell and landscape simulation target
- Shared playground and simulation contracts
- Flow Lab registration boundary
- CPU D2Q9 Lattice Boltzmann technical prototype
- Channel/obstacle demonstration scene
- Pressure-like density and velocity fields exposed by the solver
- Architecture and solver limitations documented

## Running locally

1. Install a stable Godot 4 release.
2. Clone this repository.
3. Open `project.godot` in Godot.
4. Run the project.

The initial prototype opens directly into the Flow Lab solver demonstration. Mobile export configuration will be hardened as the MVP progresses.

## Flow solver strategy

The first technical spike uses a **D2Q9 Lattice Boltzmann Method (LBM)** implementation on the CPU because it maps naturally to a grid, supports interactive obstacles, and exposes velocity/density fields useful for game visualization and scoring.

The solver is intentionally an educational/gameplay approximation. It is **not professional CFD** and must not be presented as validated engineering analysis.

The architecture leaves room for:

1. CPU LBM baseline for broad compatibility.
2. GPU compute acceleration on capable mobile devices.
3. Dynamic resolution/quality scaling.
4. Different domain solvers for future playgrounds.

## MVP rules

We are intentionally **not** building these yet:

- 3D CFD
- CAD import
- FEA
- Full MBD/GD&T implementation
- Manufacturing simulation
- Electrical simulation
- Classroom dashboards
- AI tutor
- Multiplayer/community marketplace

The product hypothesis is simpler:

> Is engineering optimization fun enough that players voluntarily design, test, fail, and retry?

## GitHub

Primary MVP epic: **#1 — Engineering Playground Mobile MVP — Flow Lab**

Foundation work:

- **#2** Mobile foundation and project architecture
- **#3** Shared game-core systems and playground contract
- **#4** Flow solver spike — real-time 2D mobile fluid simulation

Next playable-loop work begins with visualization, touch editing, challenges, and scoring.