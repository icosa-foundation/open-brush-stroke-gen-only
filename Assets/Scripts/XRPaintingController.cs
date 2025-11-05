// XR Painting Controller for VR painting demo
// This controller enables painting in 3D space using VR controllers
// Controls:
// - Right Controller Trigger: Hold to paint
// - The pointer follows the dominant hand controller position
using TiltBrush;
using UnityEngine;

public partial class XRPaintingController : MonoBehaviour
{
	[Godot.Export] public Godot.NodePath PointerPath;
	[Godot.Export] public Godot.NodePath ControllerPath;
	[Godot.Export] public string TriggerAction = "trigger_click"; // XR trigger action name
	[Godot.Export] public bool UseRightHand = true; // True for right hand, false for left hand

	private PointerScript Pointer;
	private Godot.XRController3D Controller;

	private bool _isDrawing = false;
	private bool _wasTriggerPressed = false;

	public override void Awake()
	{
		base.Awake();
		Godot.GD.Print("XRPaintingController: Awake called");

		// Resolve the NodePath to get the actual PointerScript
		if (PointerPath != null && !PointerPath.IsEmpty)
		{
			Godot.GD.Print($"XRPaintingController: Resolving PointerPath: {PointerPath}");
			var node = GetNode(PointerPath);
			if (node != null)
			{
				Godot.GD.Print($"XRPaintingController: Found node: {node.Name}, Type: {node.GetType().Name}");
				Pointer = node as PointerScript;
				if (Pointer == null)
				{
					Godot.GD.PushError($"XRPaintingController: Node at path is not a PointerScript! It's a {node.GetType().Name}");
				}
			}
			else
			{
				Godot.GD.PushError($"XRPaintingController: Could not find node at path: {PointerPath}");
			}
		}
		else
		{
			Godot.GD.PushError("XRPaintingController: PointerPath is not set!");
		}

		// Resolve controller
		if (ControllerPath != null && !ControllerPath.IsEmpty)
		{
			var node = GetNode(ControllerPath);
			Controller = node as Godot.XRController3D;
			if (Controller == null)
			{
				Godot.GD.PushError($"XRPaintingController: ControllerPath does not point to an XRController3D!");
			}
			else
			{
				Godot.GD.Print($"XRPaintingController: Found XR Controller: {Controller.Name}");
			}
		}
		else
		{
			Godot.GD.PushError("XRPaintingController: ControllerPath is not set!");
		}
	}

	public override void Start()
	{
		base.Start();
		Godot.GD.Print("XRPaintingController: Start called");
		Godot.GD.Print($"XRPaintingController: Pointer = {(Pointer != null ? "assigned" : "NULL")}");
		Godot.GD.Print($"XRPaintingController: Controller = {(Controller != null ? "assigned" : "NULL")}");

		PrintControls();
	}

	private void PrintControls()
	{
		Godot.GD.Print("========================================");
		Godot.GD.Print("XR PAINTING CONTROLS:");
		Godot.GD.Print($"  {(UseRightHand ? "RIGHT" : "LEFT")} CONTROLLER TRIGGER - Hold to paint");
		Godot.GD.Print("  Move controller in 3D space to paint");
		Godot.GD.Print("========================================");
	}

	public override void Update()
	{
		base.Update();

		if (Pointer == null || Controller == null)
		{
			return;
		}

		// Update pointer position to match controller position
		UpdatePointerPosition();

		// Check trigger state
		bool triggerPressed = IsTriggerPressed();

		// Handle drawing toggle with trigger (hold to draw)
		if (triggerPressed && !_wasTriggerPressed)
		{
			StartDrawing();
		}
		else if (!triggerPressed && _wasTriggerPressed)
		{
			StopDrawing();
		}

		_wasTriggerPressed = triggerPressed;
	}

	private void UpdatePointerPosition()
	{
		// Get controller's global position and rotation
		var controllerGlobalPos = Controller.GlobalPosition;
		var controllerGlobalRot = Controller.GlobalRotation;

		// Update pointer position
		var pointerNode = Pointer as UnityEngine.MonoBehaviour;
		if (pointerNode != null)
		{
			pointerNode.GlobalPosition = new Vector3(controllerGlobalPos.X, controllerGlobalPos.Y, controllerGlobalPos.Z);
			pointerNode.GlobalRotation = new Vector3(controllerGlobalRot.X, controllerGlobalRot.Y, controllerGlobalRot.Z);
		}
	}

	private bool IsTriggerPressed()
	{
		if (Controller == null) return false;

		// Check if the trigger button/action is pressed
		// In Godot XR, we use GetFloat to get the trigger value (0.0 to 1.0)
		float triggerValue = Controller.GetFloat(TriggerAction);

		// Consider trigger pressed if value is greater than 0.5 (halfway pressed)
		return triggerValue > 0.5f;
	}

	private void StartDrawing()
	{
		if (Pointer != null)
		{
			_isDrawing = true;
			Pointer.DrawingEnabled = true;
			Godot.GD.Print("========================================");
			Godot.GD.Print("XR Drawing STARTED - Release trigger to stop");
			Godot.GD.Print($"Pointer.Canvas = {(Pointer.Canvas != null ? "assigned" : "NULL")}");
			Godot.GD.Print($"Pointer.m_CurrentBrush = {(Pointer.m_CurrentBrush != null ? Pointer.m_CurrentBrush.m_DurableName : "NULL")}");
			Godot.GD.Print("========================================");
		}
	}

	private void StopDrawing()
	{
		if (Pointer != null)
		{
			_isDrawing = false;
			Pointer.DrawingEnabled = false;
			Godot.GD.Print("XR Drawing STOPPED");

			// Check if any geometry was created
			var canvas = Pointer.Canvas;
			if (canvas != null)
			{
				int childCount = (canvas as UnityEngine.MonoBehaviour)?.GetChildCount() ?? 0;
				Godot.GD.Print($"Canvas has {childCount} child nodes after drawing");
			}
		}
	}
}
