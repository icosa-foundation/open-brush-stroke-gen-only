using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEngine;

namespace TiltBrush
{
    public class MinimalExampleCore
    {
        private BrushDescriptor m_DefaultBrush;
        private Pointer m_Pointer;
        private Canvas m_Canvas;

        public void Initialize(GameObject owner, TiltBrushManifest standard, TiltBrushManifest experimental,
            BrushDescriptor defaultBrush, PointerScript pointerScript)
        {
            var mergedManifest = Object.Instantiate(standard);
            if (experimental != null)
            {
                mergedManifest.AppendFrom(experimental);
            }
            BrushCatalog.Init(mergedManifest);
            var canvasScript = owner.AddComponent<CanvasScript>();
            m_Canvas = canvasScript.Core;
            pointerScript.Canvas = canvasScript;
            m_Pointer = pointerScript.Core;
            m_DefaultBrush = defaultBrush;
        }

        public void DrawCircle()
        {
            var path = new List<TrTransform>();
            int segments = 32;
            float radius = 1.5f;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2 * Mathf.PI / segments;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, position);
                path.Add(TrTransform.TRS(position, rotation, 1));
            }

            var color = Color.blue;
            var brush = m_DefaultBrush;
            float smoothing = 0;
            float brushScale = 1f;
            float brushSize = 1f;
            int seed = 0;
            uint group = 0;
            var tr = TrTransform.identity;

            uint time = 0;

            int cpCount = path.Count - 1;
            if (smoothing > 0) cpCount *= 3;
            var controlPoints = new List<ControlPoint>(cpCount);

            for (var vertexIndex = 0; vertexIndex < path.Count - 1; vertexIndex++)
            {
                Vector3 position = path[vertexIndex].translation;
                Quaternion orientation = path[vertexIndex].rotation;
                float pressure = path[vertexIndex].scale;
                Vector3 nextPosition = path[(vertexIndex + 1) % path.Count].translation;

                void addPoint(Vector3 pos)
                {
                    controlPoints.Add(new ControlPoint
                    {
                        m_Pos = pos,
                        m_Orient = orientation,
                        m_Pressure = pressure,
                        m_TimestampMs = time++
                    });
                }

                addPoint(position);
                if (smoothing > 0)
                {
                    addPoint(position);
                    addPoint(position + (nextPosition - position) * smoothing);
                    addPoint(position + (nextPosition - position) * .5f);
                    addPoint(position + (nextPosition - position) * (1 - smoothing));
                }
            }

            var stroke = new Stroke
            {
                m_Type = Stroke.Type.NotCreated,
                m_IntendedCanvas = m_Canvas,
                m_BrushGuid = brush.m_Guid,
                m_BrushScale = brushScale,
                m_BrushSize = brushSize,
                m_Color = color,
                m_Seed = seed,
                m_ControlPoints = controlPoints.ToArray(),
            };
            stroke.m_ControlPointsToDrop = Enumerable.Repeat(false, stroke.m_ControlPoints.Length).ToArray();
            stroke.Recreate(m_Pointer, tr, m_Canvas);
        }
    }
}
