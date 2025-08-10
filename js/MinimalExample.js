import { Group, Vector3, Quaternion, Color } from 'three';
import { TrTransform } from './TrTransform.js';
import { ControlPoint } from './ControlPoint.js';
import { TubeBrush } from './TubeBrush.js';
import BrushCatalog from './BrushCatalog.js';

export function createCircleStroke(scene) {
  const canvas = new Group();
  scene.add(canvas);

  const path = [];
  const segments = 32;
  const radius = 1.5;
  for (let i = 0; i < segments; i++) {
    const angle = (i * 2 * Math.PI) / segments;
    const position = new Vector3(Math.cos(angle) * radius, Math.sin(angle) * radius, 0);
    const rotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), angle);
    path.push(TrTransform.TRS(position, rotation, 1));
  }

  const controlPoints = [];
  let time = 0;
  for (const tr of path) {
    const cp = new ControlPoint(
      tr.translation.clone(),
      tr.rotation.clone(),
      tr.scale,
      time++
    );
    controlPoints.push(cp);
  }

  const catalog = BrushCatalog.createDefault();
  const desc = catalog.getDescriptorByName('TubeBrush');
  const brush = new TubeBrush();
  brush.m_Color = new Color('blue');
  brush.BaseSize_PS = 0.05;
  brush.initBrush(desc, TrTransform.identity);
  for (const cp of controlPoints) {
    brush.addControlPoint(cp);
  }
  brush.finalizeStroke();
  canvas.add(brush.group);

  return { canvas, brush };
}
