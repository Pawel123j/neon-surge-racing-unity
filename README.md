# Neon Surge — Night Circuit

A compact Unity time trial built from procedural meshes and built-in primitives. The player races a neon GT around one oval circuit, crosses four ordered gates per lap, records split times, and completes a three-lap run. The local best is saved with `PlayerPrefs`.

## Open the project

This project is pinned to **[Unity 6.0.30f1](https://unity.com/releases/editor/whats-new/6000.0.30f1)**. Open this folder in that editor version through Unity Hub, then open `Assets/Scenes/NeonSurge.unity` and press Play.

The scene contains one bootstrap object. It builds the track, start line, checkpoints, car, lights, camera, and HUD at runtime. It uses no downloaded art or external packages beyond Unity Test Framework.

## Controls

- **W** or **Up Arrow** — accelerate
- **S** or **Down Arrow** — brake and reverse
- **A/D** or **Left/Right Arrow** — steer
- **Escape** — pause or resume
- **R** — reset the trial and return to the start screen

Begin the trial, drive through the magenta start line, and follow the four gates in order. The clock starts on the first start-line crossing and stops after the third lap. Steer left through the course and ease off in the long bends.

## Tests and build

The EditMode suite in `Assets/Scripts/Tests/EditMode` covers trial start, checkpoint ordering, lap timing, finish state, and reset behavior. In Unity, open **Window → General → Test Runner**, choose **EditMode**, and run the suite.

The standalone scene is registered in `ProjectSettings/EditorBuildSettings.asset`. Select a target in Build Profiles / Build Settings and build when a Unity 6 editor is available.

**Validation status:** project files and EditMode tests are included, but this session did not run Unity. The user deferred the final editor import, EditMode run, and platform build to a Unity-enabled environment.

## Scope

The car uses an intentionally simple arcade controller. There are no AI opponents, online records, imported art, vehicle damage, or platform-specific systems. The game is a small offline time-trial demo.
