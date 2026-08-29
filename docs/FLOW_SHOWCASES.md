# Flow Lab Showcase Scenarios

The MVP includes four real-world showcase challenges intended to demonstrate Engineering Playground's long-term potential while staying honest about the current 2D educational solver.

## Unity Showcase presentation

The Unity production path now represents Showcase as a first-class Flow Lab mode. PREV/NEXT cycles through the four authored scenarios, each scenario uses the shared Flow solver/editor/visualizer, and SCORE evaluates the existing normalized gameplay objective.

Showcase scores are demonstration/optimization feedback only. They do **not** write into campaign progression.

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

Tradeoff: delivery vs pressure loss vs material usage.

Reference-estimate assumptions currently use room-temperature water through a 19 mm equivalent pipe, with an illustrative path length, reference velocity, roughness, and aggregate minor-loss coefficient.

Use in media: show a constrained house-like route, pressure overlay, normalized gameplay improvement, and the separate reference card as an educational engineering check.

Current limitation: this is not a code-compliant plumbing-network calculator. The MVP does not model fixture diversity, elevation head, water hammer, pipe schedules, independent fixture demand, or code sizing.

## 2. Build the Exhaust — Automotive Exhaust

Tradeoff: flow vs restriction vs packaging/path length.

Reference-estimate assumptions currently use a simplified steady gas reference through a 63.5 mm equivalent tube.

Use in media: show an engine-to-rear-exit concept, compare a sharp route against a smoother optimized route, and clearly separate the normalized visual/gameplay result from the classical reference calculation.

Current limitation: the MVP uses simplified 2D incompressible visual flow, while the reference card is only a steady 1D teaching estimate. It does not model compressible pulse behavior, RPM, scavenging, acoustics, temperature fields, catalysts, or dyno-grade power changes. Never market the result as an exhaust performance prediction.

## 3. Balance the HVAC — HVAC Distribution

Tradeoff: delivery vs symmetry vs restriction/path complexity.

Reference-estimate assumptions currently use room-condition air through a 300 mm equivalent hydraulic diameter.

Use in media: show an air-handler-to-rooms concept, demonstrate how geometry changes the visible flow field, and use the reference card to introduce Reynolds number and pressure-loss concepts.

Current limitation: the MVP does not yet solve independent branch airflow or room pressure. 'Balance' is currently a visual/symmetry engineering exercise using pressure-loss and recirculation proxies, not a duct-network commissioning, fan-curve, leakage, or complete building-network tool.

## 4. Design the Manifold — Manifold Optimization

Tradeoff: uniform-looking distribution vs pressure loss vs compact packaging.

Reference-estimate assumptions currently use an air-like single-runner case with a 50 mm equivalent runner diameter.

Use in media: show one inlet, a constrained plenum/manifold concept, and the improvement in pressure/swirl visualization after geometry refinement while keeping the 1D reference calculation separate.

Current limitation: the MVP does not yet report independent outlet mass-flow rates or a true percent-balance calculation. Multi-outlet balance and flow-bench equivalence must not be claimed until independent branch measurement and calibration exist.

## Current geometry limitation

The four authored Showcase definitions currently use the shared `default_channel` starting geometry. They are distinct in challenge text, scoring, targets, constraints, theme metadata, and Reference-estimate assumptions, but they are **not yet visually distinct solver environments**.

A future geometry-preset/presentation pass should add scenario-specific packaging and starting geometry for plumbing, exhaust, HVAC, and manifold use cases without changing the honest fidelity boundaries above.

## Responsible presentation rules

- Call the current visual solver an educational/gameplay approximation, never validated CFD.
- Keep normalized gameplay scoring separate from dimensional engineering reference calculations.
- Every dimensional card must be labeled **Reference estimate** and list its assumptions/fidelity limitation.
- Never convert lattice density, velocity, or pressure-like fields directly into PSI, kPa, GPM, CFM, horsepower, or other engineering units unless an explicit benchmark/calibration path has been completed.
- Screenshots and trailers may show score, pressure-like fields, velocity, swirl, classical reference quantities, and design iteration as long as their meaning is clear.
- Do not claim plumbing code compliance, HVAC commissioning accuracy, exhaust horsepower gains, or manifold flow-bench equivalence.
- Every showcase should emphasize the game loop: build → test → observe → improve.

## Test/validation status

Unity EditMode regression-test source now checks that:

- all four Showcase entries are present and unique
- Showcase paths are Unity-relative and safe
- all four standalone challenge definitions parse
- all four physical assumption sets produce finite positive flow/Reynolds/friction/loss values
- every formatted card includes the Reference-estimate and no-lattice-conversion warnings

These are source-level test definitions only until a clean Unity Test Framework run is recorded.

## Future upgrades

These showcases are deliberately structured so they can become more realistic later without changing their player-facing identity. Planned future capabilities include visually distinct geometry presets/overlays, placeable inlets/outlets, discrete valves/fans/pumps, independent outlet measurements, 1D network solvers for plumbing/HVAC, and compressible/pulsating exhaust models where justified.
