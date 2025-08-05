using System.Collections.Generic;
using System.Linq;
using TiltBrush;
using UnityEngine;

public class MinimalExample : MonoBehaviour
{
    void Start()
    {
        BrushCatalog.m_Instance.Init();
        BrushCatalog.m_Instance.BeginReload();

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
        var brush = BrushCatalog.m_Instance.DefaultBrush;
        float smoothing = 0;
        var canvas = App.ActiveCanvas;
        float brushScale = 1f;
        float brushSize = 1f;
        int seed = 0;
        uint group = 0;
        var tr = TrTransform.identity;

        uint time = 0;
        int pathIndex = 0;

        int cpCount = path.Count - 1;
        if (smoothing > 0) cpCount *= 3; // Three control points per original vertex
        var controlPoints = new List<PointerManager.ControlPoint>(cpCount);

        for (var vertexIndex = 0; vertexIndex < path.Count - 1; vertexIndex++)
        {
            Vector3 position = path[vertexIndex].translation;
            Quaternion orientation = path[vertexIndex].rotation;
            float pressure = path[vertexIndex].scale;
            Vector3 nextPosition = path[(vertexIndex + 1) % path.Count].translation;

            void addPoint(Vector3 pos)
            {
                controlPoints.Add(new PointerManager.ControlPoint
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
                // smoothing controls much to pull extra vertices towards the middle
                // 0.25 smooths corners a lot, 0.1 is tighter
                addPoint(position);
                addPoint(position + (nextPosition - position) * smoothing);
                addPoint(position + (nextPosition - position) * .5f);
                addPoint(position + (nextPosition - position) * (1 - smoothing));
            }
        }

        var stroke = new Stroke
        {
            m_Type = Stroke.Type.NotCreated,
            m_IntendedCanvas = canvas,
            m_BrushGuid = brush.m_Guid,
            m_BrushScale = brushScale,
            m_BrushSize = brushSize,
            m_Color = color,
            m_Seed = seed,
            m_ControlPoints = controlPoints.ToArray(),
        };
        stroke.m_ControlPointsToDrop = Enumerable.Repeat(false, stroke.m_ControlPoints.Length).ToArray();
        stroke.Group = new SketchGroupTag(group);
        stroke.Recreate(tr, canvas);
        if (pathIndex != 0) stroke.m_Flags = SketchMemoryScript.StrokeFlags.IsGroupContinue;
        SketchMemoryScript.m_Instance.MemoryListAdd(stroke);

        // GameObject gameObj = new GameObject("BrushObject");
        // BaseBrushScript brushScript = gameObj.AddComponent<BaseBrushScript>();
        // brushScript.HideBrush(false);
    }

}
