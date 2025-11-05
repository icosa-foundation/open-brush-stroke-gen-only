# Open Brush Stroke Generator - Godot Port

This project ports Unity-based TiltBrush stroke generation code to Godot 4.5+ using C#/.NET.

## Status: Initial Setup Complete ✓

The Unity scripts have been wrapped with a compatibility layer that allows them to run in Godot with minimal modifications.

### What's Working

- ✓ **Unity→Godot Type Mapping**: All Unity math types (Vector3, Quaternion, etc.) now use Godot's native types
- ✓ **MonoBehaviour Compatibility**: Scripts inherit from `GodotMonoBehaviour` which bridges Unity/Godot lifecycles
- ✓ **Core Systems**: Debug, Time, Transform, GameObject wrappers functional
- ✓ **Mesh System**: Basic mesh generation wrapper in place
- ✓ **Script Updates**: All MonoBehaviour scripts updated to use GodotMonoBehaviour

### What Needs Work

- ⚠️ **Brush Prefabs**: Unity prefabs need to be converted to Godot scenes
- ⚠️ **Material/Shader System**: Unity shaders need porting to Godot
- ⚠️ **BrushCatalog**: Brush loading/initialization system needs implementation
- ⚠️ **Batch Rendering**: Batched brush rendering needs Godot equivalent
- ⚠️ **Input System**: Unity Input API needs Godot implementation
- ⚠️ **Full Testing**: Needs end-to-end testing with actual brushes

## Quick Start

### Prerequisites

- Godot 4.5 or later with .NET support
- .NET SDK 8.0 or later

### Build the Project

1. Open project in Godot Editor
2. **Project → Tools → C# → Create C# Solution**
3. **Build → Build Project**
4. Check Output panel for any errors

### Understanding the Structure

```
Scripts/
├── UnityEngine/          # Compatibility layer (DON'T MODIFY SCRIPTS HERE)
│   ├── UnityMathTypes.cs # Type aliases (Vector3 → Godot.Vector3)
│   ├── UnityMathf.cs     # Math utilities
│   ├── UnityCore.cs      # Core Unity classes
│   ├── UnityMesh.cs      # Mesh/rendering wrappers
│   ├── GodotMonoBehaviour.cs  # MonoBehaviour base class
│   └── README.md         # Detailed wrapper documentation
├── Brushes/              # Brush implementations (from Unity)
├── Util/                 # Utility classes
├── App.cs               # Application entry point
├── PointerScript.cs     # Drawing pointer logic
├── CanvasScript.cs      # Canvas/stroke container
└── SimpleStrokeDemo.cs  # Example usage script
```

## Documentation

- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Step-by-step setup and troubleshooting
- **[Scripts/UnityEngine/README.md](Scripts/UnityEngine/README.md)** - Detailed wrapper API documentation

## Key Design Decisions

### Using Godot Native Types

Instead of creating duplicate `Vector3`, `Quaternion` classes, the wrapper uses **type aliases**:

```csharp
// In UnityMathTypes.cs
using Vector3 = Godot.Vector3;
using Quaternion = Godot.Quaternion;
```

This means:
- Zero runtime overhead
- Direct compatibility with Godot APIs
- No conversion needed
- Smaller code footprint

### MonoBehaviour Bridge

Unity scripts inherit from `MonoBehaviour`. The compatibility layer implements `MonoBehaviour` as a Godot `Node3D`:

```csharp
// Unity scripts work as-is (just add 'partial' keyword)
using UnityEngine;

public partial class MyScript : MonoBehaviour
{
    void Awake() { }
    void Update() { }
}
```

`MonoBehaviour` automatically:
- Maps `Awake()` → `_Ready()`
- Maps `Update()` → `_Process()`
- Maps `FixedUpdate()` → `_PhysicsProcess()`
- Provides `gameObject` and `transform` properties
- Handles component system basics

## Example Usage

See `Scripts/SimpleStrokeDemo.cs` for a complete example:

```csharp
public partial class SimpleStrokeDemo : MonoBehaviour
{
    [Export] public CanvasScript Canvas;
    [Export] public PointerScript Pointer;

    public void Awake()
    {
        // Initialize here
    }

    public void Update()
    {
        if (Input.IsKeyPressed(Key.Space))
        {
            Pointer.DrawingEnabled = true;
        }
    }
}
```

## Demo Scenes

### Basic Drawing Demo (SampleScene.tscn)

A simple 2D/3D drawing demo using mouse and keyboard controls:
- **MOUSE** - Move pointer in 3D space
- **SPACE** - Hold to draw
- **1-5** - Change colors
- **LEFT/RIGHT ARROW** - Cycle through brushes
- **M** - Toggle automatic circle movement

This is a great starting point to understand the basic stroke generation system.

### XR Painting Demo (XRPaintingScene.tscn)

An immersive VR painting experience for drawing in 3D space with VR controllers:
- **Right Controller Trigger** - Hold to paint
- **Move Controller** - Draw freely in 3D space

This demo provides a "Tilt Brush" style experience where you can paint in virtual reality using your dominant hand controller.

See **[XR_PAINTING_DEMO.md](XR_PAINTING_DEMO.md)** for detailed setup instructions, VR configuration, and extending the XR demo with additional features.

## Next Steps

1. **Build and Test Compilation**
   - Ensure all scripts compile without errors
   - Fix any missing types or APIs

2. **Create Test Scene**
   - Set up Canvas + Pointer nodes
   - Attach SimpleStrokeDemo script
   - Test basic initialization

3. **Implement Brush System**
   - Convert Unity brush prefabs → Godot scenes
   - Port materials/shaders
   - Initialize BrushCatalog

4. **Test Stroke Generation**
   - Start with simplest brush type
   - Verify geometry generation works
   - Test with different brush types

5. **Polish and Optimize**
   - Performance profiling
   - Handle edge cases
   - Improve mesh generation

## Contributing

When adding Unity functionality:

1. **For math operations**: Add to `Scripts/UnityEngine/UnityMathf.cs`
2. **For type extensions**: Add to `UnityMathExtensions` in `UnityMathTypes.cs`
3. **For Unity components**: Add wrappers in `UnityCore.cs` or `UnityMesh.cs`
4. **For new lifecycle methods**: Extend `MonoBehaviour` in `MonoBehaviour.cs`

Keep the compatibility layer in `Scripts/UnityEngine/` separate from application code.

## License

Original TiltBrush code: Apache License 2.0 (see file headers)
Compatibility layer: Same as original project

## Known Issues

- Coordinate system differences (Unity left-handed, Godot right-handed) may require adjustments
- Mesh winding order may need flipping
- Some Unity-specific features are not yet implemented (see SETUP_GUIDE.md)

## Getting Help

1. Check [SETUP_GUIDE.md](SETUP_GUIDE.md) for common issues
2. Review [Scripts/UnityEngine/README.md](Scripts/UnityEngine/README.md) for API details
3. Look at `SimpleStrokeDemo.cs` for usage examples
4. Check Godot console output for detailed error messages
