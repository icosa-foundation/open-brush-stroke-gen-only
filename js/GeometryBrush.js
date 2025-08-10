import { BaseBrush } from './BaseBrush.js';

// Simplified port of Tilt Brush's GeometryBrush base class.
// Handles control-point collection and delegates mesh creation to subclasses.
export class GeometryBrush extends BaseBrush {
  constructor({ canBatch = true, upperBoundVertsPerKnot = 0, doubleSided = false } = {}) {
    super(canBatch);
    this.m_UpperBoundVertsPerKnot = upperBoundVertsPerKnot;
    this.m_bDoubleSided = doubleSided;
    this.controlPoints = [];
    this.mesh = null;
  }

  initBrush(desc, localPointerXf) {
    super.initBrush(desc, localPointerXf);
    this.controlPoints.length = 0;
  }

  // Store control points for later geometry generation.
  addControlPoint(cp) {
    this.controlPoints.push(cp);
  }

  // Subclasses should override to build their specific mesh.
  createMesh() {
    // TODO: implement in subclasses
    return null;
  }

  finalizeStroke() {
    if (this.mesh) {
      this.group.remove(this.mesh);
      this.mesh.geometry.dispose();
      this.mesh.material.dispose();
    }
    const mesh = this.createMesh();
    if (mesh) {
      this.mesh = mesh;
      this.group.add(mesh);
    }
  }
}

export default GeometryBrush;
