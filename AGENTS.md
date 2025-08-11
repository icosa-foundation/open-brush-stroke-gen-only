# AGENTS

## Coding Guidelines
- Move functionality out of Unity `MonoBehaviour` classes into plain C# classes.
- Keep `MonoBehaviour` scripts as small wrappers that forward calls to the standalone classes.
- Place new C# source files in `Assets/Scripts` and ensure namespaces remain `TiltBrush`.
- When adding files or modifying code, run `dotnet build` from the repository root.

