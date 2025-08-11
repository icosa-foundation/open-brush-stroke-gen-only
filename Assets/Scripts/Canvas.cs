using UnityEngine;

namespace TiltBrush
{
    public class Canvas
    {
        private readonly Transform m_Transform;

        public Canvas(Transform transform)
        {
            m_Transform = transform;
        }

        public TrTransform Pose => Coords.AsGlobal[m_Transform];
    }
}
