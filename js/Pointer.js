import { StrokeType } from './Stroke.js';
import { TrTransform } from './TrTransform.js';
import BrushCatalog from './BrushCatalog.js';

export class Pointer {
  constructor(canvas) {
    this.canvas = canvas;
    this.currentBrush = null;
    this.currentStroke = null;
  }

  /**
  * Begin a stroke using the provided Stroke data. The stroke should have
  * brushGuid, color, brushSize, and optional brushScale fields populated.
  */
  beginStroke(stroke) {
    const desc = BrushCatalog.GetBrush(stroke.brushGuid);
    if (!desc || !desc.m_BrushPrefab) {
      console.error(`No brush prefab for guid ${stroke.brushGuid}`);
      return;
    }
    this.currentBrush = new desc.m_BrushPrefab();
    if (stroke.color) {
      this.currentBrush.m_Color.copy(stroke.color);
    }
    this.currentBrush.BaseSize_PS = stroke.brushSize || 0.01;
    if (stroke.crossSection) {
      this.currentBrush.crossSection = stroke.crossSection;
    }
    if (stroke.shapeModifier) {
      this.currentBrush.shapeModifier = stroke.shapeModifier;
    }
    if (stroke.surfaceOffset !== undefined) {
      this.currentBrush.surfaceOffset = stroke.surfaceOffset;
    }
    this.currentBrush.initBrush(desc, TrTransform.identity);
    this.currentStroke = stroke;
    this.currentStroke.controlPoints = this.currentStroke.controlPoints || [];
  }

  /** Add a control point to the active stroke. */
  updateStroke(controlPoint) {
    if (!this.currentBrush || !this.currentStroke) return;
    this.currentBrush.addControlPoint(controlPoint);
    this.currentStroke.controlPoints.push(controlPoint);
  }

  /** Finalize the active stroke and add its mesh to the canvas. */
  endStroke() {
    if (!this.currentBrush || !this.currentStroke) return null;

    this.currentBrush.finalizeStroke();
    this.canvas.add(this.currentBrush.group);

    const brushRef = this.currentBrush;
    const stroke = this.currentStroke;
    stroke.object = {
      canvas: this.canvas,
      hideBrush: hide => {
        brushRef.group.visible = !hide;
      },
      setParent: parent => {
        parent.add(brushRef.group);
        this.canvas = parent;
      },
    };
    stroke.type = StrokeType.BrushStroke;

    this.currentBrush = null;
    this.currentStroke = null;
    return stroke;
  }

  /** Recreate a stroke's mesh from stored control points. */
  recreateLineFromMemory(stroke) {
    this.beginStroke(stroke);
    for (const cp of stroke.controlPoints) {
      this.currentBrush.addControlPoint(cp);
    }
    return this.endStroke();
  }
}
