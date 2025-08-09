import { Group, Color } from 'three';
import { TrTransform } from './TrTransform.js';

export class BaseBrush {
  constructor(canBatch = false) {
    this.m_bCanBatch = canBatch;
    this.m_Desc = null;
    this.m_EnableBackfaces = false;
    this.m_PreviewMode = false;
    this.m_IsLoading = false;
    this.m_Color = new Color();
    this.m_LastSpawnXf = TrTransform.identity;
    this.m_BaseSize_PS = 0;
    this.m_rng = { seed: 0 };
    this.group = new Group();
  }

  static create(parent, xfInParentSpace, desc, color, size_PS) {
    const brush = new BaseBrush();
    brush.group.name = desc?.description || desc?.durableName || 'BrushStroke';
    if (parent && parent.add) {
      parent.add(brush.group);
    }
    brush.m_Desc = desc;
    brush.m_Color.copy(color);
    brush.m_BaseSize_PS = size_PS;
    brush.m_LastSpawnXf = xfInParentSpace;
    brush.initBrush(desc, xfInParentSpace);
    return brush;
  }

  get StrokeScale() {
    return this.m_LastSpawnXf.scale;
  }

  get LOCAL_TO_POINTER() {
    return 1 / this.m_LastSpawnXf.scale;
  }

  get POINTER_TO_LOCAL() {
    return this.m_LastSpawnXf.scale;
  }

  get BaseSize_PS() {
    return this.m_BaseSize_PS;
  }

  set BaseSize_PS(v) {
    this.m_BaseSize_PS = v;
  }

  get BaseSize_LS() {
    return this.m_BaseSize_PS * this.POINTER_TO_LOCAL;
  }

  get Descriptor() {
    return this.m_Desc;
  }

  get CurrentColor() {
    return this.m_Color;
  }

  get RandomSeed() {
    return this.m_rng.seed || 0;
  }

  set RandomSeed(value) {
    this.m_rng.seed = value;
  }

  setIsLoading() {
    this.m_IsLoading = true;
  }

  setPreviewMode() {
    this.m_PreviewMode = true;
  }

  // Stub methods for subclasses to override
  initBrush(desc, xfInParentSpace) {
    // TODO: implement brush-specific initialization
  }

  addControlPoint(cp, pointer, pressure, timestamp) {
    // TODO: handle control point addition
  }

  finalizeStroke() {
    // TODO: finalize stroke geometry
  }
}

export default BaseBrush;
