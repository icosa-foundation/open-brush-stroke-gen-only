import { Vector3, Quaternion } from 'three';

export class ControlPoint {
  constructor(pos = new Vector3(), orient = new Quaternion(), pressure = 0, timestampMs = 0) {
    this.pos = pos;
    this.orient = orient;
    this.pressure = pressure;
    this.timestampMs = timestampMs;
  }
}

export default ControlPoint;
