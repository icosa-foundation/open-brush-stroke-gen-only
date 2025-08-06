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
    public class App : MonoBehaviour
    {
        public const float METERS_TO_UNITS = 10f;
        public const float UNITS_TO_METERS = .1f;

        /// Time origin of sketch in seconds for case when drawing is not sync'd to media.
        private static double m_sketchTimeBase = 0;
        public static double CurrentSketchTime =>
            // Unity's Time.time has useful precision probably <= 1ms, and unknown
            // drift/accuracy. It is a single (but is a double, internally), so its
            // raw precision drops to ~2ms after ~4 hours and so on.
            // Time.timeSinceLevelLoad is also an option.
            //
            // C#'s DateTime API has low-ish precision (10+ ms depending on OS)
            // but likely the highest accuracy with respect to wallclock, since
            // it's reading from an RTC.
            //
            // High-precision timers are the opposite: high precision, but are
            // subject to drift.
            //
            // For realtime sync, Time.time is probably the best thing to use.
            // For postproduction sync, probably C# DateTime.
            // If you change this, also modify SketchTimeToLevelLoadTime
            Time.timeSinceLevelLoad - m_sketchTimeBase;
    }
}