import { Quaternion, Vector3 } from 'three';

/**
 * Utilities mirroring Tilt Brush's QuaternionExtensions.
 */
export default class QuaternionExtensions {
  /**
   * Creates a quaternion rotating by `angle` radians around `axis`.
   */
  static angleAxisRad(angle, axis) {
    const q = new Quaternion();
    q.setFromAxisAngle(axis, angle);
    return q;
  }

  /**
   * Quaternion logarithm; returns a quaternion whose xyz represent half-angle.
   * The input must be unit-length.
   */
  static log(q) {
    const vecLenSq = q.x * q.x + q.y * q.y + q.z * q.z;
    const lenSq = vecLenSq + q.w * q.w;
    if (Math.abs(lenSq - 1) > 3e-3) {
      throw new Error('Quaternion must be unit');
    }

    const sinTheta = Math.sqrt(vecLenSq);
    const theta = Math.atan2(sinTheta, q.w);

    if (sinTheta < 1e-5) {
      if (q.w > 0) {
        return new Quaternion(q.x, q.y, q.z, 0);
      }
      const axis = new Vector3(q.x, q.y, q.z).normalize();
      if (axis.lengthSq() === 0) axis.set(0, 1, 0);
      axis.multiplyScalar(theta);
      return new Quaternion(axis.x, axis.y, axis.z, 0);
    } else {
      const k = theta / sinTheta;
      return new Quaternion(k * q.x, k * q.y, k * q.z, 0);
    }
  }

  /**
   * Quaternion exponentiation; input must have w === 0.
   */
  static exp(q) {
    if (q.w !== 0) {
      throw new Error('Quaternion must be pure (w=0)');
    }
    const v = new Vector3(q.x, q.y, q.z);
    const vLen = v.length();
    let sinVOverV;
    if (vLen < 1e-4) {
      sinVOverV = vLen;
    } else {
      sinVOverV = Math.sin(vLen) / vLen;
    }

    v.multiplyScalar(sinVOverV);
    return new Quaternion(v.x, v.y, v.z, Math.cos(vLen));
  }

  /**
   * Returns the negated quaternion.
   */
  static negated(q) {
    return new Quaternion(-q.x, -q.y, -q.z, -q.w);
  }

  /**
   * Returns the imaginary component of the quaternion.
   */
  static im(q) {
    return new Vector3(q.x, q.y, q.z);
  }

  /**
   * Returns the inverse without assuming unit-length.
   */
  static trueInverse(q) {
    const f = 1 / q.lengthSq();
    return new Quaternion(-q.x * f, -q.y * f, -q.z * f, q.w * f);
  }
}
