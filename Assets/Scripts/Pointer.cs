// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TiltBrush
{

    public class Pointer
    {
        // ---- public types

        public bool DrawingEnabled;
        private bool m_WasDrawingEnabled;
        private Canvas m_Canvas;
        public Canvas Canvas
        {
            get => m_Canvas;
            set => m_Canvas = value;
        }

        // ---- Private inspector data

        // ---- Private member data

        public Color m_CurrentColor;
        public BrushDescriptor m_CurrentBrush;
        public float m_CurrentBrushSize; // In pointer aka room space
        private Vector2 m_BrushSizeRange;
        public float m_CurrentPressure; // TODO: remove and query line instead?
        private BaseBrushScript m_CurrentLine;
        private List<ControlPoint> m_ControlPoints;
        private bool m_LastControlPointIsKeeper;

        // ---- Public properties, accessors, events

        float _FromRadius(float x)
        {
            return Mathf.Sqrt(x);
        }
        float _ToRadius(float x)
        {
            return x * x;
        }

        /// The brush size, using "normalized" values in the range [0,1].
        /// On get, values are raw and may be outside [0,1].
        /// On set, values outside of the range [0,1] are clamped.
        public float BrushSize01
        {
            get
            {
                float min = _FromRadius(m_BrushSizeRange.x);
                float max = _FromRadius(m_BrushSizeRange.y);
                return Mathf.InverseLerp(min, max, _FromRadius(BrushSizeAbsolute));
            }
            set
            {
                float min = _FromRadius(m_BrushSizeRange.x);
                float max = _FromRadius(m_BrushSizeRange.y);
                BrushSizeAbsolute = _ToRadius(Mathf.Lerp(min, max, Mathf.Clamp01(value)));
            }
        }

        /// The brush size, in absolute room space units.
        /// On get, values are raw and may be outside the brush's desired range.
        /// On set, values outside the brush's nominal range are clamped.
        public float BrushSizeAbsolute
        {
            get => m_CurrentBrushSize;
            set => _SetBrushSizeAbsolute(Mathf.Clamp(value, m_BrushSizeRange.x, m_BrushSizeRange.y));
        }

        // ---- Unity events

        public void Initialize()
        {
            m_ControlPoints = new List<ControlPoint>();
            m_CurrentBrushSize = 1.0f;
            m_BrushSizeRange.x = 1.0f;
            m_BrushSizeRange.y = 2.0f;
            m_CurrentPressure = 1.0f;
        }

        public void Tick(Transform pointerTransform)
        {
            if (DrawingEnabled && !m_WasDrawingEnabled)
            {
                // Drawing just got enabled, so we need to create a new line.
                // This is a no-op if the current line is already set.
                CreateNewLine(m_Canvas, TrTransform.FromLocalTransform(pointerTransform));
            }
            else if (DrawingEnabled && m_WasDrawingEnabled)
            {
                UpdateLineFromObject(pointerTransform);
            }
            else if (!DrawingEnabled && m_WasDrawingEnabled)
            {
                DetachLine(false);
            }
            m_WasDrawingEnabled = DrawingEnabled;
        }

        /// Returns xf_RS, relative to the passed line transform.
        /// Applies m_LineDepth and ignores xf_RS.scale
        /// TODO: see above.
        TrTransform GetTransformForLine(Transform line, TrTransform xf_RS)
        {
            var xfRoomFromLine = Coords.AsRoom[line];
            xf_RS.translation += xf_RS.forward;
            xf_RS.scale = 1;
            return TrTransform.InvMul(xfRoomFromLine, xf_RS);
        }

        /// Non-playback case:
        /// - Update the stroke based on the object's position.
        /// - Save off control points
        /// - Play audio.
        public void UpdateLineFromObject(Transform pointerTransform)
        {
            if (m_CurrentLine == null) return;
            var xf_LS = GetTransformForLine(m_CurrentLine.transform, Coords.AsRoom[pointerTransform]);

            bool bQuadCreated = m_CurrentLine.UpdatePosition_LS(xf_LS, m_CurrentPressure);

            // TODO: let brush take care of storing control points, not us
            SetControlPoint(xf_LS, isKeeper: bQuadCreated);
            UpdateLineVisuals();
        }

        /// Playback case:
        /// - Update stroke based on the passed transform (in local coordinates)
        /// - Do _not_ apply any normal adjustment; it's baked into the control point
        /// - Do not update the mesh
        /// TODO: replace with a bulk-ControlPoint API
        public void UpdateLineFromControlPoint(ControlPoint cp)
        {
            float scale = m_CurrentLine.StrokeScale;
            m_CurrentLine.UpdatePosition_LS(
                TrTransform.TRS(cp.m_Pos, cp.m_Orient, scale), cp.m_Pressure);
        }

        /// Bulk control point addition
        public void UpdateLineFromStroke(Stroke stroke)
        {
            float scale = m_CurrentLine.StrokeScale;
            foreach (var cp in stroke.m_ControlPoints.Where((x, i) => !stroke.m_ControlPointsToDrop[i]))
            {
                m_CurrentLine.UpdatePosition_LS(TrTransform.TRS(cp.m_Pos, cp.m_Orient, scale), cp.m_Pressure);
            }
        }

        public void UpdateLineVisuals()
        {
            m_CurrentLine.ApplyChangesToVisuals();
        }

        void _SetBrushSizeAbsolute(float value)
        {
            m_CurrentBrushSize = value;
        }

        /// Pass a Canvas parent, and a transform in that canvas's space.
        /// If overrideDesc passed, use that for the visuals -- m_CurrentBrush does not change.
        public void CreateNewLine(Canvas canvas, TrTransform xf_CS, BrushDescriptor overrideDesc = null)
        {
            // If straightedge is enabled, we may have a minimum size requirement.
            // Initialize parametric stroke creator for our type of straightedge.
            // Maybe change the brush to a proxy brush.
            BrushDescriptor desc = overrideDesc != null ? overrideDesc : m_CurrentBrush;

            GameObject line = Object.Instantiate(desc.m_BrushPrefab);
            line.transform.SetParent(canvas.Transform);
            Coords.AsLocal[line.transform] = TrTransform.identity;
            line.name = desc.Description;

            m_CurrentLine = line.GetComponent<BaseBrushScript>();
            m_CurrentLine.SetCreationState(m_CurrentColor, m_CurrentBrushSize);
            m_CurrentLine.InitializeCore(desc, xf_CS);
        }

        /// Like BeginLineFromMemory + EndLineFromMemory
        /// To help catch bugs in higher-level stroke code, it is considered
        /// an error unless the stroke is in state NotCreated.
        public void RecreateLineFromMemory(Stroke stroke, Transform pointerTransform)
        {
            if (stroke.m_Type != Stroke.Type.NotCreated)
            {
                throw new InvalidOperationException();
            }
            if (BeginLineFromMemory(stroke, stroke.Canvas, pointerTransform) == null)
            {
                // Unclear why it would have failed, but okay.
                // I guess we keep the old version?
                Debug.LogError("Unexpected error recreating line");
                return;
            }

            UpdateLineFromStroke(stroke);

            // It's kind of warty that this needs to happen; brushes should probably track
            // the mesh-dirty state and flush it in Finalize().
            // TODO: Check if this is still necessary now that QuadStripBrushStretchUV
            // flushes pending geometry changes in Finalize*Brush()
            m_CurrentLine.ApplyChangesToVisuals();

            // Copy in new contents
            {
                m_CurrentLine.FinalizeSolitaryBrush();

                stroke.m_Type = Stroke.Type.BrushStroke;
                stroke.m_IntendedCanvas = null;
                stroke.m_Object = m_CurrentLine.gameObject;
                stroke.m_Object.GetComponent<BaseBrushScript>().Stroke = stroke;
            }

            m_CurrentLine = null;
        }

        public GameObject BeginLineFromMemory(Stroke stroke, Canvas canvas, Transform pointerTransform)
        {
            BrushDescriptor rBrush = BrushCatalog.GetBrush(stroke.m_BrushGuid);
            if (rBrush == null)
            {
                // Ignore stroke
                return null;
            }

            var cp0 = stroke.m_ControlPoints[0];
            var xf_CS = TrTransform.TRS(cp0.m_Pos, cp0.m_Orient, stroke.m_BrushScale);
            var xf_RS = canvas.Pose * xf_CS;

            // This transform used to be incorrect, but we didn't notice.
            // That implies this isn't necessary?
            pointerTransform.position = xf_RS.translation;
            pointerTransform.rotation = xf_RS.rotation;

            m_CurrentBrush = rBrush;
            m_CurrentBrushSize = stroke.m_BrushSize;
            m_CurrentColor = stroke.m_Color;
            CreateNewLine(canvas, xf_CS);
            m_CurrentLine.SetIsLoading();
            m_CurrentLine.RandomSeed = stroke.m_Seed;

            return m_CurrentLine.gameObject;
        }

        /// Record the tranform as a control point.
        /// If the most-recent point is a keeper, append a new control point.
        /// Otherwise, the most-recent point is a keeper, and will be overwritten.
        ///
        /// The parameter "keep" specifies whether the newly-written point is a keeper.
        ///
        /// The current pointer is /not/ queried to get the transform of the new
        /// control point. Instead, caller is responsible for passing in the same
        /// xf that was passed to line.UpdatePosition_LS()
        public void SetControlPoint(TrTransform lastSpawnXf_LS, bool isKeeper)
        {
            ControlPoint rControlPoint;
            rControlPoint.m_Pos = lastSpawnXf_LS.translation;
            rControlPoint.m_Orient = lastSpawnXf_LS.rotation;
            rControlPoint.m_Pressure = m_CurrentPressure;
            rControlPoint.m_TimestampMs = (uint)(App.CurrentSketchTime * 1000);

            if (m_ControlPoints.Count == 0 || m_LastControlPointIsKeeper)
            {
                m_ControlPoints.Add(rControlPoint);
            }
            else
            {
                m_ControlPoints[m_ControlPoints.Count - 1] = rControlPoint;
            }

            m_LastControlPointIsKeeper = isKeeper;
        }


        // During playback, rMemoryObjectForPlayback is non-null, and strokeFlags should not be passed.
        // otherwise, rMemoryObjectForPlayback is null, and strokeFlags should be valid.
        // When non-null, rMemoryObjectForPlayback corresponds to the current line.
        public void DetachLine(bool bDiscard)
        {
            if (bDiscard)
            {
                m_CurrentLine.DestroyMesh();
                Object.Destroy(m_CurrentLine.gameObject);
            }
            else
            {
                //copy master brush over to current line
                m_CurrentLine.FinalizeSolitaryBrush();
            }
            m_CurrentLine = null;
        }
    }
} // namespace TiltBrush
