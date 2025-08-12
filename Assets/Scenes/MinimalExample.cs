using TiltBrush;
using UnityEngine;

public class MinimalExample : MonoBehaviour
{
    public TiltBrushManifest m_ManifestStandard;
    public TiltBrushManifest m_ManifestExperimental;
    public BrushDescriptor m_DefaultBrush;
    public PointerScript m_Pointer;

    public MinimalExampleCore Core { get; } = new MinimalExampleCore();

    void Start()
    {
        Core.Initialize(gameObject, m_ManifestStandard, m_ManifestExperimental, m_DefaultBrush, m_Pointer);
    }

    [ContextMenu("Draw Circle")]
    public void DrawCircle()
    {
        Core.DrawCircle();
    }
}
