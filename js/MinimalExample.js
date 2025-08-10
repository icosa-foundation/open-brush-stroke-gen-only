import { Group, Vector3, Quaternion, Color, Matrix4 } from 'three';
import { ControlPoint } from './ControlPoint.js';
import BrushCatalog from './BrushCatalog.js';
import BrushDescriptor from './BrushDescriptor.js';
import BrushManifest from './BrushManifest.js';
import { Pointer } from './Pointer.js';
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
    const radial = new Vector3(Math.cos(angle), Math.sin(angle), 0);
    const tangent = new Vector3(-Math.sin(angle), Math.cos(angle), 0);
    const matrix = new Matrix4().makeBasis(radial, new Vector3(0, 0, 1), tangent);
    const rotation = new Quaternion().setFromRotationMatrix(matrix);
    const cp = new ControlPoint(position, rotation, 1, i);
    controlPoints.push(cp);
  }

  // Close the loop by repeating the first control point at the end.
  const first = controlPoints[0];
  controlPoints.push(
    new ControlPoint(first.pos.clone(), first.orient.clone(), first.pressure, segments)
  );

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
  const manifest = BrushManifest.fromJSON({ Brushes: [tubeDesc], CompatibilityBrushes: [] });
  BrushCatalog.Init(manifest);

  const pointer = new Pointer(canvas);
  pointer.recreateLineFromMemory(stroke);

  console.log(`Created TubeBrush stroke with ${stroke.controlPoints.length} control points`);

  return { canvas, stroke };
}
