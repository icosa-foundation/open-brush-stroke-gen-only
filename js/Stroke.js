import { Vector3, Quaternion } from 'three';
import { StrokeData } from './StrokeData.js';

export const StrokeType = Object.freeze({
  NotCreated: 0,
  BrushStroke: 1,
  BatchedBrushStroke: 2,
});

/**
 * Port of Tilt Brush's Stroke class. Handles stroke metadata and basic
 * transformation logic without any Unity-specific rendering code.
 */
export class Stroke extends StrokeData {
  constructor(existing = null) {
    super(existing);
    this.type = StrokeType.NotCreated;
    this.intendedCanvas = null;
    this.object = null;
    this.copyForSaveThread = null;
    this.controlPointsToDrop = existing ? [...(existing.controlPointsToDrop || [])] : [];
  }

  get canvas() {
    if (this.type === StrokeType.NotCreated) {
      return this.intendedCanvas;
    }
    if (this.type === StrokeType.BrushStroke) {
      return this.object ? this.object.canvas : null;
    }
    throw new Error('Invalid stroke type');
  }

  invalidateCopy() {
    this.copyForSaveThread = null;
  }

  uncreate() {
    this.intendedCanvas = this.canvas;
    this.object = null;
    this.type = StrokeType.NotCreated;
  }

  setParent(canvas) {
    const prevCanvas = this.canvas;
    if (prevCanvas === canvas) {
      return;
    }

    if (this.type === StrokeType.BrushStroke && this.object && typeof this.object.setParent === 'function') {
      this.object.setParent(canvas);
    } else {
      this.intendedCanvas = canvas;
    }
  }

  /**
   * Applies a left transform to all control points. The transform should be an
   * object with { position: Vector3, rotation: Quaternion, scale: number }.
   * The brushScale is updated by the transform and the cached copy is invalidated.
   */
  leftTransformControlPoints(transform, absoluteScale = false) {
    let position, rotation, scale;
    if (transform && transform.translation) {
      position = transform.translation;
      rotation = transform.rotation;
      scale = transform.scale;
    } else {
      ({ position = new Vector3(), rotation = new Quaternion(), scale = 1 } = transform || {});
    }
    for (const cp of this.controlPoints) {
      cp.pos.multiplyScalar(scale);
      cp.pos.applyQuaternion(rotation);
      cp.pos.add(position);
      cp.orient.premultiply(rotation);
    }
    this.brushScale *= absoluteScale ? Math.abs(scale) : scale;
    this.invalidateCopy();
  }

  // TODO: port matrix-based variant
  leftTransformControlPointsMatrix(matrix) {
    // TODO: implement using Matrix4 operations
    throw new Error('leftTransformControlPointsMatrix not implemented');
  }

  /**
   * Set the parent canvas while preserving world-space position.
   * If leftTransform is provided, it will be applied to the control points.
   * When geometry exists, the stroke is recreated through the provided pointer.
   */
  setParentKeepWorldPosition(canvas, pointer, leftTransform = null, absoluteScale = false) {
    const prevCanvas = this.canvas;
    if (prevCanvas === canvas) {
      return;
    }

    const transform = leftTransform;
    if (this.type === StrokeType.NotCreated || !transform) {
      this.setParent(canvas);
      if (transform) {
        this.leftTransformControlPoints(transform, absoluteScale);
      }
    } else if (this.type === StrokeType.BrushStroke) {
      this.uncreate();
      this.intendedCanvas = canvas;
      this.leftTransformControlPoints(transform, absoluteScale);
      if (pointer && typeof pointer.recreateLineFromMemory === 'function') {
        pointer.recreateLineFromMemory(this);
      }
    } else {
      this.setParent(canvas);
    }
  }

  /**
   * Hide or show the stroke's geometry if possible.
   */
  hide(hide) {
    if (this.type === StrokeType.BrushStroke && this.object && typeof this.object.hideBrush === 'function') {
      this.object.hideBrush(hide);
    } else if (this.type === StrokeType.NotCreated) {
      console.error('Unexpected: NotCreated stroke');
    }
  }

  // Placeholder for full recreate logic; only handles basic transform/parenting.
  recreate(pointer, leftTransform = null, canvas = null, absoluteScale = false) {
    if (leftTransform || this.type === StrokeType.NotCreated) {
      this.uncreate();
      if (canvas) {
        this.setParent(canvas);
      }
      if (leftTransform) {
        this.leftTransformControlPoints(leftTransform, absoluteScale);
      }
      if (pointer && typeof pointer.recreateLineFromMemory === 'function') {
        pointer.recreateLineFromMemory(this);
      }
    } else if (canvas) {
      this.setParent(canvas);
    } else {
      throw new Error('Nothing to do');
    }
  }
}

export default Stroke;
