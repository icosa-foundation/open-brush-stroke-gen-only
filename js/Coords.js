import { GlobalAccessor, LocalAccessor, RelativeAccessor } from './TransformExtensions.js';

/**
 * Coordinate system helpers mirroring Tilt Brush's Coords class.
 * AsRoom and AsCanvas are placeholders until full scene infrastructure exists.
 */
export default class Coords {}

Coords.AsRoom = undefined; // TODO: define room-space accessor
Coords.AsGlobal = new GlobalAccessor();
Coords.AsLocal = new LocalAccessor();
Coords.AsCanvas = undefined; // Deprecated; provided by canvas instances
