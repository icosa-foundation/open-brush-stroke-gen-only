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
    [System.Serializable]
    public class Stroke : StrokeData
    {
        public enum Type
        {
            /// Brush stroke has not been realized into geometry (or whatever else it turns into)
            /// so we don't yet know whether it is batched or unbatched
            NotCreated,
            /// Brush stroke geometry exists in the form of a GameObject
            BrushStroke,
            /// Brush stroke geoemtry exists in the form of a BatchSubset
            BatchedBrushStroke,
        }

        // Instance API

        /// How the geometry is contained (if there is any)
        public Type m_Type = Type.NotCreated;
        /// Valid only when type == NotCreated. May be null.
        public CanvasScript m_IntendedCanvas;
        /// Valid only when type == BrushStroke. Never null; will always have a BaseBrushScript.
        public GameObject m_Object;

        /// A copy of the StrokeData part of the stroke.
        /// Used for the saving thread to serialize the sketch.
        private StrokeData m_CopyForSaveThread;

        /// Which control points on the stroke should be dropped due to simplification
        public bool[] m_ControlPointsToDrop;

        /// The canvas this stroke is a part of.
        public CanvasScript Canvas
        {
            get
            {
                if (m_Type == Type.NotCreated)
                {
                    return m_IntendedCanvas;
                }
                else if (m_Type == Type.BrushStroke)
                {
                    // Null checking is needed because sketches that fail to load
                    // can create invalid strokes that with no script.
                    return m_Object?.GetComponent<BaseBrushScript>()?.Canvas;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public Stroke()
        {
            m_Guid = Guid.NewGuid();
        }

        /// Clones the passed stroke into a new NotCreated stroke.
        ///
        /// Group affiliation is copied, implying that the resulting stroke:
        /// - must be put into the same sketch as 'existing'
        /// - is selectable, meaning the caller is responsible for getting it out of NotCreated
        /// The caller is responsible for setting result.Group = SketchGroupTag.None if
        /// those things aren't both true.
        ///
        /// TODO: semantics are cleaner & safer if group affiliation is not copied;
        /// caller can do it explicitly if desired.
        public Stroke(Stroke existing) : base(existing)
        {
            m_ControlPointsToDrop = new bool[existing.m_ControlPointsToDrop.Length];
            Array.Copy(existing.m_ControlPointsToDrop, m_ControlPointsToDrop,
                existing.m_ControlPointsToDrop.Length);

            if (existing.m_Guid != null)
                m_Guid = Guid.NewGuid();
        }

        public void InvalidateCopy()
        {
            m_CopyForSaveThread = null;
        }

        /// Sets type to NotCreated, releasing render resources if applicable.
        /// This means the subset will be destroyed!
        public void Uncreate()
        {
            // Save off before we lose the object/batch that tells us what canvas we're in
            m_IntendedCanvas = Canvas;

            if (m_Object != null)
            {
                Object.Destroy(m_Object);
                m_Object = null;
            }
            m_Type = Type.NotCreated;
        }

        /// Ensure there is geometry for this stroke, creating if necessary.
        /// Optionally also calls SetParent() or LeftTransformControlPoints() before creation.
        ///
        /// Assumes that any existing geometry is up-to-date with the data in the stroke;
        /// this assumption may be used for optimizations. Caller may therefore wish to
        /// call Uncreate() before calling Recreate().
        ///
        /// TODO: name is misleading because geo may be reused instead of recreated
        ///
        /// TODO: Consider moving the code from the "m_Type == StrokeType.BrushStroke"
        /// case of SetParentKeepWorldPosition() into here.
        public void Recreate(TrTransform? leftTransform = null, CanvasScript canvas = null, bool absoluteScale = false)
        {
            // TODO: Try a fast-path that uses VertexLayout+GeometryPool to modify geo directly
            if (leftTransform != null || m_Type == Type.NotCreated)
            {
                // Uncreate first, or SetParent() will do a lot of needless work
                Uncreate();
                if (canvas != null)
                {
                    SetParent(canvas);
                }
                if (leftTransform != null)
                {
                    LeftTransformControlPoints(leftTransform.Value, absoluteScale);
                }

                // PointerManager's pointer management is a complete mess.
                // "5" is the most-likely to be unused. It's terrible that this
                // needs to go through a pointer.
                var pointer = App.Instance.m_PointerForNonOpenBrush;
                pointer.RecreateLineFromMemory(this);
            }
            else if (canvas != null)
            {
                SetParent(canvas);
            }
            else
            {
                // It's already created, not being moved, not being reparented -- the only
                // reason the caller might have done this is they expected the geo to be destroyed
                // and recreated. They're not going to get that, so treat this as a logic error.
                // I expect this case will go away when the name/params get fixed to something
                // more reasonable
                throw new InvalidOperationException("Nothing to do");
            }
        }

        // TODO: Possibly could optimize this in C++ for 11.5% of time in selection.
        private void LeftTransformControlPoints(TrTransform leftTransform, bool absoluteScale = false)
        {
            for (int i = 0; i < m_ControlPoints.Length; i++)
            {
                var point = m_ControlPoints[i];
                var xfOld = TrTransform.TR(point.m_Pos, point.m_Orient);
                var xfNew = leftTransform * xfOld;
                point.m_Pos = xfNew.translation;
                point.m_Orient = xfNew.rotation;
                m_ControlPoints[i] = point;
            }

            m_BrushScale *= absoluteScale
                ? Mathf.Abs(leftTransform.scale)
                : leftTransform.scale;
            InvalidateCopy();
        }

        private void LeftTransformControlPoints(Matrix4x4 leftTransform)
        {
            for (int i = 0; i < m_ControlPoints.Length; i++)
            {
                var point = m_ControlPoints[i];
                point.m_Pos = leftTransform.MultiplyPoint3x4(point.m_Pos);
                point.m_Orient = leftTransform.rotation * point.m_Orient;
                m_ControlPoints[i] = point;
            }

            m_BrushScale *= Mathf.Abs(leftTransform.lossyScale.x);
            InvalidateCopy();
        }

        /// Set the parent canvas of this stroke, preserving the _canvas_-relative position.
        /// There will be a pop if the previous and current canvases have different
        /// transforms.
        ///
        /// Directly analagous to Transform.SetParent, except strokes may not
        /// be parented to an arbitrary Transform, only to CanvasScript.
        public void SetParent(CanvasScript canvas)
        {
            CanvasScript prevCanvas = Canvas;
            if (prevCanvas == canvas)
            {
                return;
            }

            switch (m_Type)
            {
                case Type.BrushStroke:
                    {
                        m_Object.transform.SetParent(canvas.transform, false);
                        break;
                    }
                case Type.NotCreated:
                    {
                        m_IntendedCanvas = canvas;
                        break;
                    }
            }
        }

        /// Set the parent canvas of this stroke, preserving the scene-relative position.
        ///
        /// Slower than the other SetParent(), since the stroke might be recreated from scratch or
        /// transformed.  There will *not* be a pop if the brush's geometry generation is not
        /// transform-invariant.  So production brushes need to be checked to be transform-invariant
        /// some other way.
        ///
        /// Directly analagous to Transform.SetParent, except strokes may not
        /// be parented to an arbitrary Transform, only to CanvasScript.
        public void SetParentKeepWorldPosition(CanvasScript canvas, TrTransform? leftTransform = null)
        {
            CanvasScript prevCanvas = Canvas;
            if (prevCanvas == canvas)
            {
                return;
            }

            // Invariant is:
            //   newCanvas.Pose * newCP = prevCanvas.Pose * prevCP
            // Solve for newCp:
            //   newCP = (newCanvas.Pose.inverse * prevCanvas.Pose) * prevCP
            TrTransform leftTransformValue = leftTransform ?? canvas.Pose.inverse * prevCanvas.Pose;
            bool bWasTransformed = leftTransform.HasValue &&
                !TrTransform.Approximately(App.ActiveCanvas.Pose, leftTransform.Value);
            if (m_Type == Type.NotCreated || !bWasTransformed)
            {
                SetParent(canvas);
                LeftTransformControlPoints(leftTransformValue);
            }
            else
            {
                if (m_Type == Type.BrushStroke)
                {
                    Object.Destroy(m_Object);
                    m_Object = null;

                    m_Type = Type.NotCreated;
                    m_IntendedCanvas = canvas;

                    LeftTransformControlPoints(leftTransform.Value);
                    // PointerManager's pointer management is a complete mess.
                    // "5" is the most-likely to be unused. It's terrible that this
                    // needs to go through a pointer.
                    var pointer = App.Instance.m_PointerForNonOpenBrush;
                    pointer.RecreateLineFromMemory(this);
                }
            }
        }

        public void Hide(bool hide)
        {
            switch (m_Type)
            {
                case Type.BrushStroke:
                    BaseBrushScript rBrushScript =
                        m_Object.GetComponent<BaseBrushScript>();
                    if (rBrushScript)
                    {
                        rBrushScript.HideBrush(hide);
                    }
                    break;
                case Type.NotCreated:
                    Debug.LogError("Unexpected: NotCreated stroke");
                    break;
            }
        }
    }
} // namespace TiltBrush
