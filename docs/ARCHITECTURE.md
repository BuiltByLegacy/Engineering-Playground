# Engineering Playground architecture

## Principle

**One game, multiple playgrounds, independent domain engines.**

The shared core owns game behavior. Each playground owns engineering-domain behavior.

## Dependency direction

```text
App Shell
   ↓
Game Core
   ↓
Playground Contract
   ↓
Domain Playground
   ↓
Domain Simulation Adapter
```

`src/core` must not import `src/flow`.

## Shared core responsibilities

- Playground registration and metadata
- Challenge lifecycle
- Player/session state
- Generic scoring/result envelope
- Progression hooks
- Concept unlock hooks
- Save/load contracts
- Simulation lifecycle contract
- Shared telemetry events

## Playground responsibilities

A playground provides:

- Identity and metadata
- Domain-specific challenge content
- Domain-specific editor/tools
- Simulation adapter
- Domain-specific metrics mapped into a generic result envelope
- Domain-specific visualization
- Optional learning concepts

## Simulation adapter lifecycle

Every simulation domain should be able to support the same high-level lifecycle:

1. `configure(config)`
2. `reset()`
3. `step(delta)`
4. `get_metrics()`
5. `get_visualization_data()`

A Flow solver may expose velocity and density. A future Mechanics solver might expose displacement/forces. The core should only know that metrics and visualization payloads exist.

## Initial module map

```text
src/
├── app/
│   └── main.gd
├── core/
│   ├── playground.gd
│   ├── simulation_adapter.gd
│   └── challenge_definition.gd
└── flow/
    ├── flow_playground.gd
    └── lbm_solver.gd
```

## Future module examples

### Definition Lab
Owns datum/GD&T/MBD rules and geometric evaluation. It does not reuse Flow physics.

### Factory Lab
Owns manufacturing-process rules, cost models, process capability, and DFM logic.

### Mechanics Lab
Owns rigid-body/kinematic/structural mechanics appropriate to its gameplay.

## Data boundaries

Challenge content should be data-driven. The shared challenge definition carries common fields; each playground may attach a `domain_config` dictionary/resource interpreted only by that playground.

This prevents the generic challenge system from growing Flow-specific fields such as inlet pressure or viscosity.

## Testing strategy

- Unit-test shared contracts without Flow.
- Test Flow solver numerics independently from UI.
- Use a mock playground to prove the core does not depend on Flow.
- Add device performance tests before raising solver resolution.

## Mobile performance strategy

Flow Lab starts with a CPU grid small enough for interactive iteration. Quality levels can adjust:

- simulation grid resolution
- simulation steps per rendered frame
- particle/tracer count
- visualization overlay resolution

GPU compute may later replace or augment the CPU solver on supported devices without changing the shared game contract.
