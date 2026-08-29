# ADR-001 — Mobile stack and engine

## Status
**Superseded by [`ADR-002 — Production game engine: Unity`](ADR-002-unity-engine.md).**

This ADR records the original prototype decision only. Godot/GDScript is no longer the production direction for Engineering Playground.

## Historical context
Engineering Playground is intended to be one mobile game with multiple engineering playgrounds. The first Flow Lab prototype needed a tactile 2D experience on phones/tablets, local/offline play, deterministic simulation updates, and a future GPU-compute path.

## Historical decision
The prototype originally used **Godot 4 + GDScript** with a CPU D2Q9 Lattice Boltzmann solver.

That choice was useful for validating:
- touch-first Flow Lab interactions
- simulation visualization
- challenge/scoring systems
- progression and Learn mode
- sandbox behavior
- applied showcase concepts

## Superseding decision
The production game will use **Unity + C#**.

The retained Godot files are reference/prototype material only during migration. New production feature work should follow ADR-002.

## Preserved architectural lessons
The following rules survive the engine change:

1. Engineering Playground remains one mobile game with modular playgrounds.
2. Flow-specific numerical code stays isolated from shared game systems.
3. The CPU solver is the correctness baseline before any GPU acceleration.
4. Real-device performance must be measured before grid resolution is increased.
5. Responsive gameplay matters more than unnecessary professional-solver fidelity.
6. Flow Lab must not be described as validated professional CFD.
7. Local/offline progression remains the MVP baseline.
8. Simulation workspaces remain landscape-first while hub/navigation may use portrait layouts where useful.
