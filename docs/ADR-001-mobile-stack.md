# ADR-001 — Mobile stack and engine

## Status
Accepted for Flow Lab MVP foundation.

## Context
Engineering Playground is intended to be one mobile game with multiple engineering playgrounds. The MVP must support a tactile 2D Flow Lab on phones and tablets while leaving room for domain-specific engines later.

The product needs:

- iOS and Android export
- strong 2D rendering and touch input
- deterministic game-loop ownership
- real-time simulation visualization
- local/offline play
- a future GPU compute path without requiring it for all devices
- modular boundaries between game systems and engineering solvers

## Decision
Use **Godot 4 with GDScript** for the MVP.

Use the Godot **Mobile renderer** on supported devices and preserve the ability to introduce a Compatibility/CPU fallback path where required.

Flow Lab begins with a CPU **D2Q9 Lattice Boltzmann Method** technical prototype. GPU compute is an optimization path, not a launch dependency.

## Why Godot

### Mobile game fit
Godot provides a native game runtime, scene system, touch input, mobile export, animation, audio, haptics integration paths, and 2D rendering without requiring a web UI layer to become a game engine.

### Simulation/rendering fit
Flow Lab needs to update a grid and render derived scalar/vector fields. Godot can support this first on CPU and later through RenderingDevice/compute on supported hardware.

### Architecture fit
Playground modules can remain ordinary Godot code/resources behind domain-neutral interfaces. Flow, Mechanics, Definition/MBD, Manufacturing, Thermal, and Electrical do not need to share a solver.

### Developer velocity
The MVP question is whether the design → test → improve loop is fun. Godot minimizes non-game platform work while we answer that question.

## Alternatives considered

### React Native / Expo
Excellent product UI ecosystem and cross-platform application development. Less attractive for a simulation-first game where rendering and deterministic high-frequency updates dominate the experience. It remains a viable future companion/admin technology, but not the chosen gameplay runtime.

### Unity
Strong mobile game engine and compute ecosystem. Rejected for MVP because Godot provides sufficient capability with a lighter-weight open-source stack and fewer product/licensing concerns for this project stage.

### Native Swift/Kotlin
Maximum platform control but doubles platform-specific work and slows early iteration.

### Browser-first/WebGPU
Attractive for distribution, but the stated product target is a mobile game. Browser/WebGPU can be revisited later as a secondary target if demand justifies it.

## Orientation
- Hub/navigation may eventually support portrait presentation.
- Flow simulation workspace is **landscape-first**.
- The initial technical prototype launches directly into landscape Flow Lab.

## Persistence
MVP progression and settings should work locally/offline first. Online identity/cloud sync are future shared-core services, not blockers for proving gameplay.

## Consequences

### Positive
- One engine for iOS/Android game development.
- Touch and rendering are first-class.
- CPU solver can be prototyped immediately.
- GPU acceleration remains available later.
- Future playgrounds can use different domain engines.

### Tradeoffs
- GPU compute capability/performance varies across mobile devices.
- GDScript CPU numerics will need profiling and may need native/GPU acceleration at larger grid sizes.
- iOS device export still requires Apple's signing/Xcode workflow.
- The simulation must degrade quality gracefully instead of assuming desktop-class hardware.

## Guardrails
1. Do not describe Flow Lab as validated CFD.
2. Do not couple `src/core` to Flow concepts.
3. Do not require GPU compute for the first playable build.
4. Measure solver frame time before increasing grid resolution.
5. Prefer responsive gameplay over numerical fidelity that the product does not need.
