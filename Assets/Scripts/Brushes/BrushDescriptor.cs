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
using System.IO;
using UnityEngine;
using UnityEngine.Localization;

namespace TiltBrush
{

    /// Authored data shared by all brushes.
    public class BrushDescriptor : ScriptableObject
    {
        [Header("Identity")]
        [DisabledProperty]
        public SerializableGuid m_Guid;

        [DisabledProperty]
        [Tooltip("A human readable name that cannot change, but is not guaranteed to be unique.")]
        public string m_DurableName;

        // TODO: change this to m_FirstReleasedVersion
        [DisabledProperty]
        public string m_CreationVersion;

        [DisabledProperty]
        [Tooltip("Set to the current version of Tilt Brush when making non-compatible changes")]
        public string m_ShaderVersion = "10.0";

        [DisabledProperty]
        public GameObject m_BrushPrefab;

        [Tooltip("A category that can be used to determine whether a brush will be included in the brush panel")]
        public List<string> m_Tags = new List<string> { "default" };

        [Tooltip("Set to true if brush should not be checked for save/load determinism")]
        public bool m_Nondeterministic;

        [Tooltip("When upgrading a brush, populate this field with the prior version")]
        public BrushDescriptor m_Supersedes;
        // The reverse link to m_Supersedes; filled in on startup.
        [NonSerialized]
        public BrushDescriptor m_SupersededBy;

        [Tooltip("True if this brush looks identical to the version it supersedes. Causes brush to be silently-upgraded on load, and silently-downgraded to the maximally-compatible version on save")]
        public bool m_LooksIdentical = false;

        [Header("GUI")]
        public Texture2D m_ButtonTexture;
        [Tooltip("Name of the brush, in the UI and elsewhere")]
        public LocalizedString m_LocalizedDescription;

        public string Description
        {
            get
            {
                try
                {
                    var locString = m_LocalizedDescription.GetLocalizedStringAsync().Result;
                    return locString;
                }
                catch
                {
                    return m_DurableName;
                }
            }
        }

        // Previously Experimental-Mode only
        [Tooltip("Optional, experimental-only information about the brush")]
        public string m_DescriptionExtra;

        [System.NonSerialized] public bool m_HiddenInGui = false;

        [Header("Material")]
        [SerializeField] private Material m_Material;
        // Number of atlas textures in the V direction
        public int m_TextureAtlasV;
        public float m_TileRate;

        [Header("Size")]
        [DisabledProperty]
        [Vec2AsRange(LowerBound = 0, Slider = false)]
        public Vector2 m_BrushSizeRange;
        [DisabledProperty]
        [Vec2AsRange(LowerBound = 0, Slider = false, HideMax = true)]
        [SerializeField]
        private Vector2 m_PressureSizeRange = new Vector2(.1f, 1f);
        public float m_SizeVariance; // Used by particle and spray brushes.
        [Range(.001f, 1)]
        public float m_PreviewPressureSizeMin = .001f;

        [Header("Color")]
        public float m_Opacity;
        [Vec2AsRange(LowerBound = 0, UpperBound = 1)]
        public Vector2 m_PressureOpacityRange;
        [Range(0, 1)] public float m_ColorLuminanceMin;
        [Range(0, 1)] public float m_ColorSaturationMax;

        [Header("Particle")]
        public float m_ParticleSpeed;
        public float m_ParticleRate;
        public float m_ParticleInitialRotationRange;
        public bool m_RandomizeAlpha;

        // To be removed!
        [Header("QuadBatch")]
        public float m_SprayRateMultiplier;
        public float m_RotationVariance;
        public float m_PositionVariance;
        public Vector2 m_SizeRatio;

        [Header("Geometry Brush")]
        public bool m_M11Compatibility;

        [Header("Tube")]
        // Want to add this to brush description but not obvious how to do it
        //public int m_VertsInClosedCircle = 9;
        // This is defined in pointer space
        public float m_SolidMinLengthMeters_PS = 0.002f;
        // Store radius in z component of uv0
        public bool m_TubeStoreRadiusInTexcoord0Z;

        [Header("Misc")]
        public bool m_RenderBackfaces; // whether we should submit backfaces to renderer
        public bool m_BackIsInvisible; // whether the backside is visible to the user
        public float m_BackfaceHueShift;
        public float m_BoundsPadding; // amount to pad bounding box by in canvas space in meters

        /// Return non-instantiated material
        public Material Material => m_Material;

        public override string ToString()
        {
            return string.Format("BrushDescriptor<{0} {1} {2}>", this.name, Description, m_Guid);
        }

        public float PressureSizeMin(bool previewMode)
        {
            return (previewMode ? m_PreviewPressureSizeMin : m_PressureSizeRange.x);
        }
    }
} // namespace TiltBrush
