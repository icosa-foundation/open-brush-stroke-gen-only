// Port of Tilt Brush's BrushManifest ScriptableObject to JavaScript.
// Holds the list of available brushes and compatibility brushes.
export default class BrushManifest {
  constructor() {
    // Array of BrushDescriptor
    this.Brushes = [];
    // Array of BrushDescriptor that are hidden but still loadable
    this.CompatibilityBrushes = [];
  }

  // Create a manifest from a plain JSON object
  static fromJSON(json) {
    const manifest = new BrushManifest();
    if (json) {
      manifest.Brushes = json.Brushes ? Array.from(json.Brushes) : [];
      manifest.CompatibilityBrushes = json.CompatibilityBrushes ? Array.from(json.CompatibilityBrushes) : [];
    }
    return manifest;
  }
}
