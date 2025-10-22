using UnityEngine;

namespace TiltBrush
{
    public static class AppCore
    {
        public const float METERS_TO_UNITS = 10f;
        public const float UNITS_TO_METERS = 0.1f;

        private static double m_sketchTimeBase = 0;
        public static double GetCurrentSketchTime(double timeSinceLevelLoad)
        {
            return timeSinceLevelLoad - m_sketchTimeBase;
        }
    }
}
