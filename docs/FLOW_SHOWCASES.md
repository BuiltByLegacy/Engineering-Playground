# Flow Lab Showcase Scenarios

The MVP includes four real-world showcase challenges intended to demonstrate Engineering Playground's long-term potential while staying honest about the current 2D educational solver.

## 1. Fix the Shower — Residential Plumbing

Tradeoff: delivery vs pressure loss vs material usage.

Use in media: show a constrained house-like route, pressure overlay, and before/after score improvement.

Current limitation: this is not a code-compliant plumbing-network calculator. The MVP does not model fixture diversity, elevation head, water hammer, pipe schedules, or independent fixture demand.

## 2. Build the Exhaust — Automotive Exhaust

Tradeoff: flow vs restriction vs packaging/path length.

Use in media: show an engine-to-rear-exit path inside a constrained envelope, then compare a sharp route against a smoother optimized route.

Current limitation: the MVP uses simplified 2D incompressible flow. It does not model compressible pulse behavior, RPM, scavenging, acoustics, temperature, catalysts, or dyno-grade power changes. Never market the result as an exhaust performance prediction.

## 3. Balance the HVAC — HVAC Distribution

Tradeoff: delivery vs symmetry vs restriction/path complexity.

Use in media: show an air-handler-to-rooms layout and how smoother/symmetric geometry changes the visible flow field.

Current limitation: the MVP does not yet solve independent branch airflow or room pressure. 'Balance' is currently a visual/symmetry engineering exercise using pressure-loss and recirculation proxies, not a duct-network commissioning tool.

## 4. Design the Manifold — Manifold Optimization

Tradeoff: uniform-looking distribution vs pressure loss vs compact packaging.

Use in media: show one inlet, a constrained plenum/manifold shape, and the improvement in pressure/swirl visualization after geometry refinement.

Current limitation: the MVP does not yet report independent outlet mass-flow rates or a true percent-balance calculation. Multi-outlet balance must not be claimed until branch measurement exists.

## Responsible presentation rules

- Call the current solver an educational/gameplay approximation, never validated CFD.
- Screenshots and trailers may show score, pressure, velocity, swirl, and design iteration.
- Do not show professional-looking unit precision that implies validated engineering analysis.
- Do not claim plumbing code compliance, HVAC commissioning accuracy, exhaust horsepower gains, or manifold flow-bench equivalence.
- Every showcase should emphasize the game loop: build → test → observe → improve.

## Future upgrades

These showcases are deliberately structured so they can become more realistic later without changing their player-facing identity. Planned future capabilities include placeable inlets/outlets, discrete valves/fans/pumps, independent outlet measurements, 1D network solvers for plumbing/HVAC, and compressible/pulsating exhaust models where justified.
