import { Color } from 'three';

function colorCalc(c, t1, t2) {
  if (c < 0) {
    c += 6;
  } else if (c >= 6) {
    c -= 6;
  }
  if (c < 1) return t1 + (t2 - t1) * c;
  if (c < 3) return t2;
  if (c < 4) return t1 + (t2 - t1) * (4 - c);
  return t1;
}

export class HSLColor {
  static HUE_MAX = 6;

  constructor(h = 0, s = 0, l = 0, a = 1) {
    h = h % HSLColor.HUE_MAX;
    if (h < 0) h += HSLColor.HUE_MAX;
    this.h = h;
    this.s = s;
    this.l = l;
    this.a = a;
  }

  get hueDegrees() {
    return this.h * (360 / HSLColor.HUE_MAX);
  }

  set hueDegrees(value) {
    value = ((value * HSLColor.HUE_MAX) / 360) % HSLColor.HUE_MAX;
    if (value < 0) value += HSLColor.HUE_MAX;
    this.h = value;
  }

  get hue01() {
    return this.h * (1 / HSLColor.HUE_MAX);
  }

  set hue01(value) {
    value = (value * HSLColor.HUE_MAX) % HSLColor.HUE_MAX;
    if (value < 0) value += HSLColor.HUE_MAX;
    this.h = value;
  }

  toColor() {
    if (this.s === 0) {
      const gray = new Color(this.l, this.l, this.l);
      return { color: gray, a: this.a };
    }
    let t2;
    if (this.l < 0.5) {
      t2 = this.l * (1 + this.s);
    } else {
      t2 = this.l + this.s - this.l * this.s;
    }
    const t1 = 2 * this.l - t2;
    const th = this.h * (6 / HSLColor.HUE_MAX);
    const tr = th + 2;
    const tg = th;
    const tb = th - 2;
    const color = new Color(
      colorCalc(tr, t1, t2),
      colorCalc(tg, t1, t2),
      colorCalc(tb, t1, t2)
    );
    return { color, a: this.a };
  }

  static fromColor(color, a = 1) {
    const r = color.r;
    const g = color.g;
    const b = color.b;
    const min = Math.min(r, g, b);
    const max = Math.max(r, g, b);
    const delta = max - min;
    let h = 0;
    let s = 0;
    const l = (max + min) * 0.5;
    if (delta !== 0) {
      if (l < 0.5) {
        s = delta / (max + min);
      } else {
        s = delta / (2 - max - min);
      }
      if (r === max) {
        h = (g - b) / delta;
      } else if (g === max) {
        h = 2 + (b - r) / delta;
      } else {
        h = 4 + (r - g) / delta;
      }
    }
    h *= HSLColor.HUE_MAX / 6;
    return new HSLColor(h, s, l, a);
  }

  static fromHSV(h, s, v, a = 1) {
    h = h % HSLColor.HUE_MAX;
    if (h < 0) h += HSLColor.HUE_MAX;
    const l = v - 0.5 * s * v;
    let s2;
    if (l <= 0.5) {
      s2 = s / (2 - s);
    } else {
      s2 = (s * v) / (2 * (1 - v) + s * v);
    }
    return new HSLColor(h, s2, l, a);
  }

  getBaseColor() {
    return new HSLColor(this.h, this.s, 0.5, this.a);
  }

  toString() {
    return `HSLA(${this.h.toFixed(3)}, ${this.s.toFixed(3)}, ${this.l.toFixed(3)}, ${this.a.toFixed(3)})`;
  }
}

export default HSLColor;
