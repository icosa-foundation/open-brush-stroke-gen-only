import { Color } from 'three';
import { ControlPoint } from './ControlPoint.js';

export class StrokeData {
  constructor(existing = null) {
    if (existing) {
      this.color = existing.color?.clone ? existing.color.clone() : existing.color;
      this.brushGuid = existing.brushGuid;
      this.brushSize = existing.brushSize;
      this.brushScale = existing.brushScale;
      this.seed = existing.seed;
      this.shapeModifier = existing.shapeModifier;
      this.controlPoints = existing.controlPoints.map(cp => new ControlPoint(
        cp.pos?.clone ? cp.pos.clone() : cp.pos,
        cp.orient?.clone ? cp.orient.clone() : cp.orient,
        cp.pressure,
        cp.timestampMs
      ));
    } else {
      this.color = new Color();
      this.brushGuid = null;
      this.brushSize = 0;
      this.brushScale = 1;
      this.seed = 0;
      this.controlPoints = [];
      this.shapeModifier = null;
    }

    const uuidFn = globalThis.crypto && typeof globalThis.crypto.randomUUID === 'function'
      ? () => globalThis.crypto.randomUUID()
      : () => Math.random().toString(36).slice(2);

    this.guid = uuidFn();
  }
}

export default StrokeData;
