// Port of Tilt Brush's BrushCatalog to JavaScript.
// Manages registration and lookup of BrushDescriptor instances.
import BrushDescriptor from './BrushDescriptor.js';

export default class BrushCatalog {
  static m_GlobalNoiseTexture = null;
  static m_GuidToBrush = new Map();
  static m_GuiBrushList = [];
  static m_Manifest = null;

  static GetBrush(guid) {
    return this.m_GuidToBrush.get(guid) || null;
  }

  static Init(manifest) {
    this.m_Manifest = manifest;
    this.m_GuidToBrush = new Map();
    this.m_GuiBrushList = [];

    // TODO: Shader.SetGlobalTexture("_GlobalNoiseTexture", m_GlobalNoiseTexture);
    const manifestBrushes = this.LoadBrushesInManifest();

    for (const brush of manifestBrushes) {
      const existing = this.m_GuidToBrush.get(brush.m_Guid);
      if (existing && existing !== brush) {
        console.error(`Guid collision: ${existing} ${brush}`);
        continue;
      }
      this.m_GuidToBrush.set(brush.m_Guid, brush);
    }

    // Add reverse links and auto-add compat brushes
    for (const brush of manifestBrushes) {
      brush.m_SupersededBy = null;
    }
    for (const brush of manifestBrushes) {
      const older = brush.m_Supersedes;
      if (!older) continue;
      if (!this.m_GuidToBrush.has(older.m_Guid)) {
        this.m_GuidToBrush.set(older.m_Guid, older);
        older.m_HiddenInGui = true;
      }
      if (older.m_SupersededBy && older.m_SupersededBy.name !== brush.name) {
        console.warn(
          `Unexpected: ${older.name} is superseded by both ${older.m_SupersededBy.name} and ${brush.name}`
        );
      } else {
        older.m_SupersededBy = brush;
      }
    }

    // Postprocess: put brushes into parse-friendly list
    this.m_GuiBrushList = [];
    for (const brush of this.m_GuidToBrush.values()) {
      if (brush.m_HiddenInGui) continue;
      this.m_GuiBrushList.push(brush);
    }
  }

  static LoadBrushesInManifest() {
    const output = [];
    if (!this.m_Manifest) return output;
    const brushes = this.m_Manifest.Brushes || [];
    for (const desc of brushes) {
      if (desc) output.push(desc);
    }

    const compat = this.m_Manifest.CompatibilityBrushes || [];
    const hidden = compat.filter(desc => !brushes.includes(desc));
    for (const desc of hidden) {
      if (desc) {
        desc.m_HiddenInGui = true;
        output.push(desc);
      }
    }
    return output;
  }
}
