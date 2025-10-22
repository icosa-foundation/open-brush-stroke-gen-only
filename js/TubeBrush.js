import {
  BufferGeometry,
  Float32BufferAttribute,
  Mesh,
  MeshStandardMaterial,
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
    // Cross-section shape (round by default)
    this.crossSection = TubeBrush.CrossSection.ROUND;
    // Shape modifier along the length of the stroke
    this.shapeModifier = TubeBrush.ShapeModifier.NONE;
    // How UVs are generated along the stroke
    this.uvStyle = TubeBrush.UVStyle.DISTANCE;
    // Specific to Taper modifier
    this.taperScalar = 1.0;
    // Specific to Petal modifier
    this.petalDisplacementAmt = 0.5;
    this.petalDisplacementExp = 3.0;
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

    const baseRadius = this.BaseSize_LS || 0.01;
    const radialSegments = this.pointsInClosedCircle;

    const positions = [];
    const normals = [];
    const uvs = [];
    const indices = [];

    const tmpPos = new Vector3();
    const tmpNormal = new Vector3();
    const right = new Vector3();
    const up = new Vector3();

    // Precompute cumulative lengths for distance-based UVs
    const lengths = new Array(cpCount).fill(0);
    for (let i = 1; i < cpCount; i++) {
      lengths[i] = lengths[i - 1] + this.controlPoints[i].pos.distanceTo(this.controlPoints[i - 1].pos);
    }
    const totalLength = lengths[cpCount - 1];

    // Precomputed square cross-section coordinates and normals in local space
    const squarePos = [
      [1,  1],
      [1,  0],
      [1, -1],
      [0, -1],
      [-1, -1],
      [-1, 0],
      [-1, 1],
      [0,  1],
    ];
    const squareNorm = [
      [1, 0],
      [1, 0],
      [1, 0],
      [0, -1],
      [-1, 0],
      [-1, 0],
      [-1, 0],
      [0, 1],
    ];

    for (let i = 0; i < cpCount; i++) {
      const cp = this.controlPoints[i];
      const t = i / (cpCount - 1);
      let curve = 1;
      let offsetAmt = 0;
      switch (this.shapeModifier) {
        case TubeBrush.ShapeModifier.TAPER:
          curve = this.taperScalar * (1 - t);
          break;
        case TubeBrush.ShapeModifier.SIN:
          curve = Math.abs(Math.sin(t * Math.PI));
          break;
        case TubeBrush.ShapeModifier.COMET:
          curve = Math.sin(t * 1.5 + 1.55);
          break;
        case TubeBrush.ShapeModifier.PETAL:
          curve = Math.abs(Math.sin(t * Math.PI));
          offsetAmt = Math.pow(t, this.petalDisplacementExp) *
            this.petalDisplacementAmt * baseRadius * cp.pressure;
          break;
        case TubeBrush.ShapeModifier.DOUBLE_SIDED_TAPER:
          // Radius grows from both ends toward the middle and then tapers
          // back down, forming a symmetrical pointy shape.
          curve = 1 - Math.abs(2 * t - 1);
          break;
      }
      let radius = baseRadius * curve;
      right.set(1, 0, 0).applyQuaternion(cp.orient);
      up.set(0, 1, 0).applyQuaternion(cp.orient);
      const halfW = radius;
      const halfH = radius * TubeBrush.kCrossSectionAspect;

      for (let j = 0; j < radialSegments; j++) {
        if (this.crossSection === TubeBrush.CrossSection.SQUARE) {
          const sx = squarePos[j][0] * halfW;
          const sy = squarePos[j][1] * halfH;
          tmpPos.copy(cp.pos)
            .addScaledVector(right, sx)
            .addScaledVector(up, sy);
          tmpNormal.copy(right).multiplyScalar(squareNorm[j][0])
            .addScaledVector(up, squareNorm[j][1])
            .normalize();
        } else {
          const angle = (j / radialSegments) * Math.PI * 2;
          tmpPos.set(Math.cos(angle) * halfW, Math.sin(angle) * halfW, 0);
          tmpNormal.set(Math.cos(angle), Math.sin(angle), 0);
          tmpPos.applyQuaternion(cp.orient).add(cp.pos);
          tmpNormal.applyQuaternion(cp.orient);
        }
        if (offsetAmt !== 0) {
          tmpPos.addScaledVector(tmpNormal, offsetAmt);
        }
        positions.push(tmpPos.x, tmpPos.y, tmpPos.z);
        normals.push(tmpNormal.x, tmpNormal.y, tmpNormal.z);
        const v = (this.uvStyle === TubeBrush.UVStyle.STRETCH || totalLength === 0)
          ? i / (cpCount - 1)
          : lengths[i] / totalLength;
        uvs.push(j / radialSegments, v);
      }
    }

    for (let i = 0; i < cpCount - 1; i++) {
      for (let j = 0; j < radialSegments; j++) {
        const a = i * radialSegments + j;
        const b = (i + 1) * radialSegments + j;
        const c = i * radialSegments + (j + 1) % radialSegments;
        const d = (i + 1) * radialSegments + (j + 1) % radialSegments;

        // Form two triangles for the quad between ring i and i+1 at segment j.
        // Winding is chosen so face normals align with the generated vertex normals.
        indices.push(a, c, b);
        indices.push(c, d, b);
      }
    }

    const geometry = new BufferGeometry();
    geometry.setAttribute('position', new Float32BufferAttribute(positions, 3));
    geometry.setAttribute('normal', new Float32BufferAttribute(normals, 3));
    geometry.setAttribute('uv', new Float32BufferAttribute(uvs, 2));
    geometry.setIndex(indices);

    // Use a lit material so normals can be visually inspected in the demo.
    const material = new MeshStandardMaterial({ color: this.CurrentColor });
    return new Mesh(geometry, material);
  }
}

// Cross-section shapes
TubeBrush.CrossSection = {
  ROUND: 'round',
  SQUARE: 'square',
};

// Shape modifiers along the stroke length
TubeBrush.ShapeModifier = {
  NONE: 'none',
  DOUBLE_SIDED_TAPER: 'doubleSidedTaper',
  SIN: 'sin',
  COMET: 'comet',
  TAPER: 'taper',
  PETAL: 'petal',
};

// UV mapping styles
TubeBrush.UVStyle = {
  DISTANCE: 'distance',
  STRETCH: 'stretch',
};

// Height/width ratio used by the square cross-section, mirroring the C# constant
TubeBrush.kCrossSectionAspect = 0.375;

export default TubeBrush;
