import { BufferGeometry, LineBasicMaterial, LineLoop } from 'three';
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
