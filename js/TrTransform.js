import { Vector3, Quaternion, Matrix4 } from 'three';
import QuaternionExtensions from './QuaternionExtensions.js';

/**
 * Port of Tilt Brush's TrTransform struct.
 * Represents translation, rotation, and uniform scale.
 */
export class TrTransform {
  constructor(translation = new Vector3(), rotation = new Quaternion(), scale = 1) {
    this.translation = translation;
    this.rotation = rotation;
    this.scale = scale;
  }

  static identity = TrTransform.TR(new Vector3(), new Quaternion());

  // Methods analogous to Matrix4x4.TRS
  static T(t) {
    return new TrTransform(t, new Quaternion(), 1);
  }

  static RQuaternion(r) {
    return new TrTransform(new Vector3(), r, 1);
  }

  static R(angle, axis) {
    const q = new Quaternion().setFromAxisAngle(axis, angle);
    return new TrTransform(new Vector3(), q, 1);
  }

  static S(s) {
    return new TrTransform(new Vector3(), new Quaternion(), s);
  }

  static TR(t, r) {
    return new TrTransform(t, r, 1);
  }

  static TRS(t, r, s) {
    return new TrTransform(t, r, s);
  }

  static fromMatrix4(m) {
    // TODO: decompose matrix; assumes uniform scale
    const t = new Vector3();
    const r = new Quaternion();
    const s = new Vector3();
    m.decompose(t, r, s);
    return new TrTransform(t, r, s.x);
  }

  static fromTransform(xf) {
    if (!xf) {
      throw new Error('fromTransform requires an Object3D');
    }
    xf.updateMatrixWorld();
    const t = new Vector3();
    const r = new Quaternion();
    const s = new Vector3();
    xf.matrixWorld.decompose(t, r, s);
    return new TrTransform(t, r, s.x);
  }

  static fromLocalTransform(xf) {
    if (!xf) {
      throw new Error('fromLocalTransform requires an Object3D');
    }
    const t = xf.position.clone();
    const r = xf.quaternion.clone();
    const s = xf.scale.x;
    return new TrTransform(t, r, s);
  }

  static invMul(a, b) {
    const aInvRot = QuaternionExtensions.trueInverse(a.rotation);
    const translation = b.translation
      .clone()
      .sub(a.translation)
      .divideScalar(a.scale)
      .applyQuaternion(aInvRot);
    const rotation = aInvRot.clone().multiply(b.rotation);
    const scale = b.scale / a.scale;
    return TrTransform.TRS(translation, rotation, scale);
  }

  static lerp(a, b, t) {
    const translation = a.translation.clone().lerp(b.translation, t);
    const rotation = a.rotation.clone().slerp(b.rotation, t);
    const scale = Math.exp(Math.log(a.scale) * (1 - t) + Math.log(b.scale) * t);
    return new TrTransform(translation, rotation, scale);
  }

  multiplyPoint(p) {
    return p
      .clone()
      .multiplyScalar(this.scale)
      .applyQuaternion(this.rotation)
      .add(this.translation);
  }

  multiplyVector(v) {
    return v
      .clone()
      .multiplyScalar(this.scale)
      .applyQuaternion(this.rotation);
  }

  multiplyBivector(v) {
    return v
      .clone()
      .multiplyScalar(this.scale * this.scale)
      .applyQuaternion(this.rotation);
  }

  multiplyNormal(v) {
    return v.clone().applyQuaternion(this.rotation);
  }

  // TODO: implement plane transformation
  multiplyPlane(plane) {
    throw new Error('multiplyPlane not implemented');
  }

  mul(b) {
    return TrTransform.TRS(
      this.rotation.clone().multiply(b.translation.clone().multiplyScalar(this.scale)).add(this.translation),
      this.rotation.clone().multiply(b.rotation),
      this.scale * b.scale
    );
  }

  static approximately(lhs, rhs) {
    const sameTranslation = lhs.translation.equals(rhs.translation);
    const sameRotation = lhs.rotation.equals(rhs.rotation);
    const sameScale = Math.abs(lhs.scale - rhs.scale) <= Number.EPSILON;
    return sameTranslation && sameRotation && sameScale;
  }

  get inverse() {
    const rinv = QuaternionExtensions.trueInverse(this.rotation);
    const invScale = 1 / this.scale;
    const translation = this.translation
      .clone()
      .multiplyScalar(-invScale)
      .applyQuaternion(rinv);
    return TrTransform.TRS(translation, rinv, invScale);
  }

  forward() {
    return new Vector3(0, 0, 1).applyQuaternion(this.rotation);
  }

  up() {
    return new Vector3(0, 1, 0).applyQuaternion(this.rotation);
  }

  right() {
    return new Vector3(1, 0, 0).applyQuaternion(this.rotation);
  }

  isFinite() {
    return [
      this.translation.x,
      this.translation.y,
      this.translation.z,
      this.rotation.x,
      this.rotation.y,
      this.rotation.z,
      this.rotation.w,
      this.scale,
    ].every(Number.isFinite);
  }

  toString() {
    return `T: ${this.translation.x.toExponential()} ${this.translation.y.toExponential()} ${this.translation.z.toExponential()}\n` +
      `R: ${this.rotation.x.toExponential()} ${this.rotation.y.toExponential()} ${this.rotation.z.toExponential()} ${this.rotation.w.toExponential()}\n` +
      `S: ${this.scale.toExponential()}`;
  }

  equals(o) {
    return (
      o instanceof TrTransform &&
      this.translation.equals(o.translation) &&
      this.rotation.equals(o.rotation) &&
      this.scale === o.scale
    );
  }

  toMatrix4() {
    const m = new Matrix4();
    m.compose(
      this.translation,
      this.rotation,
      new Vector3(this.scale, this.scale, this.scale)
    );
    return m;
  }

  toTransform(xf) {
    if (!xf) {
      throw new Error('toTransform requires an Object3D');
    }
    const m = this.toMatrix4();
    if (xf.parent) {
      xf.parent.updateMatrixWorld();
      const parentInv = new Matrix4().copy(xf.parent.matrixWorld).invert();
      m.premultiply(parentInv);
    }
    m.decompose(xf.position, xf.quaternion, xf.scale);
    xf.updateMatrix();
    xf.updateMatrixWorld(true);
  }

  toLocalTransform(xf) {
    if (!xf) {
      throw new Error('toLocalTransform requires an Object3D');
    }
    xf.position.copy(this.translation);
    xf.quaternion.copy(this.rotation);
    xf.scale.setScalar(this.scale);
    xf.updateMatrix();
    xf.updateMatrixWorld(true);
  }

  transformBy(rhs) {
    const similar = rhs.rotation.clone().multiply(this.rotation).multiply(QuaternionExtensions.trueInverse(rhs.rotation));
    const retTrans = similar
      .clone()
      .multiply(rhs.translation.clone().multiplyScalar(-this.scale))
      .add(rhs.rotation.clone().multiply(this.translation.clone().multiplyScalar(rhs.scale)))
      .add(rhs.translation);
    return new TrTransform(retTrans, similar, this.scale);
  }
}

export default TrTransform;
