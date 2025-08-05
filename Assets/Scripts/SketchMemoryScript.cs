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
using TiltBrush;
using System.Collections;
using System.Threading.Tasks;

namespace TiltBrush
{
    public class SketchMemoryScript : MonoBehaviour
    {
        public static SketchMemoryScript m_Instance;
        [Flags]
        public enum StrokeFlags
        {
            None = 0,
            Deprecated1 = 1 << 0,
            /// This stroke continues a group that is considered a single entity with respect to undo and
            /// redo. To support timestamp ordering, only strokes having identical timestamps on the initial
            /// control point may be grouped (e.g. mirrored strokes). Currently, these strokes must also be
            /// added to SketchMemoryScript at the same time.
            ///
            /// This is distinct from Stroke.Group, which is a collection of strokes (of possibly differing
            /// timestamps) that are selected together.
            IsGroupContinue = 1 << 1,
        }

        // Why we can get away with linked list performance for sequence time ordering:
        //    * load case:  sort once at init to populate sequence-time list.  Since we save in
        //      timestamp order, this should be O(N).
        //    * playback case:  simply walk sequence-time list for normal case.  For timeline scrub,
        //      hopefully skip interval is small (say 10 seconds) so the number of items to traverse
        //      is reasonable.
        //    * edit case:  update current position in sequence-time list every frame (same as playback)
        //      so we're always ready to insert new strokes
        private LinkedList<Stroke> m_MemoryList = new LinkedList<Stroke>();
        // Used as a starting point for any search by time.  Either null or a node contained in
        // m_MemoryList.
        // TODO: Have Update() advance this position to match current sketch time so that we
        // amortize list traversal in timeline edit mode.
        private LinkedListNode<Stroke> m_CurrentNodeByTime;
        //for loading .sketches
        public enum PlaybackMode
        {
            Distance,
            Timestamps,
        }
        private IScenePlayback m_ScenePlayback;


        void Awake()
        {
            m_Instance = this;
        }

        private static bool StrokeTimeLT(Stroke a, Stroke b)
        {
            return (a.HeadTimestampMs < b.HeadTimestampMs);
        }

        private static bool StrokeTimeLTE(Stroke a, Stroke b)
        {
            return !StrokeTimeLT(b, a);
        }

        // The memory list by time is updated-- control points are expected to be initialized
        // and immutable. This includes the timestamps on the control points.
        public void MemoryListAdd(Stroke stroke)
        {
            Debug.Assert(stroke.m_Type == Stroke.Type.NotCreated ||
                stroke.m_Type == Stroke.Type.BrushStroke ||
                stroke.m_Type == Stroke.Type.BatchedBrushStroke);
            if (stroke.m_ControlPoints.Length == 0)
            {
                Debug.LogWarning("Unexpected zero-length stroke");
                return;
            }

            // add to sequence-time list
            // We add to furthest position possible in the list (i.e. following all strokes
            // with lead control point timestamp LTE the new one).  This ensures that grouped
            // strokes are not divided, given that such strokes must have identical timestamps.
            // NOTE: O(1) given expected timestamp order of strokes in the .tilt file.
            var node = stroke.m_NodeByTime;
            if (m_MemoryList.Count == 0 || StrokeTimeLT(stroke, m_MemoryList.First.Value))
            {
                m_MemoryList.AddFirst(node);
            }
            else
            {
                // find insert position-- most efficient for "not too far ahead of current position" case
                var addAfter = m_CurrentNodeByTime;
                if (addAfter == null || StrokeTimeLT(stroke, addAfter.Value))
                {
                    addAfter = m_MemoryList.First;
                }
                while (addAfter.Next != null && StrokeTimeLTE(addAfter.Next.Value, stroke))
                {
                    addAfter = addAfter.Next;
                }
                m_MemoryList.AddAfter(addAfter, node);
            }
            m_CurrentNodeByTime = node;

            // add to scene playback
            if (m_ScenePlayback != null)
            {
                m_ScenePlayback.AddStroke(stroke);
            }
        }
    }
} // namespace TiltBrush
