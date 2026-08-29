# Flow Lab Showcase Scenarios

The MVP includes four real-world showcase challenges intended to demonstrate Engineering Playground's long-term potential while staying honest about the current 2D educational solver.

## Unity Showcase presentation

The Unity production path represents Showcase as a first-class Flow Lab mode. PREV/NEXT cycles through the four authored scenarios, each scenario uses the shared Flow solver/editor/visualizer, and SCORE evaluates the existing normalized gameplay objective.

Showcase scores are demonstration/optimization feedback only. They do **not** write into campaign progression.

Each Showcase now has two separate visual layers:

1. **Scenario-specific solver geometry** — a deterministic solid-mask preset that actually changes the 2D LBM flow domain.
2. **Packaging/context overlay** — translucent house, vehicle, room, or manifold graphics and labels that help the player understand the engineering setting but do **not** participate in the solver physics.

The current geometry presets are deliberately engineering-game proxies, not literal network or component models. The distinction is surfaced in the UI so a player is never meant to read a packaging outline as a CFD boundary.

Each Showcase also displays a separate **Reference estimate — Classical 1D** card. This card is intentionally independent of the D2Q9 lattice simulation. It is calculated from explicit physical assumptions using the #15 engineering reference model and may show:

- equivalent/hydraulic diameter
- assumed path length and velocity
- volumetric flow rate
- Reynolds number and flow regime
- Darcy friction factor
- dynamic pressure
- Darcy-Weisbach major loss
- minor `K` loss
- total estimated pressure loss

Every Reference estimate must state that it is **not a conversion of lattice units** and is **not a professional engineering result**.

## 1. Fix the Shower — Residential Plumbing

Geometry preset: `showcase_residential_plumbing`

The solver mask uses staggered house/service-space obstructions to create a constrained route. The overlay adds a house envelope, floor/chase cues, and SUPPLY / SINK / UPSTAIRS SHOWER labels.

Tradeoff: delivery vs pressure loss vs material usage.

Reference-estimate assumptions currently use room-temperature water through a 19 mm equivalent pipe, with an illustrative path length, reference velocity, roughness, and aggregate minor-loss coefficient.

Current limitation: this is not a code-compliant plumbing-network calculator. The geometry is a routing/packaging proxy; the MVP does not model fixture diversity, elevation head, water hammer, pipe schedules, independent fixture demand, or code sizing.

## 2. Build the Exhaust — Automotive Exhaust

Geometry preset: `showcase_automotive_exhaust`

The solver mask uses floor/chassis, drivetrain, and rear-suspension obstruction zones to leave a constrained underbody route. The overlay labels ENGINE, CHASSIS / FLOOR, and REAR EXIT.

Tradeoff: flow vs restriction vs packaging/path length.

Reference-estimate assumptions currently use a simplified steady gas reference through a 63.5 mm equivalent tube.

Current limitation: the MVP uses simplified 2D incompressible visual flow, while the reference card is only a steady 1D teaching estimate. It does not model compressible pulse behavior, RPM, scavenging, acoustics, temperature fields, catalysts, or dyno-grade power changes. Never market the result as an exhaust performance prediction.

## 3. Balance the HVAC — HVAC Distribution

Geometry preset: `showcase_hvac_distribution`

The solver mask creates a symmetric plenum/distribution problem with central and room-side obstruction zones. The overlay adds an AIR HANDLER and three room envelopes.

Tradeoff: delivery vs symmetry vs restriction/path complexity.

Reference-estimate assumptions currently use room-condition air through a 300 mm equivalent hydraulic diameter.

Current limitation: the MVP does not solve independent branch airflow or room pressure. 'Balance' is a visual/symmetry engineering exercise using pressure-loss and recirculation proxies, not a duct-network commissioning, fan-curve, leakage, or complete building-network tool.

## 4. Design the Manifold — Manifold Optimization

Geometry preset: `showcase_manifold_optimization`

The solver mask uses a central plenum obstruction and three downstream separators to create four visible passages. The overlay labels one inlet and four apparent outlets.

Tradeoff: uniform-looking distribution vs pressure loss vs compact packaging.

Reference-estimate assumptions currently use an air-like single-runner case with a 50 mm equivalent runner diameter.

Current limitation: the LBM domain still has one right-side solver outlet boundary. The four visible passages are a geometry/distribution proxy, not four independently measured outlet boundary conditions. Multi-outlet balance and flow-bench equivalence must not be claimed until independent branch measurement and calibration exist.

## Geometry safety rules

- Showcase geometry IDs are explicit content values; unknown `showcase_*` geometry IDs fail rather than silently falling back to the default channel.
- All four presets preserve the open inlet and outlet columns expected by the current D2Q9 boundary-condition implementation.
- Presets are resolution-relative, so the authored layout scales with the current grid dimensions.
- Packaging overlays are non-interactive and have raycasts disabled so drawing/erasing continues to target the Flow workspace.
- Packaging overlays are hidden in Challenge, Sandbox, and Learn modes.
- Editing a Showcase changes the solver mask, while the packaging overlay remains context-only.

## Responsible presentation rules

- Call the current visual solver an educational/gameplay approximation, never validated CFD.
- Keep normalized gameplay scoring separate from dimensional engineering reference calculations.
- Every dimensional card must be labeled **Reference estimate** and list its assumptions/fidelity limitation.
- Never convert lattice density, velocity, or pressure-like fields directly into PSI, kPa, GPM, CFM, horsepower, or other engineering units unless an explicit benchmark/calibration path has been completed.
- Screenshots and trailers may show scenario packaging, solver geometry, score, pressure-like fields, velocity, swirl, classical reference quantities, and design iteration as long as their meaning is clear.
- Do not claim plumbing code compliance, HVAC commissioning accuracy, exhaust horsepower gains, or manifold flow-bench equivalence.
- Every showcase should emphasize the game loop: build → test → observe → improve.

## Test/validation status

Unity EditMode regression-test source now checks that:

- all four Showcase entries are present and unique
- Showcase paths are Unity-relative and safe
- all four standalone challenge definitions parse
- the four scenarios use four dedicated geometry IDs rather than `default_channel`
- all four generated solid masks are distinct
- every preset preserves the inlet/outlet columns
- unknown Showcase geometry IDs are rejected
- all four physical assumption sets produce finite positive flow/Reynolds/friction/loss values
- every formatted card includes the Reference-estimate and no-lattice-conversion warnings

These are source-level test definitions only until a clean Unity Test Framework run is recorded.

## Future upgrades

The next realism steps can now build on stable scenario identities: authored production art/prefabs to replace the bootstrap overlay primitives, placeable inlets/outlets, discrete valves/fans/pumps, independent outlet measurements, 1D network solvers for plumbing/HVAC, and compressible/pulsating exhaust models where justified.
