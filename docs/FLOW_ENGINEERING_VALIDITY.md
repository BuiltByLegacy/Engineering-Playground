# Flow Lab Engineering Validity Model

Flow Lab should feel mathematically credible without presenting the MVP as professional CFD or a design-certification tool.

## Product rule

Use two related but distinct layers:

1. **Visual simulation layer** — the 2D D2Q9 Lattice Boltzmann solver shows how the velocity/pressure-like field reacts to geometry, restrictions, wakes, recirculation, and smooth vs abrupt transitions.
2. **Engineering reference layer** — classical engineering equations produce dimensional reference values from explicit scenario assumptions.

Never silently convert lattice units into real PSI, Pa, GPM, CFM, horsepower, or temperature. Dimensional values must come from the reference model with visible assumptions.

## Core equations

### Continuity / conservation of mass

For incompressible flow:

`Q = A V`

and mass flow:

`m_dot = rho A V`

Use this to teach why velocity rises as available flow area decreases.

### Dynamic pressure / Bernoulli relationship

`q = 0.5 rho V^2`

For steady, incompressible, inviscid flow with negligible elevation change:

`p + 0.5 rho V^2 = constant`

Use Bernoulli as an intuition/reference relationship, not as the loss model through viscous fittings.

### Reynolds number

`Re = rho V D_h / mu`

Use Reynolds number to classify the reference operating point as laminar, transitional, or turbulent and to choose an appropriate friction-factor approximation.

### Darcy-Weisbach major loss

`DeltaP_major = f (L / D_h) (rho V^2 / 2)`

This is the preferred general-purpose reference relationship for straight-pipe/duct friction in the MVP.

### Minor losses

`DeltaP_minor = K (rho V^2 / 2)`

Represent bends, contractions, expansions, valves/fittings, mufflers, junctions, and other localized restrictions using an aggregate `K` only when the scenario explicitly defines the approximation.

### Friction factor

- Laminar: `f = 64 / Re`
- Turbulent MVP reference: Haaland explicit approximation to Colebrook using relative roughness.

## LBM guardrails

The D2Q9 LBM solver is appropriate for nearly incompressible, low-Mach-number visual flow behavior. It should be treated as a qualitative/semi-quantitative educational solver unless benchmarked and scaled for a specific case.

Do not use the current solver to claim:

- compressible exhaust pulse/scavenging behavior
- acoustic prediction
- combustion or temperature coupling
- validated pump/fan curves
- true plumbing network fixture-demand analysis
- true multi-outlet HVAC/manifold flow balance until independent outlet measurement and network/component models exist
- certification-grade pressure drop, CFM, GPM, horsepower, or efficiency

## Player-facing math levels

### Explorer

Show simple relationships and directionality:

- smaller area -> faster local flow
- faster flow -> larger dynamic-pressure term
- longer/narrower path -> more friction loss
- sharp fittings -> more local loss

### Engineer

Show a compact calculation card:

- reference fluid
- hydraulic diameter
- reference velocity
- Reynolds number
- estimated friction factor
- major loss
- minor loss
- total reference pressure loss

Always include `Reference estimate` or equivalent language.

## Validation plan

Before calling the Flow solver quantitatively trustworthy, benchmark it against canonical cases:

1. Poiseuille/channel flow profile
2. Flow around a cylinder at low/moderate Reynolds number
3. Sudden expansion/contraction trends
4. Pressure-loss trend versus path length
5. Grid-resolution sensitivity
6. Mass conservation error at inlet/outlet
7. Time-step / relaxation-parameter stability

Acceptance should be based on trend and normalized error bands suitable for an educational game, not professional CFD tolerance.

## Architecture

`FlowEngineeringReferenceModel` owns classical equations and physical assumptions. The LBM solver remains independent. A showcase can display both:

- **SIMULATION** — live field from LBM
- **REFERENCE MATH** — dimensional estimate from formulas

This makes the experience look and behave like engineering while preserving honest solver boundaries.
