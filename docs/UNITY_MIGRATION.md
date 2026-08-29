# Unity Migration

Engineering Playground production development is standardized on **Unity 6.3 LTS + C#**.

Initial editor pin: **6000.3.18f1**.

The Godot implementation remains in the repository only as migration reference until Unity reaches functional parity. Do not extend the Godot runtime with new production features.

## Production boundaries

### Unity owns
- mobile lifecycle
- iOS / Android builds
- scenes and UI
- touch input
- field visualization
- audio / haptics
- application navigation

### Plain C# owns engineering truth where practical
- D2Q9 LBM baseline
- engineering reference equations
- scoring math
- challenge/content validation
- progression rules

This preserves deterministic testability and leaves room for later Burst/Jobs or compute-shader acceleration without making a `MonoBehaviour` the source of numerical truth.

## Current Unity foundation

- `ProjectSettings/ProjectVersion.txt` pins Unity 6000.3.18f1.
- `Packages/manifest.json` includes Input System, Newtonsoft JSON, Unity Test Framework, and UGUI.
- `ProjectSettings/ProjectSettings.asset` establishes Engineering Playground identifiers and mobile minimums.
- Flow simulation workspace is landscape-first.
- iOS minimum target is currently 15.0.
- Android minimum SDK is currently API 26.

## Content migration

The existing `content/` tree remains authoritative.

In the Unity Editor, runtime content loaders resolve directly to the repository-root `content/` directory. Before a player build, `EngineeringContentSync` copies that tree into `Assets/StreamingAssets/content/`.

This avoids duplicating challenge authoring while keeping mobile players self-contained.

Newtonsoft JSON is used intentionally because the challenge schema contains nested dictionaries / flexible domain data that should not be rewritten merely to fit `JsonUtility`.

Migration rules:
- preserve challenge IDs
- preserve scoring semantics unless deliberately versioned
- preserve concept IDs
- keep the 30-level campaign source authoritative
- keep all four showcase definitions authoritative
- normalize legacy `res://content/` showcase paths in Unity rather than changing player-facing identity

## Current Unity runtime slice

Implemented source-level pieces:
- campaign/challenge content models and validation
- local PlayerPrefs-backed progression store
- plain-C# D2Q9 Flow solver
- low-Mach and finite-state guardrails
- `FlowLabRuntimeController`
- Flow / velocity / pressure / vorticity texture visualization
- draw / erase / pan editor tools
- interpolated strokes
- undo / redo solid-mask history
- clear / reset
- migration bootstrap that creates a basic Flow workspace and toolbar without requiring an authored scene
- content regression tests for 30 campaign challenges and four showcase files
- build-time content synchronization

The bootstrap UI is migration scaffolding. Once Unity has a clean import and the first parity slice is running, replace it with authored scenes/prefabs rather than treating the generated toolbar as final game UX.

## Still required before Godot retirement

- clean Unity 6.3 import/compile evidence
- passing EditMode tests
- true touch pinch-zoom validation on device
- challenge lifecycle/scoring/result panel parity
- campaign navigation and star gates
- Learn UI and concept unlock parity
- Sandbox mode parity
- Showcase selection + reference-math card parity
- run telemetry parity
- authored production UI/prefabs
- iOS device build
- Android device build
- performance/thermal measurements
- all #15 numerical benchmarks

## Retirement rule

Do not delete `project.godot`, `scenes/`, or the GDScript prototype until equivalent Unity behavior is verified. Once parity is demonstrated, archive or remove legacy runtime code in one explicit cleanup change so there is no ambiguity about the production engine.
