import { Vector3 } from 'three';
import TrTransform from './TrTransform.js';

/**
 * Helpers for working with three.js Object3D transforms
 * using Tilt Brush's TrTransform abstraction.
 */
export function getUniformScale(xf) {
  let uniformScale = xf.scale.x;
  for (let cur = xf.parent; cur; cur = cur.parent) {
    uniformScale *= cur.scale.x;
  }
  return uniformScale;
}

export function setUniformScale(xf, scale) {
  if (xf.parent) {
    scale /= getUniformScale(xf.parent);
  }
  xf.scale.setScalar(scale);
}

export class LocalAccessor {
  get(xf) {
    return TrTransform.fromLocalTransform(xf);
  }

  set(xf, value) {
    value.toLocalTransform(xf);
  }
}

export class GlobalAccessor {
  get(xf) {
    return TrTransform.fromTransform(xf);
  }

  set(xf, value) {
    value.toTransform(xf);
  }
}

export class RelativeAccessor {
  constructor(parent) {
    this.parent = parent;
    this.asGlobal = new GlobalAccessor();
    this.asLocal = new LocalAccessor();
  }

  get(xf) {
    // TODO: compute transform relative to parent
    throw new Error('RelativeAccessor.get not implemented');
  }

  set(xf, value) {
    // TODO: set transform relative to parent
    throw new Error('RelativeAccessor.set not implemented');
  }
}

export default {
  getUniformScale,
  setUniformScale,
  LocalAccessor,
  GlobalAccessor,
  RelativeAccessor,
};
