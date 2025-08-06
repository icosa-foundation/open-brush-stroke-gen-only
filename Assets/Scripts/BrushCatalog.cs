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
using UnityEngine;

namespace TiltBrush
{
    public class BrushCatalog
    {
        public static Texture2D m_GlobalNoiseTexture;
        private static Dictionary<Guid, BrushDescriptor> m_GuidToBrush;
        private static List<BrushDescriptor> m_GuiBrushList;
        private static TiltBrushManifest m_Manifest;

        public static BrushDescriptor GetBrush(Guid guid)
        {
            try
            {
                return m_GuidToBrush[guid];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static void Init(TiltBrushManifest manifest)
        {
            m_Manifest = manifest;
            m_GuidToBrush = new Dictionary<Guid, BrushDescriptor>();
            m_GuiBrushList = new List<BrushDescriptor>();

            Shader.SetGlobalTexture("_GlobalNoiseTexture", m_GlobalNoiseTexture);
            var manifestBrushes = LoadBrushesInManifest();

            m_GuidToBrush.Clear();

            foreach (var brush in manifestBrushes)
            {
                BrushDescriptor tmp;
                if (m_GuidToBrush.TryGetValue(brush.m_Guid, out tmp) && tmp != brush)
                {
                    Debug.LogErrorFormat("Guid collision: {0}, {1}", tmp, brush);
                    continue;
                }
                m_GuidToBrush[brush.m_Guid] = brush;
            }

            // Add reverse links to the brushes
            // Auto-add brushes as compat brushes
            foreach (var brush in manifestBrushes) { brush.m_SupersededBy = null; }
            foreach (var brush in manifestBrushes)
            {
                var older = brush.m_Supersedes;
                if (older == null) { continue; }
                // Add as compat
                if (!m_GuidToBrush.ContainsKey(older.m_Guid))
                {
                    m_GuidToBrush[older.m_Guid] = older;
                    older.m_HiddenInGui = true;
                }
                // Set reverse link
                if (older.m_SupersededBy != null)
                {
                    // No need to warn if the superseding brush is the same
                    if (older.m_SupersededBy.name != brush.name)
                    {
                        Debug.LogWarningFormat(
                            "Unexpected: {0} is superseded by both {1} and {2}",
                            older.name, older.m_SupersededBy.name, brush.name);
                    }
                }
                else
                {
                    older.m_SupersededBy = brush;
                }
            }

            // Postprocess: put brushes into parse-friendly list
            m_GuiBrushList.Clear();
            foreach (var brush in m_GuidToBrush.Values)
            {
                // Some brushes are hardcoded as hidden
                if (brush.m_HiddenInGui) continue;
                m_GuiBrushList.Add(brush);
            }
        }

        // Returns brushes in both sections of the manifest (compat and non-compat)
        // Brushes that are found only in the compat section will have m_HiddenInGui = true
        private static List<BrushDescriptor> LoadBrushesInManifest()
        {
            List<BrushDescriptor> output = new List<BrushDescriptor>();
            foreach (var desc in m_Manifest.Brushes)
            {
                if (desc != null)
                {
                    output.Add(desc);
                }
            }

            // Additional hidden brushes
            var hidden = m_Manifest.CompatibilityBrushes.Except(m_Manifest.Brushes);
            foreach (var desc in hidden)
            {
                if (desc != null)
                {
                    desc.m_HiddenInGui = true;
                    output.Add(desc);
                }
            }
            return output;
        }
    }
} // namespace TiltBrush
