import {
  Scene,
  Group,
  BufferGeometry,
  LineBasicMaterial,
  LineLoop,
  Vector3,
  Quaternion,
  Color,
} from 'three';
import { pathToFileURL } from 'node:url';
import { TrTransform } from './TrTransform.js';
import { ControlPoint } from './ControlPoint.js';
import { Stroke, StrokeType } from './Stroke.js';

class Pointer {
  constructor(canvas) {
    this.canvas = canvas;
  }

  recreateLineFromMemory(stroke) {
    const points = stroke.controlPoints.map(cp => cp.pos);
    const geometry = new BufferGeometry().setFromPoints(points);
    const material = new LineBasicMaterial({ color: stroke.color });
    const line = new LineLoop(geometry, material);
    this.canvas.add(line);
    stroke.object = {
      canvas: this.canvas,
      hideBrush: hide => {
        line.visible = !hide;
      },
      setParent: parent => {
        parent.add(line);
        this.canvas = parent;
      },
    };
    stroke.type = StrokeType.BrushStroke;
  }
}

export function drawCircle() {
  const scene = new Scene();
  const canvas = new Group();
  scene.add(canvas);

  const pointer = new Pointer(canvas);

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

  const stroke = new Stroke();
  stroke.intendedCanvas = canvas;
  stroke.brushGuid = 'default';
  stroke.brushScale = 1;
  stroke.brushSize = 1;
  stroke.color = new Color('blue');
  stroke.seed = 0;
  stroke.controlPoints = controlPoints;
  stroke.controlPointsToDrop = new Array(controlPoints.length).fill(false);

  stroke.recreate(pointer, TrTransform.identity, canvas);

  console.log(`Created stroke with ${controlPoints.length} control points.`);
  console.log(`Canvas has ${canvas.children.length} object(s).`);
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  drawCircle();
}
