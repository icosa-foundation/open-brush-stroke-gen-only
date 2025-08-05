using System.Runtime.InteropServices;
using UnityEngine;

namespace TiltBrush
{
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [System.Serializable]
        public struct ControlPoint
        {
            public Vector3 m_Pos;
            public Quaternion m_Orient;

            public float m_Pressure;
            public uint m_TimestampMs; // CurrentSketchTime of creation, in milliseconds
        }
}