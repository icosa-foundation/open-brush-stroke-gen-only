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
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TiltBrush
{

    public class PointerScript : MonoBehaviour
    {
        private Color m_CurrentColor;
        private TiltBrush.BrushDescriptor m_CurrentBrush;
        private float m_CurrentBrushSize; // In pointer aka room space
        private Vector2 m_BrushSizeRange;
        private BaseBrushScript m_CurrentLine;

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

        void Awake()
        {
            m_CurrentBrushSize = 1.0f;
            m_BrushSizeRange.x = 1.0f;
            m_BrushSizeRange.y = 2.0f;
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

        void _SetBrushSizeAbsolute(float value)
        {
            m_CurrentBrushSize = value;
        }

        /// Pass a Canvas parent, and a transform in that canvas's space.
        /// If overrideDesc passed, use that for the visuals -- m_CurrentBrush does not change.
        public void CreateNewLine(CanvasScript canvas, TrTransform xf_CS, BrushDescriptor overrideDesc = null)
        {
            // If straightedge is enabled, we may have a minimum size requirement.
            // Initialize parametric stroke creator for our type of straightedge.
            // Maybe change the brush to a proxy brush.
            BrushDescriptor desc = overrideDesc != null ? overrideDesc : m_CurrentBrush;

            m_CurrentLine = BaseBrushScript.Create(
                canvas.transform, xf_CS,
                desc, m_CurrentColor, m_CurrentBrushSize);
        }

        /// Like BeginLineFromMemory + EndLineFromMemory
        /// To help catch bugs in higher-level stroke code, it is considered
        /// an error unless the stroke is in state NotCreated.
        public void RecreateLineFromMemory(Stroke stroke)
        {
            if (stroke.m_Type != Stroke.Type.NotCreated)
            {
                throw new InvalidOperationException();
            }
            if (BeginLineFromMemory(stroke, stroke.Canvas) == null)
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

        public GameObject BeginLineFromMemory(Stroke stroke, CanvasScript canvas)
        {
            BrushDescriptor rBrush = BrushCatalog.m_Instance.GetBrush(stroke.m_BrushGuid);
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
            transform.position = xf_RS.translation;
            transform.rotation = xf_RS.rotation;

            m_CurrentBrush = rBrush;
            m_CurrentBrushSize = stroke.m_BrushSize;
            m_CurrentColor = stroke.m_Color;
            CreateNewLine(canvas, xf_CS);
            m_CurrentLine.SetIsLoading();
            m_CurrentLine.RandomSeed = stroke.m_Seed;

            return m_CurrentLine.gameObject;
        }
    }
} // namespace TiltBrush
