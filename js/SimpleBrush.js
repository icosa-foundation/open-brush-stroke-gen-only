import { BufferGeometry, Float32BufferAttribute, Vector3 } from 'three';

/**
 * Very small brush geometry builder that extrudes a ribbon from control points.
 * This is a placeholder for full Tilt Brush brush implementations.
 */
export class SimpleBrush {
  /**
   * Build a ribbon mesh from control points.
   * @param {ControlPoint[]} controlPoints
   * @param {number} size Brush diameter
   * @returns {BufferGeometry}
   */
  static buildGeometry(controlPoints, size = 1) {
    if (!controlPoints || controlPoints.length < 2) {
      return new BufferGeometry();
    }

    const positions = [];
    const uvs = [];
    const indices = [];
    const half = size / 2;

    for (let i = 0; i < controlPoints.length; i++) {
      const cp = controlPoints[i];
      // Use orientation's X axis as the brush's right vector
      const right = new Vector3(1, 0, 0).applyQuaternion(cp.orient).multiplyScalar(half);
      const v0 = cp.pos.clone().add(right);
      const v1 = cp.pos.clone().sub(right);

      positions.push(v0.x, v0.y, v0.z, v1.x, v1.y, v1.z);
      const t = i / (controlPoints.length - 1);
      uvs.push(0, t, 1, t);

      if (i > 0) {
        const base = i * 2;
        indices.push(base - 2, base - 1, base);
        indices.push(base, base - 1, base + 1);
      }
    }

    const geom = new BufferGeometry();
    geom.setAttribute('position', new Float32BufferAttribute(positions, 3));
    geom.setAttribute('uv', new Float32BufferAttribute(uvs, 2));
    geom.setIndex(indices);
    geom.computeVertexNormals();
    return geom;
  }
}

export default SimpleBrush;
