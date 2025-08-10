import BrushDescriptor from './BrushDescriptor.js';

/**
 * Port of Tilt Brush's BrushCatalog to manage brush descriptors and lookups.
 * This minimal version registers descriptors and provides lookup utilities.
 */
export class BrushCatalog {
  constructor() {
    this.descriptors = new Map(); // guid -> BrushDescriptor
    this.nameLookup = new Map(); // durableName/description -> guid
  }

  /**
   * Register a new brush descriptor.
   * @param {BrushDescriptor} desc
   */
  register(desc) {
    this.descriptors.set(desc.guid, desc);
    if (desc.durableName) this.nameLookup.set(desc.durableName, desc.guid);
    if (desc.description) this.nameLookup.set(desc.description, desc.guid);
  }

  /**
   * Retrieve a descriptor by guid.
   * @param {string} guid
   * @returns {BrushDescriptor|null}
   */
  getDescriptor(guid) {
    return this.descriptors.get(guid) || null;
  }

  /**
   * Retrieve a descriptor by name (durableName or description).
   * @param {string} name
   * @returns {BrushDescriptor|null}
   */
  getDescriptorByName(name) {
    const guid = this.nameLookup.get(name);
    return guid ? this.getDescriptor(guid) : null;
  }

  /**
   * Return all registered descriptors.
   * @returns {BrushDescriptor[]}
   */
  getAllDescriptors() {
    return Array.from(this.descriptors.values());
  }

  // Stub: load descriptors from external source (e.g., JSON/Unity assets)
  loadDescriptors(/* source */) {
    // TODO: implement descriptor loading
  }

  // Stub: initialize catalog with default brushes
  static createDefault() {
    const catalog = new BrushCatalog();
    // Default TubeBrush descriptor
    catalog.register(
      new BrushDescriptor({
        guid: 'tube-brush',
        durableName: 'TubeBrush',
        description: 'Tube Brush',
      })
    );
    return catalog;
  }
}

export default BrushCatalog;
