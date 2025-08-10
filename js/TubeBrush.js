import {
  BufferGeometry,
  Float32BufferAttribute,
  Mesh,
  MeshBasicMaterial,
  Vector3,
} from 'three';
import { GeometryBrush } from './GeometryBrush.js';

// Port of Tilt Brush's TubeBrush. This version builds a tube mesh
// along the collected control points using custom BufferGeometry rather
// than Three.js's TubeGeometry helper.
export class TubeBrush extends GeometryBrush {
  constructor() {
    super({ canBatch: true, upperBoundVertsPerKnot: 24, doubleSided: false });
    this.pointsInClosedCircle = 8;
  }

  initBrush(desc, localPointerXf) {
    super.initBrush(desc, localPointerXf);
    // TODO: mirror TubeBrush-specific initialization from C# version
  }

  resetBrushForPreview(unusedXf) {
    super.resetBrushForPreview(unusedXf);
    // TODO: clear any TubeBrush preview-specific state
  }

  addControlPoint(cp) {
    super.addControlPoint(cp);
    this.controlPointsChanged(this.controlPoints.length - 1);
  }

  getSpawnInterval(pressure01) {
    // TODO: compute spawn interval based on pressure and descriptor
    return this.Descriptor?.m_SolidMinLengthMeters_PS || 0.002;
  }

  controlPointsChanged(startIndex) {
    // For now, rebuild the entire mesh whenever control points change.
    // This mirrors the C# behavior that regenerates geometry incrementally,
    // but keeps the implementation simple until partial updates are ported.
    if (this.controlPoints.length < 2) {
      return;
    }
    // Recreate the mesh to reflect new control points.
    this.finalizeStroke();
  }

  onChangedFrameKnots(startIndex) {
    // TODO: frame knots and detect strip breaks
    return false;
  }

  onChangedMakeGeometry(startIndex) {
    // TODO: generate mesh data for affected knots
  }

  resizeGeometry() {
    // TODO: resize internal geometry buffers
  }

  onChangedStretchUVs(startIndex) {
    // TODO: update UVs when stretch style is active
  }

  onChangedModifySilhouette(startIndex) {
    // TODO: apply shape modifiers to the silhouette
  }

  // Generate a tube mesh along control points without relying on Three.js helpers.
  createMesh() {
    const cpCount = this.controlPoints.length;
    if (cpCount < 2) {
      return null;
    }

    const radius = this.BaseSize_LS || 0.01;
    const radialSegments = this.pointsInClosedCircle;

    const positions = [];
    const normals = [];
    const uvs = [];
    const indices = [];

    const tmpPos = new Vector3();
    const tmpNormal = new Vector3();

    for (let i = 0; i < cpCount; i++) {
      const cp = this.controlPoints[i];
      for (let j = 0; j < radialSegments; j++) {
        const angle = (j / radialSegments) * Math.PI * 2;
        tmpPos.set(Math.cos(angle) * radius, Math.sin(angle) * radius, 0);
        tmpNormal.copy(tmpPos).normalize();
        tmpPos.applyQuaternion(cp.orient).add(cp.pos);
        tmpNormal.applyQuaternion(cp.orient);
        positions.push(tmpPos.x, tmpPos.y, tmpPos.z);
        normals.push(tmpNormal.x, tmpNormal.y, tmpNormal.z);
        uvs.push(j / radialSegments, i / (cpCount - 1));
      }
    }

    for (let i = 0; i < cpCount - 1; i++) {
      for (let j = 0; j < radialSegments; j++) {
        const a = i * radialSegments + j;
        const b = (i + 1) * radialSegments + j;
        const c = i * radialSegments + (j + 1) % radialSegments;
        const d = (i + 1) * radialSegments + (j + 1) % radialSegments;

        // Form two triangles for the quad between ring i and i+1 at segment j.
        indices.push(a, b, d);
        indices.push(a, d, c);
      }
    }

    const geometry = new BufferGeometry();
    geometry.setAttribute('position', new Float32BufferAttribute(positions, 3));
    geometry.setAttribute('normal', new Float32BufferAttribute(normals, 3));
    geometry.setAttribute('uv', new Float32BufferAttribute(uvs, 2));
    geometry.setIndex(indices);

    // Use a basic material so the stroke is visible without scene lighting.
    const material = new MeshBasicMaterial({ color: this.CurrentColor });
    return new Mesh(geometry, material);
  }
}

export default TubeBrush;
