using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Graphic dengan bentuk sudut terpotong (clip path) dan border.
// Dipakai untuk SEMUA panel dan tombol, jadi bentuknya selalu sama.
public class ChamferedGraphic : MaskableGraphic
{
    public float chamfer = 16f;
    public float border = 2f;
    public Color borderColor = new Color(0.45f, 0.85f, 1f, 1f);
    public bool cutTL = true, cutTR = false, cutBR = true, cutBL = false;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = GetPixelAdjustedRect();

        var outer = Outline(r, chamfer);
        var inner = Outline(Shrink(r, border), Mathf.Max(0f, chamfer - border));

        AddFan(vh, outer, borderColor);
        AddFan(vh, inner, color);
    }

    List<Vector2> Outline(Rect r, float c)
    {
        c = Mathf.Min(c, r.width * 0.5f, r.height * 0.5f);
        float xMin = r.xMin, xMax = r.xMax, yMin = r.yMin, yMax = r.yMax;
        var p = new List<Vector2>(8);

        // Searah jarum jam mulai dari kiri atas
        if (cutTL) { p.Add(new Vector2(xMin, yMax - c)); p.Add(new Vector2(xMin + c, yMax)); }
        else p.Add(new Vector2(xMin, yMax));

        if (cutTR) { p.Add(new Vector2(xMax - c, yMax)); p.Add(new Vector2(xMax, yMax - c)); }
        else p.Add(new Vector2(xMax, yMax));

        if (cutBR) { p.Add(new Vector2(xMax, yMin + c)); p.Add(new Vector2(xMax - c, yMin)); }
        else p.Add(new Vector2(xMax, yMin));

        if (cutBL) { p.Add(new Vector2(xMin + c, yMin)); p.Add(new Vector2(xMin, yMin + c)); }
        else p.Add(new Vector2(xMin, yMin));

        return p;
    }

    static Rect Shrink(Rect r, float b)
    {
        return new Rect(r.x + b, r.y + b, r.width - b * 2f, r.height - b * 2f);
    }

    // Polygon cembung, digambar sebagai kipas segitiga dari titik tengah
    static void AddFan(VertexHelper vh, List<Vector2> pts, Color col)
    {
        Vector2 center = Vector2.zero;
        foreach (var p in pts) center += p;
        center /= pts.Count;

        int start = vh.currentVertCount;
        vh.AddVert(center, col, Vector2.zero);
        foreach (var p in pts) vh.AddVert(p, col, Vector2.zero);

        int n = pts.Count;
        for (int i = 1; i <= n; i++)
        {
            int next = (i == n) ? 1 : i + 1;
            vh.AddTriangle(start, start + i, start + next);
        }
    }
}

// Background "cover": mengisi layar tanpa distorsi. ExecuteAlways agar jalan di Editor.
[ExecuteAlways]
public class CoverFit : MonoBehaviour
{
    Sprite sprite;
    RectTransform rt;

    public void Init(Sprite s) { sprite = s; rt = (RectTransform)transform; }

    void LateUpdate()
    {
        if (!sprite || rt == null || rt.parent == null) return;
        var pr = ((RectTransform)rt.parent).rect;
        float ar = sprite.rect.width / sprite.rect.height;
        float pa = pr.width / pr.height;
        rt.sizeDelta = pa > ar ? new Vector2(pr.width, pr.width / ar) : new Vector2(pr.height * ar, pr.height);
    }
}
