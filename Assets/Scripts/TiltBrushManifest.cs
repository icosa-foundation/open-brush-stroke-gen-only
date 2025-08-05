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

using System.Linq;
using UnityEngine;

namespace TiltBrush
{

    [CreateAssetMenu(fileName = "Manifest", menuName = "Tilt Brush Manifest")]
    public class TiltBrushManifest : ScriptableObject
    {
        public BrushDescriptor[] Brushes;
        public BrushDescriptor[] CompatibilityBrushes;

        // lhs = lhs + rhs, with duplicates removed.
        // The leading portion of the returned array will be == lhs.
        void AppendUnique<T>(ref T[] lhs, T[] rhs) where T : class
        {
            var refEquals = new ReferenceComparer<T>();
            lhs = lhs.Except(rhs, refEquals).Union(rhs, refEquals).ToArray();
        }

        /// Append the contents of rhs to this, eliminating duplicates
        public void AppendFrom(TiltBrushManifest rhs)
        {
            AppendUnique(ref Brushes, rhs.Brushes);
            AppendUnique(ref CompatibilityBrushes, rhs.CompatibilityBrushes);
        }

    } // TiltBrushManifest
} // namespace TiltBrush
