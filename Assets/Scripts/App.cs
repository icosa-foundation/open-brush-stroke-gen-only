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
using UnityEngine;
namespace TiltBrush
{
    public partial class App : MonoBehaviour
    {
        public const float METERS_TO_UNITS = 10f;
        public const float UNITS_TO_METERS = .1f;
        private static App m_Instance;
        public CanvasScript m_Canvas;
        public static CanvasScript ActiveCanvas => Instance.m_Canvas;
        public PointerScript m_PointerForNonOpenBrush;
        public static App Instance => m_Instance;
        [SerializeField] private TiltBrushManifest m_ManifestStandard;
        [SerializeField] private TiltBrushManifest m_ManifestExperimental;
        private TiltBrushManifest m_ManifestFull;

        public TiltBrushManifest ManifestFull
        {
            get
            {
                if (m_ManifestFull == null)
                {
                    m_ManifestFull = MergeManifests();
                }
                return m_ManifestFull;
            }
        }
        /// Time origin of sketch in seconds for case when drawing is not sync'd to media.
        private double m_sketchTimeBase = 0;

        void Awake()
        {
            m_Instance = this;
        }

        private TiltBrushManifest MergeManifests()
        {
            var manifest = Instantiate(m_ManifestStandard);
            if (m_ManifestExperimental != null)
            {
                manifest.AppendFrom(m_ManifestExperimental);
            }
            return manifest;
        }
    }
}