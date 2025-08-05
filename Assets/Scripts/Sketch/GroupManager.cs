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

namespace TiltBrush
{
    /// A plain old int, with a wrapper type for type-safety
    public struct SketchGroupTag : IEquatable<SketchGroupTag>
    {
        static public SketchGroupTag None = new SketchGroupTag(0);

        // An opaque value
        private readonly UInt32 cookie;

        // Only for use by GroupManager
        public SketchGroupTag(UInt32 cookie)
        {
            this.cookie = cookie;
        }

        public override bool Equals(object obj)
        {
            if (obj is SketchGroupTag)
            {
                return this.Equals((SketchGroupTag)obj);
            }
            return false;
        }

        public bool Equals(SketchGroupTag tag)
        {
            return (this.cookie == tag.cookie);
        }

        public override int GetHashCode()
        {
            return (int)this.cookie;
        }

        public static bool operator ==(SketchGroupTag lhs, SketchGroupTag rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SketchGroupTag lhs, SketchGroupTag rhs)
        {
            return !(lhs.Equals(rhs));
        }

        // C# will "helpfully" allow == and != null comparisons (for subtle and surprising reasons)
        // Make sure these generate compile warnings.

        [Obsolete]
        public static bool operator ==(SketchGroupTag lhs, String rhs)
        {
            throw new InvalidOperationException();
        }

        [Obsolete]
        public static bool operator !=(SketchGroupTag lhs, String rhs)
        {
            throw new InvalidOperationException();
        }
    }

} // namespace TiltBrush
