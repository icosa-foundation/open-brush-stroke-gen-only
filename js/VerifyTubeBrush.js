import { Vector3, Quaternion, Matrix4 } from 'three';
import { ControlPoint } from './ControlPoint.js';
import TubeBrush from './TubeBrush.js';
import { TrTransform } from './TrTransform.js';

// Build a circular stroke similar to the minimal example
function buildCircleControlPoints(segments = 16, radius = 1.5) {
  const cps = [];
  for (let i = 0; i < segments; i++) {
    const angle = (i * 2 * Math.PI) / segments;
    const position = new Vector3(Math.cos(angle) * radius, Math.sin(angle) * radius, 0);
    const radial = new Vector3(Math.cos(angle), Math.sin(angle), 0);
    const tangent = new Vector3(-Math.sin(angle), Math.cos(angle), 0);
    const binormal = new Vector3().crossVectors(tangent, radial).normalize();
    const matrix = new Matrix4().makeBasis(radial, binormal, tangent);
    const rotation = new Quaternion().setFromRotationMatrix(matrix);
    cps.push(new ControlPoint(position, rotation, 1, i));
  }
  // close loop
  const first = cps[0];
  cps.push(new ControlPoint(first.pos.clone(), first.orient.clone(), first.pressure, segments));
  return cps;
}

function buildLineControlPoints(count = 8, spacing = 0.1) {
  const cps = [];
  for (let i = 0; i < count; i++) {
    const position = new Vector3(i * spacing, 0, 0);
    const radial = new Vector3(0, 1, 0);
    const tangent = new Vector3(1, 0, 0);
    const binormal = new Vector3(0, 0, 1);
    const matrix = new Matrix4().makeBasis(radial, binormal, tangent);
    const rotation = new Quaternion().setFromRotationMatrix(matrix);
    cps.push(new ControlPoint(position, rotation, 1, i));
  }
  return cps;
}

function verifyTubeBrush() {
  const cps = buildCircleControlPoints();
  const brush = new TubeBrush();
  brush.BaseSize_PS = 0.05;
  brush.initBrush({ m_Guid: 'tube-brush' }, TrTransform.identity);
  for (const cp of cps) {
    brush.addControlPoint(cp);
  }
  const mesh = brush.createMesh();
  if (!mesh) {
    throw new Error('TubeBrush returned null mesh');
  }
  const radialSegments = brush.pointsInClosedCircle;
  const radius = brush.BaseSize_LS || 0.01;
  const positions = mesh.geometry.getAttribute('position');
  const normalsAttr = mesh.geometry.getAttribute('normal');
  const pos = new Vector3();
  const normal = new Vector3();

  if (positions.count !== cps.length * radialSegments) {
    throw new Error(`Vertex count ${positions.count} does not match expected ${cps.length * radialSegments}`);
  }

  let maxError = 0;
  let minDistFromOrigin = Infinity;
  let minNormalDot = 1;
  for (let i = 0; i < cps.length; i++) {
    const center = cps[i].pos;
    for (let j = 0; j < radialSegments; j++) {
      pos.fromBufferAttribute(positions, i * radialSegments + j);
      const dist = pos.distanceTo(center);
      const error = Math.abs(dist - radius);
      if (error > maxError) maxError = error;
      const fromOrigin = pos.length();
      if (fromOrigin < minDistFromOrigin) minDistFromOrigin = fromOrigin;
      normal.fromBufferAttribute(normalsAttr, i * radialSegments + j);
      const outward = normal.dot(pos.clone().sub(center).normalize());
      if (outward < minNormalDot) minNormalDot = outward;
    }
  }

  console.log(`Max radial error: ${maxError}`);
  console.log(`Closest vertex distance from origin: ${minDistFromOrigin}`);

  if (maxError > 1e-3) {
    throw new Error('Radial variance exceeds tolerance');
  }
  if (minDistFromOrigin < radius * 0.5) {
    throw new Error('Detected vertex too close to origin');
  }
  if (minNormalDot < 0.5) {
    throw new Error('Normals appear to be flipped or inaccurate');
  }

  // Ensure triangle winding matches vertex normals by comparing face normals
  // against the normal of the first vertex in each triangle.
  const indexAttr = mesh.geometry.getIndex();
  const indicesArr = indexAttr.array;
  const v0 = new Vector3();
  const v1 = new Vector3();
  const v2 = new Vector3();
  const n0 = new Vector3();
  const tri = new Vector3();
  for (let i = 0; i < indicesArr.length; i += 3) {
    v0.fromBufferAttribute(positions, indicesArr[i]);
    v1.fromBufferAttribute(positions, indicesArr[i + 1]);
    v2.fromBufferAttribute(positions, indicesArr[i + 2]);
    tri.subVectors(v1, v0).cross(v2.clone().sub(v0)).normalize();
    n0.fromBufferAttribute(normalsAttr, indicesArr[i]);
    if (tri.dot(n0) < 0.5) {
      throw new Error('Triangle winding is inconsistent with vertex normals');
    }
  }
  console.log('TubeBrush geometry passed basic checks');
}

function verifySquareCrossSection() {
  const cps = buildCircleControlPoints();
  const brush = new TubeBrush();
  brush.BaseSize_PS = 0.05;
  brush.crossSection = TubeBrush.CrossSection.SQUARE;
  brush.initBrush({ m_Guid: 'tube-brush' }, TrTransform.identity);
  for (const cp of cps) {
    brush.addControlPoint(cp);
  }
  const mesh = brush.createMesh();
  if (!mesh) {
    throw new Error('TubeBrush returned null mesh for square shape');
  }
  const radialSegments = brush.pointsInClosedCircle;
  const radius = brush.BaseSize_LS || 0.01;
  const halfW = radius;
  const halfH = radius * TubeBrush.kCrossSectionAspect;
  const positions = mesh.geometry.getAttribute('position');
  const pos = new Vector3();

  const local = new Vector3();
  const inv = new Quaternion();
  let maxError = 0;
  for (let i = 0; i < cps.length; i++) {
    const center = cps[i].pos;
    inv.copy(cps[i].orient).invert();
    for (let j = 0; j < radialSegments; j++) {
      pos.fromBufferAttribute(positions, i * radialSegments + j);
      local.copy(pos).sub(center).applyQuaternion(inv);
      const xAbs = Math.abs(local.x);
      const yAbs = Math.abs(local.y);
      // ensure vertex lies on rectangle boundary within tolerance
      const edgeError = Math.min(Math.abs(xAbs - halfW), Math.abs(yAbs - halfH));
      if (edgeError > maxError) maxError = edgeError;
      if (xAbs > halfW + 1e-3 || yAbs > halfH + 1e-3) {
        throw new Error('Vertex outside expected square bounds');
      }
    }
  }

  console.log(`Max square error: ${maxError}`);
  if (maxError > 1e-3) {
    throw new Error('Square shape variance exceeds tolerance');
  }
  console.log('TubeBrush square cross-section passed basic checks');
}

function verifyTaperShape() {
  const cps = buildLineControlPoints();
  const brush = new TubeBrush();
  brush.BaseSize_PS = 0.05;
  brush.shapeModifier = TubeBrush.ShapeModifier.TAPER;
  brush.initBrush({ m_Guid: 'tube-brush' }, TrTransform.identity);
  for (const cp of cps) {
    brush.addControlPoint(cp);
  }
  const mesh = brush.createMesh();
  if (!mesh) {
    throw new Error('TubeBrush returned null mesh for taper modifier');
  }
  const radialSegments = brush.pointsInClosedCircle;
  const positions = mesh.geometry.getAttribute('position');
  const pos = new Vector3();

  const firstCenter = cps[0].pos;
  const lastCenter = cps[cps.length - 1].pos;
  let firstRadius = 0;
  let lastRadius = 0;
  for (let j = 0; j < radialSegments; j++) {
    pos.fromBufferAttribute(positions, j);
    firstRadius += pos.distanceTo(firstCenter);
    pos.fromBufferAttribute(positions, (cps.length - 1) * radialSegments + j);
    lastRadius += pos.distanceTo(lastCenter);
  }
  firstRadius /= radialSegments;
  lastRadius /= radialSegments;

  console.log(`First ring radius: ${firstRadius}`);
  console.log(`Last ring radius: ${lastRadius}`);
  if (lastRadius > firstRadius * 0.25) {
    throw new Error('Taper modifier did not reduce radius sufficiently');
  }
  console.log('TubeBrush taper shape passed basic checks');
}

verifyTubeBrush();
verifySquareCrossSection();
verifyTaperShape();
