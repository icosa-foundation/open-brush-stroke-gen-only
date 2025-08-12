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
        public CanvasScript Canvas;

        public Pointer Core { get; } = new Pointer();

        void Awake()
        {
            Core.Initialize();
            SyncToCore();
            SyncFromCore();
        }

        void Update()
        {
            SyncToCore();
            Core.Tick(transform);
            SyncFromCore();
        }

        void SyncToCore()
        {
            Core.DrawingEnabled = DrawingEnabled;
            Core.Canvas = Canvas;
            Core.m_CurrentColor = m_CurrentColor;
            Core.m_CurrentBrush = m_CurrentBrush;
            Core.m_CurrentBrushSize = m_CurrentBrushSize;
            Core.m_CurrentPressure = m_CurrentPressure;
        }

        void SyncFromCore()
        {
            DrawingEnabled = Core.DrawingEnabled;
            Canvas = Core.Canvas;
            m_CurrentColor = Core.m_CurrentColor;
            m_CurrentBrush = Core.m_CurrentBrush;
            m_CurrentBrushSize = Core.m_CurrentBrushSize;
            m_CurrentPressure = Core.m_CurrentPressure;
        }
    }
}

