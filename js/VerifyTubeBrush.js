import { Vector3, Quaternion, Matrix4, Color } from 'three';
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
  const pos = new Vector3();

  if (positions.count !== cps.length * radialSegments) {
    throw new Error(`Vertex count ${positions.count} does not match expected ${cps.length * radialSegments}`);
  }

  let maxError = 0;
  let minDistFromOrigin = Infinity;
  for (let i = 0; i < cps.length; i++) {
    const center = cps[i].pos;
    for (let j = 0; j < radialSegments; j++) {
      pos.fromBufferAttribute(positions, i * radialSegments + j);
      const dist = pos.distanceTo(center);
      const error = Math.abs(dist - radius);
      if (error > maxError) maxError = error;
      const fromOrigin = pos.length();
      if (fromOrigin < minDistFromOrigin) minDistFromOrigin = fromOrigin;
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
  console.log('TubeBrush geometry passed basic checks');
}

function verifySquareModifier() {
  const cps = buildCircleControlPoints();
  const brush = new TubeBrush();
  brush.BaseSize_PS = 0.05;
  brush.shapeModifier = TubeBrush.ShapeModifier.SQUARE;
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
      const maxComponent = Math.max(Math.abs(local.x), Math.abs(local.y));
      const error = Math.abs(maxComponent - radius);
      if (error > maxError) maxError = error;
    }
  }

  console.log(`Max square error: ${maxError}`);
  if (maxError > 1e-3) {
    throw new Error('Square shape variance exceeds tolerance');
  }
  console.log('TubeBrush square modifier passed basic checks');
}

function verifyTaperModifier() {
  const cps = buildLineControlPoints();
  const brush = new TubeBrush();
  brush.BaseSize_PS = 0.05;
  brush.silhouetteModifier = TubeBrush.SilhouetteModifier.TAPER;
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
  console.log('TubeBrush taper modifier passed basic checks');
}

verifyTubeBrush();
verifySquareModifier();
verifyTaperModifier();
