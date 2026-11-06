using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gambar kotak dengan sudut kiri-atas dan kanan-bawah terpotong (gaya HUD sci-fi).
/// Tidak butuh sprite. Taruh di Assets/Scripts/ChamferGraphic.cs
/// </summary>
[AddComponentMenu("UI/Chamfer Graphic")]
public class ChamferGraphic : MaskableGraphic
{
    public float cutTopLeft = 16f;
    public float cutBottomRight = 16f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = GetPixelAdjustedRect();
        float a = Mathf.Min(cutTopLeft, r.height, r.width);
        float b = Mathf.Min(cutBottomRight, r.height, r.width);

        Vector2[] p =
        {
            new Vector2(r.xMin, r.yMin),
            new Vector2(r.xMin, r.yMax - a),
            new Vector2(r.xMin + a, r.yMax),
            new Vector2(r.xMax, r.yMax),
            new Vector2(r.xMax, r.yMin + b),
            new Vector2(r.xMax - b, r.yMin)
        };

        foreach (var v in p)
        {
            var vert = UIVertex.simpleVert;
            vert.position = v;
            vert.color = color;
            vh.AddVert(vert);
        }
        for (int i = 1; i < p.Length - 1; i++) vh.AddTriangle(0, i, i + 1);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
