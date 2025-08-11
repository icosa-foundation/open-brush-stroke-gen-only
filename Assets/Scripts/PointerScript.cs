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
        private readonly Pointer m_Pointer = new Pointer();

        public bool DrawingEnabled
        {
            get => m_Pointer.DrawingEnabled;
            set => m_Pointer.DrawingEnabled = value;
        }

        public CanvasScript Canvas
        {
            get => m_Pointer.Canvas;
            set => m_Pointer.Canvas = value;
        }

        public Color m_CurrentColor
        {
            get => m_Pointer.m_CurrentColor;
            set => m_Pointer.m_CurrentColor = value;
        }

        public BrushDescriptor m_CurrentBrush
        {
            get => m_Pointer.m_CurrentBrush;
            set => m_Pointer.m_CurrentBrush = value;
        }

        public float m_CurrentBrushSize
        {
            get => m_Pointer.m_CurrentBrushSize;
            set => m_Pointer.m_CurrentBrushSize = value;
        }

        public float m_CurrentPressure
        {
            get => m_Pointer.m_CurrentPressure;
            set => m_Pointer.m_CurrentPressure = value;
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
        }

        void Update()
        {
            m_Pointer.Tick(transform);
        }

        public void UpdateLineFromObject()
        {
            m_Pointer.UpdateLineFromObject(transform);
        }

        public void UpdateLineFromControlPoint(ControlPoint cp)
        {
            m_Pointer.UpdateLineFromControlPoint(cp);
        }

        public void UpdateLineFromStroke(Stroke stroke)
        {
            m_Pointer.UpdateLineFromStroke(stroke);
        }

        public void UpdateLineVisuals()
        {
            m_Pointer.UpdateLineVisuals();
        }

        public void CreateNewLine(CanvasScript canvas, TrTransform xf_CS, BrushDescriptor overrideDesc = null)
        {
            m_Pointer.CreateNewLine(canvas, xf_CS, overrideDesc);
        }

        public void SetControlPoint(TrTransform lastSpawnXf_LS, bool isKeeper)
        {
            m_Pointer.SetControlPoint(lastSpawnXf_LS, isKeeper);
        }

        public void DetachLine(bool bDiscard)
        {
            m_Pointer.DetachLine(bDiscard);
        }

        public void RecreateLineFromMemory(Stroke stroke)
        {
            m_Pointer.RecreateLineFromMemory(stroke, transform);
        }

        public GameObject BeginLineFromMemory(Stroke stroke, CanvasScript canvas)
        {
            return m_Pointer.BeginLineFromMemory(stroke, canvas, transform);
        }
    }
}
