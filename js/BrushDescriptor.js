export class BrushDescriptor {
  constructor({
    guid = '',
    durableName = '',
    description = '',
    brushPrefab = null,
    tags = ['default'],
    nondeterministic = false,
    supersedes = null,
    looksIdentical = false,
    hiddenInGui = false,
    material = null,
    textureAtlasV = 1,
    tileRate = 1,
    brushSizeRange = [0, 0],
    pressureSizeRange = [0.1, 1],
    sizeVariance = 0,
    previewPressureSizeMin = 0.001,
    pressureOpacityRange = [0, 1],
    opacity = 1,
  } = {}) {
    this.guid = guid;
    this.durableName = durableName;
    this.description = description;
    this.brushPrefab = brushPrefab;
    this.tags = tags;
    this.nondeterministic = nondeterministic;
    this.supersedes = supersedes;
    this.supersededBy = null;
    this.looksIdentical = looksIdentical;
    this.hiddenInGui = hiddenInGui;
    this.material = material;
    this.textureAtlasV = textureAtlasV;
    this.tileRate = tileRate;
    this.brushSizeRange = brushSizeRange;
    this.pressureSizeRange = pressureSizeRange;
    this.sizeVariance = sizeVariance;
    this.previewPressureSizeMin = previewPressureSizeMin;
    this.pressureOpacityRange = pressureOpacityRange;
    this.opacity = opacity;
  }

  pressureSizeMin(previewMode) {
    return previewMode ? this.previewPressureSizeMin : this.pressureSizeRange[0];
  }

  toString() {
    return `BrushDescriptor<${this.description} ${this.guid}>`;
  }
}

export default BrushDescriptor;
