# AGENTS

## Coding Guidelines
- Move functionality out of Unity `MonoBehaviour` classes into plain C# classes.
- Keep `MonoBehaviour` scripts as small wrappers that forward calls to the standalone classes.
- Place new C# source files in `Assets/Scripts` and ensure namespaces remain `TiltBrush`.
- When adding files or modifying code, run `dotnet build` from the repository root.

## Unity Independence Plan
1. **Consolidate core logic** – ensure all features live in non-`MonoBehaviour` classes with stubs limited to serialization and event forwarding.
2. **Abstract Unity APIs** – wrap calls to `UnityEngine` systems (e.g. time, input, transforms, file I/O) behind interfaces so they can be swapped with non‑Unity implementations.
3. **Replace Unity assets** – migrate ScriptableObjects, prefabs, and other asset data to JSON or plain C# representations.
4. **Introduce core build target** – add `.csproj` files and build scripts so the standalone core can compile and run with `dotnet` without Unity.
5. **Provide test harness** – create unit or integration tests that exercise the core library outside the Unity runtime.


## Progress
- Moved brush undo cloning logic into standalone `BaseBrush` helper, further shrinking `BaseBrushScript`.
- Extracted example scene behavior into `MinimalExampleCore` with `MinimalExample` as a stub.
- TODO: migrate remaining brush behaviors into non-`MonoBehaviour` classes.
