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

using UnityEngine;

namespace TiltBrush
{
    public class PointerScript : MonoBehaviour
    {
        // Serialized fields mirrored to the standalone Pointer implementation.
        public bool DrawingEnabled;
        public Color m_CurrentColor;
        public BrushDescriptor m_CurrentBrush;
        public float m_CurrentBrushSize;
        public float m_CurrentPressure;

        private readonly Pointer m_Pointer = new Pointer();

        private CanvasScript m_Canvas;
        public CanvasScript Canvas
        {
            get => m_Canvas;
            set
            {
                m_Canvas = value;
                m_Pointer.Canvas = value;
            }
        }

        public float BrushSize01
        {
            get => m_Pointer.BrushSize01;
            set => m_Pointer.BrushSize01 = value;
        }

        public float BrushSizeAbsolute
        {
            get => m_Pointer.BrushSizeAbsolute;
            set => m_Pointer.BrushSizeAbsolute = value;
        }

        void Awake()
        {
            m_Pointer.Initialize();
            SyncFieldsToPointer();
            SyncFieldsFromPointer();
        }

        void Update()
        {
            SyncFieldsToPointer();
            m_Pointer.Tick(transform);
            SyncFieldsFromPointer();
        }

        void SyncFieldsToPointer()
        {
            m_Pointer.DrawingEnabled = DrawingEnabled;
            m_Pointer.Canvas = m_Canvas;
            m_Pointer.m_CurrentColor = m_CurrentColor;
            m_Pointer.m_CurrentBrush = m_CurrentBrush;
            m_Pointer.m_CurrentBrushSize = m_CurrentBrushSize;
            m_Pointer.m_CurrentPressure = m_CurrentPressure;
        }

        void SyncFieldsFromPointer()
        {
            DrawingEnabled = m_Pointer.DrawingEnabled;
            m_Canvas = m_Pointer.Canvas;
            m_CurrentColor = m_Pointer.m_CurrentColor;
            m_CurrentBrush = m_Pointer.m_CurrentBrush;
            m_CurrentBrushSize = m_Pointer.m_CurrentBrushSize;
            m_CurrentPressure = m_Pointer.m_CurrentPressure;
        }

        public void UpdateLineFromObject()
        {
            m_Pointer.UpdateLineFromObject(transform);
            SyncFieldsFromPointer();
        }

        public void UpdateLineFromControlPoint(ControlPoint cp)
        {
            m_Pointer.UpdateLineFromControlPoint(cp);
            SyncFieldsFromPointer();
        }

        public void UpdateLineFromStroke(Stroke stroke)
        {
            m_Pointer.UpdateLineFromStroke(stroke);
            SyncFieldsFromPointer();
        }

        public void UpdateLineVisuals()
        {
            m_Pointer.UpdateLineVisuals();
            SyncFieldsFromPointer();
        }

        public void CreateNewLine(CanvasScript canvas, TrTransform xf_CS, BrushDescriptor overrideDesc = null)
        {
            m_Pointer.CreateNewLine(canvas, xf_CS, overrideDesc);
            SyncFieldsFromPointer();
        }

        public void SetControlPoint(TrTransform lastSpawnXf_LS, bool isKeeper)
        {
            m_Pointer.SetControlPoint(lastSpawnXf_LS, isKeeper);
            SyncFieldsFromPointer();
        }

        public void DetachLine(bool bDiscard)
        {
            m_Pointer.DetachLine(bDiscard);
            SyncFieldsFromPointer();
        }

        public void RecreateLineFromMemory(Stroke stroke)
        {
            m_Pointer.RecreateLineFromMemory(stroke, transform);
            SyncFieldsFromPointer();
        }

        public GameObject BeginLineFromMemory(Stroke stroke, CanvasScript canvas)
        {
            var go = m_Pointer.BeginLineFromMemory(stroke, canvas, transform);
            SyncFieldsFromPointer();
            return go;
        }
    }
}

