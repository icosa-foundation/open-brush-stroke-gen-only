import {
  CatmullRomCurve3,
  MeshBasicMaterial,
  Mesh,
  TubeGeometry,
} from 'three';
import { StrokeType } from './Stroke.js';

export class Pointer {
  constructor(canvas) {
    this.canvas = canvas;
  }

  // TODO: initialize drawing with the given brush
  beginStroke(brush) {
    // Implementation will integrate brush-specific behavior
  }

  // TODO: add a new control point to the current stroke
  updateStroke(controlPoint) {
    // Implementation will build stroke geometry incrementally
  }

  // TODO: finalize the stroke and commit geometry
  endStroke() {
    // Implementation will finalize stroke creation
  }

  recreateLineFromMemory(stroke) {
    const points = stroke.controlPoints.map(cp => cp.pos);

    // Build a tube mesh along the control point path
    const curve = new CatmullRomCurve3(points, true);
    const tubularSegments = Math.max(32, points.length * 8);
    const radius = stroke.brushSize || 0.05;
    const geometry = new TubeGeometry(curve, tubularSegments, radius, 8, true);
    const material = new MeshBasicMaterial({ color: stroke.color });
    const mesh = new Mesh(geometry, material);
    this.canvas.add(mesh);

    stroke.object = {
      canvas: this.canvas,
      hideBrush: hide => {
        mesh.visible = !hide;
      },
      setParent: parent => {
        parent.add(mesh);
        this.canvas = parent;
      },
    };
    stroke.type = StrokeType.BrushStroke;
  }
}
