import { Group, Vector3, Quaternion, Color } from 'three';
import { ControlPoint } from './ControlPoint.js';
import BrushCatalog from './BrushCatalog.js';
import BrushDescriptor from './BrushDescriptor.js';
import Pointer from './Pointer.js';
import { Stroke } from './Stroke.js';
import TubeBrush from './TubeBrush.js';

export function createCircleStroke(scene) {
  const canvas = new Group();
  scene.add(canvas);

  const controlPoints = [];
  const segments = 32;
  const radius = 1.5;
  for (let i = 0; i < segments; i++) {
    const angle = (i * 2 * Math.PI) / segments;
    const position = new Vector3(Math.cos(angle) * radius, Math.sin(angle) * radius, 0);
    const rotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), angle);
    const cp = new ControlPoint(position, rotation, 1, i);
    controlPoints.push(cp);
  }

  const stroke = new Stroke();
  stroke.controlPoints = controlPoints;
  stroke.color = new Color('blue');
  stroke.brushGuid = 'tube-brush';
  stroke.brushSize = 0.05;

  const tubeDesc = new BrushDescriptor();
  tubeDesc.m_Guid = 'tube-brush';
  tubeDesc.m_DurableName = 'TubeBrush';
  tubeDesc.m_LocalizedDescription = 'Tube Brush';
  tubeDesc.m_BrushPrefab = TubeBrush;
  const manifest = { Brushes: [tubeDesc], CompatibilityBrushes: [] };
  BrushCatalog.Init(manifest);

  const pointer = new Pointer(canvas);
  pointer.recreateLineFromMemory(stroke);

  console.log(`Created TubeBrush stroke with ${stroke.controlPoints.length} control points`);

  return { canvas, stroke };
}
