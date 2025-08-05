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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
// using ControllerName = TiltBrush.InputManager.ControllerName;
using Random = UnityEngine.Random;

namespace TiltBrush
{

    //TODO: Separate basic pointer management (e.g. enumeration, global operations)
    //from higher-level symmetry code.
    public partial class PointerManager : MonoBehaviour
    {
        static public PointerManager m_Instance;

        // Modifying this struct has implications for binary compatibility.
        // The layout should match the most commonly-seen layout in the binary file.
        // See SketchMemoryScript.ReadMemory.
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [System.Serializable]
        public struct ControlPoint
        {
            public Vector3 m_Pos;
            public Quaternion m_Orient;

            public float m_Pressure;
            public uint m_TimestampMs; // CurrentSketchTime of creation, in milliseconds
        }
        private bool m_StraightEdgeEnabled; // whether the mode is enabled

        public bool StraightEdgeModeEnabled
        {
            get { return m_StraightEdgeEnabled; }
            set { m_StraightEdgeEnabled = value; }
        }
        // Return a pointer suitable for transient use (like for playback)
        // Guaranteed to be different from any non-null return value of GetPointer(ControllerName)
        // Raise exception if not enough pointers
        public PointerScript GetTransientPointer(int i)
        {
            return App.Instance.m_PointerForNonOpenBrush;
        }
        void Awake()
        {
            m_Instance = this;
        }

        public bool IsMainPointerProcessingLine()
        {
            return false;
        }
    }
} // namespace TiltBrush
