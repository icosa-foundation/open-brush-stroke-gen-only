// Port of Tilt Brush's BrushDescriptor ScriptableObject to JavaScript.
// Fields mirror the C# version to aid in porting brush metadata.

export default class BrushDescriptor {
  constructor() {
    // Identity
    this.m_Guid = '';
    this.m_DurableName = '';
    this.m_CreationVersion = '';
    this.m_ShaderVersion = '10.0';
    this.m_BrushPrefab = null;
    this.m_Tags = ['default'];
    this.m_Nondeterministic = false;
    this.m_Supersedes = null;
    this.m_SupersededBy = null; // set during catalog init
    this.m_LooksIdentical = false;

    // GUI
    this.m_ButtonTexture = null;
    this.m_LocalizedDescription = null; // stub for Unity LocalizedString
    this.m_DescriptionExtra = '';
    this.m_HiddenInGui = false;

    // Material
    this.m_Material = null;
    this.m_TextureAtlasV = 1;
    this.m_TileRate = 1;

    // Size
    this.m_BrushSizeRange = [0, 0];
    this.m_PressureSizeRange = [0.1, 1];
    this.m_SizeVariance = 0;
    this.m_PreviewPressureSizeMin = 0.001;

    // Color
    this.m_Opacity = 1;
    this.m_PressureOpacityRange = [0, 1];
    this.m_ColorLuminanceMin = 0;
    this.m_ColorSaturationMax = 1;

    // Particle
    this.m_ParticleSpeed = 0;
    this.m_ParticleRate = 0;
    this.m_ParticleInitialRotationRange = 0;
    this.m_RandomizeAlpha = false;

    // QuadBatch
    this.m_SprayRateMultiplier = 0;
    this.m_RotationVariance = 0;
    this.m_PositionVariance = 0;
    this.m_SizeRatio = [1, 1];

    // Geometry Brush
    this.m_M11Compatibility = false;

    // Tube
    this.m_SolidMinLengthMeters_PS = 0.002;
    this.m_TubeStoreRadiusInTexcoord0Z = false;

    // Misc
    this.m_RenderBackfaces = false;
    this.m_BackIsInvisible = false;
    this.m_BackfaceHueShift = 0;
    this.m_BoundsPadding = 0;
  }

  get Description() {
    // LocalizedString stub: use durable name if no localized string
    return this.m_LocalizedDescription || this.m_DurableName;
  }

  get Material() {
    return this.m_Material;
  }

  PressureSizeMin(previewMode) {
    return previewMode ? this.m_PreviewPressureSizeMin : this.m_PressureSizeRange[0];
  }

  toString() {
    return `BrushDescriptor<${this.m_DurableName} ${this.m_Guid}>`;
  }
}
