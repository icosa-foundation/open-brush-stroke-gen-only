# XR Painting Demo

This demo extends the basic stroke drawing functionality to support VR painting in 3D space using XR controllers.

## Overview

The XR Painting Demo allows you to paint 3D brush strokes in virtual reality using your VR headset and controllers. This is a "Tilt Brush" style experience where you can draw in space using your dominant hand controller.

## Scene: XRPaintingScene.tscn

Location: `Assets/Scenes/XRPaintingScene.tscn`

## How to Use

### Prerequisites

1. **Godot 4.5+ with .NET support** - Make sure you have Godot 4.5 or later with C# support
2. **VR Headset** - Any OpenXR-compatible headset (Meta Quest, PSVR2, Valve Index, etc.)
3. **XR Plugin** - Enable the appropriate XR plugin in Godot:
   - Go to **Project → Project Settings → XR**
   - Enable OpenXR support
   - Configure for your target platform

### Running the Demo

1. Open the project in Godot Editor
2. Open `Assets/Scenes/XRPaintingScene.tscn`
3. Connect your VR headset
4. Click **Run Project** or press **F5**
5. Put on your headset

### Controls

- **Right Controller Trigger** - Hold to paint/draw strokes
- **Move Controller** - Move your hand in 3D space while holding the trigger to create strokes

The painting system automatically:
- Tracks your dominant hand (right hand by default)
- Creates smooth 3D brush strokes following your controller movement
- Stops drawing when you release the trigger

### Customization

You can customize the demo by modifying the `XRPaintingController` node properties in the scene:

- **Use Right Hand** - Toggle between right hand (true) and left hand (false) for painting
- **Trigger Action** - Change the input action name (default: "trigger_click")
- **Pointer Path** - Reference to the PointerScript node
- **Controller Path** - Reference to the XRController3D node to use

## Architecture

The XR demo consists of several key components:

### XRPaintingController.cs

The main controller script that:
- Connects to the XR controller (right or left hand)
- Reads the trigger button state
- Updates the pointer position to match the controller
- Enables/disables drawing based on trigger press

Key features:
- Polls the XR controller's trigger action every frame
- Updates the 3D pointer position to match controller position and rotation
- Triggers drawing when trigger value exceeds 0.5 (halfway pressed)

### Scene Structure

```
XRPaintingScene
├── App                    # Application initialization
├── BrushSystemSetup       # Brush catalog setup
├── Canvas                 # Container for strokes
│   └── Pointer            # Drawing pointer (follows controller)
│       └── PointerVisual  # Visual indicator sphere
├── XROrigin3D             # VR origin/tracking space
│   ├── XRCamera3D         # VR camera (your viewpoint)
│   ├── LeftController     # Left hand controller
│   └── RightController    # Right hand controller
├── XRPaintingController   # VR input handler
└── DirectionalLight3D     # Scene lighting
```

## Comparison with Basic Demo

### Basic Demo (SampleScene.tscn)
- Uses mouse movement for 2D/3D positioning
- Uses Space key to toggle drawing
- Draws on a flat plane at a specific Z position
- Includes automatic circle movement mode for testing

### XR Demo (XRPaintingScene.tscn)
- Uses VR controller tracking for 3D positioning
- Uses controller trigger to toggle drawing
- Draws freely in 3D space
- Natural hand tracking for immersive painting

## Technical Details

### XR Input Mapping

The demo uses Godot's XR input system:
- **Tracker**: `"right_hand"` or `"left_hand"`
- **Action**: `"trigger_click"` (can be customized)
- **Input Type**: Float value from 0.0 (not pressed) to 1.0 (fully pressed)
- **Threshold**: 0.5 (trigger is considered "pressed" when value > 0.5)

### Position Tracking

The pointer position is synchronized with the controller every frame:
```csharp
// Controller position → Pointer position
pointerNode.GlobalPosition = controllerGlobalPos;
pointerNode.GlobalRotation = controllerGlobalRot;
```

This ensures the brush stroke follows your hand naturally in 3D space.

## Extending the Demo

### Adding More Controls

You can extend the controller to add more features:

1. **Change Brush with Grip Button**
   - Read the grip button state
   - Cycle through available brushes

2. **Change Color with Thumbstick**
   - Read thumbstick X/Y values
   - Map to color wheel or color presets

3. **Adjust Brush Size**
   - Use thumbstick up/down
   - Modify `Pointer.BrushSize01`

4. **Two-Handed Painting**
   - Create a second XRPaintingController for the left hand
   - Use different colors/brushes for each hand

### Example: Adding Grip Button for Brush Cycling

```csharp
// In XRPaintingController.Update()
float gripValue = Controller.GetFloat("grip_click");
if (gripValue > 0.5f && !_wasGripPressed)
{
    CycleToNextBrush();
}
_wasGripPressed = gripValue > 0.5f;
```

## Troubleshooting

### "No XR interface detected"
- Make sure OpenXR is enabled in Project Settings → XR
- Verify your headset is connected and drivers are installed
- Check that your platform's XR runtime is running (e.g., Oculus app, SteamVR)

### "Controller not tracking"
- Ensure controllers are turned on and paired
- Check that the tracker names match: `"left_hand"` and `"right_hand"`
- Verify controllers are within tracking range

### "Trigger not working"
- Check if your XR runtime uses a different action name
- Try different trigger action names: `"trigger"`, `"trigger_click"`, `"trigger_value"`
- Use Godot's XR input debugger to see available actions

### "Pointer not moving with controller"
- Verify the ControllerPath points to the correct XRController3D node
- Check console output for any error messages
- Ensure the controller node is being tracked (check IsActive property)

## Future Enhancements

Possible improvements for this demo:

- [ ] Two-handed painting support
- [ ] Color picker using controller input
- [ ] Brush size adjustment with thumbstick
- [ ] Undo/Redo functionality
- [ ] Save/Load painted scenes
- [ ] Hand presence models instead of simple spheres
- [ ] Haptic feedback when drawing
- [ ] UI menu for brush/color selection
- [ ] Teleportation for navigation
- [ ] Scale adjustment for strokes

## License

Same as the main Open Brush project - Apache License 2.0
