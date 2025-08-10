import {
  CatmullRomCurve3,
  Mesh,
  MeshStandardMaterial,
  TubeGeometry,
} from 'three';
import { GeometryBrush } from './GeometryBrush.js';
import { ControlPoint } from './ControlPoint.js';

// Port of Tilt Brush's TubeBrush. This simplified version builds a tube mesh
// along the collected control points using Three.js's TubeGeometry.
export class TubeBrush extends GeometryBrush {
  constructor() {
    super({ canBatch: true, upperBoundVertsPerKnot: 24, doubleSided: false });
    this.pointsInClosedCircle = 8;
  }

  // Generate a tube mesh following the stored control points.
  createMesh() {
    if (this.controlPoints.length < 2) {
      return null;
    }
    const curvePoints = this.controlPoints.map(cp => cp.pos.clone());
    const curve = new CatmullRomCurve3(curvePoints, false);
    const tubularSegments = Math.max(32, curvePoints.length * 8);
    const radius = this.BaseSize_LS || 0.01;
    const radialSegments = this.pointsInClosedCircle;
    const closed = false;
    const geometry = new TubeGeometry(curve, tubularSegments, radius, radialSegments, closed);
    const material = new MeshStandardMaterial({ color: this.CurrentColor });
    return new Mesh(geometry, material);
  }
}

export default TubeBrush;
