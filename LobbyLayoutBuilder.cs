#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public static class LobbyLayoutBuilder
{
    // ==================== WARNA SESUAI GAMBAR ====================
    static readonly Color BgDark       = new Color(0.03f, 0.04f, 0.07f, 1f);
    static readonly Color PanelDark    = new Color(0.05f, 0.08f, 0.15f, 0.85f);
    static readonly Color BorderBlue   = new Color(0.2f, 0.5f, 0.8f, 0.6f);
    static readonly Color CyanBright   = new Color(0.3f, 0.75f, 1f, 1f);
    static readonly Color BtnBlue      = new Color(0.15f, 0.45f, 1f, 1f);
    static readonly Color GrayText     = new Color(0.65f, 0.75f, 0.9f, 1f);

    [MenuItem("Tools/Lobby/Build Lobby (Responsive & Exact)")]
    public static void Build()
    {
        var old = GameObject.Find("LobbyCanvas");
        if (old) Undo.DestroyObjectImmediate(old);

        // ==================== CANVAS SETUP ====================
        var canvasGO = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build Lobby");
        
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f; // Menjaga keseimbangan lebar & tinggi di semua rasio

        if (!Object.FindObjectOfType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var root = canvasGO.transform;

        // ==================== BACKGROUND ====================
        // Gambar Background (Stretch penuh agar menutupi layar apapun rasionya)
        var bg = Img(root, "Background", BgDark);
        Stretch(bg.rectTransform); 

        // Overlay Gelap
        var overlay = Img(root, "Overlay", new Color(0, 0, 0, 0.2f));
        Stretch(overlay.rectTransform);

        // ==================== GLOBE (TENGAH) ====================
        // Menggunakan AspectRatioFitter agar globe tetap bulat sempurna di layar lebar
        var globeObj = new GameObject("HologramGlobe", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
        globeObj.transform.SetParent(root, false);
        var globeImg = globeObj.GetComponent<Image>();
        globeImg.color = new Color(0.1f, 0.4f, 0.8f, 0.3f); // Ganti jadi Sprite globe nanti
        globeImg.raycastTarget = false;
        
        var globeRt = globeObj.GetComponent<RectTransform>();
        globeRt.anchorMin = globeRt.anchorMax = new Vector2(0.5f, 0.5f);
        globeRt.anchoredPosition = new Vector2(0, 40);
        globeRt.sizeDelta = new Vector2(800, 800);
        
        var arf = globeObj.GetComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        arf.aspectRatio = 1f; // 1:1 (Bulat)

        // ==================== BUILD UI ELEMENTS ====================
        BuildProfile(root);
        BuildTopRight(root);
        BuildBottomBar(root);

        Selection.activeGameObject = canvasGO;
        Debug.Log("✅ Lobby UI berhasil dibuat dengan sistem Responsive (Anti-Pecah di semua rasio layar).");
    }

    // =====================================================================
    //  PROFIL (KIRI ATAS)
    // =====================================================================
    static void BuildProfile(Transform root)
    {
        // Container Utama dengan Bentuk Chamfer
        var card = ChamferPanel(root, "ProfileCard", new Vector2(420, 120), PanelDark, 15f);
        Place(card, new Vector2(0, 1), new Vector2(40, -40), new Vector2(420, 120));

        // Avatar Box (Kotak dalam)
        var avatarBox = ChamferPanel(card, "AvatarBox", new Vector2(80, 80), new Color(0.1f, 0.15f, 0.25f, 1f), 8f);
        Place(avatarBox, new Vector2(0, 1), new Vector2(15, -15), new Vector2(80, 80));
        
        // Teks Profil
        Label(card, "Name", "CYPHER  |  AGENT 013", 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(115, -20), new Vector2(300, 30));
        
        Label(card, "Rank", "IMMORTAL 2", 16, FontStyles.Bold, CyanBright, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(115, -50), new Vector2(200, 25));
              
        Label(card, "Peak", "PEAK: RADIANT", 14, FontStyles.Bold, GrayText, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(115, -75), new Vector2(200, 20));

        // Badge Level (Kuning)
        var lvlBadge = Img(card, "LevelBadge", new Color(0.9f, 0.8f, 0.2f, 1f));
        Place(lvlBadge.rectTransform, new Vector2(0, 0), new Vector2(15, 10), new Vector2(70, 22));
        Label(card, "LvlText", "LVL 87", 14, FontStyles.Bold, Color.black, TextAlignmentOptions.Center,
              new Vector2(0, 0), new Vector2(15, 10), new Vector2(70, 22));
    }

    // =====================================================================
    //  KANAN ATAS (STATUS & MATA UANG)
    // =====================================================================
    static void BuildTopRight(Transform root)
    {
        // Anchor Kanan Atas
        var group = Rect("TopRightGroup", root);
        Place(group, new Vector2(1, 1), new Vector2(-40, -40), new Vector2(700, 50));

        // Signal
        var sigBox = ChamferPanel(group, "Signal", new Vector2(160, 40), PanelDark, 10f);
        Place(sigBox, new Vector2(1, 1), new Vector2(0, 0), new Vector2(160, 40));
        Label(sigBox, "Text", "SIGNAL 4 BARS", 14, FontStyles.Bold, GrayText, TextAlignmentOptions.Center,
              Vector2.one * 0.5f, Vector2.zero, new Vector2(160, 40));

        // VP Currency
        var vpBox = ChamferPanel(group, "VP", new Vector2(160, 40), PanelDark, 10f);
        Place(vpBox, new Vector2(1, 1), new Vector2(-180, 0), new Vector2(160, 40));
        Label(vpBox, "Text", "VP  1,200", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              Vector2.one * 0.5f, Vector2.zero, new Vector2(160, 40));

        // Coin Currency
        var coinBox = ChamferPanel(group, "Coins", new Vector2(180, 40), PanelDark, 10f);
        Place(coinBox, new Vector2(1, 1), new Vector2(-360, 0), new Vector2(180, 40));
        Label(coinBox, "Text", "COINS  15,340", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              Vector2.one * 0.5f, Vector2.zero, new Vector2(180, 40));

        // Icons (Bell & Profile)
        var bell = ChamferPanel(group, "BtnBell", new Vector2(40, 40), PanelDark, 10f);
        Place(bell, new Vector2(1, 1), new Vector2(-560, 0), new Vector2(40, 40));
        
        var profile = ChamferPanel(group, "BtnProfile", new Vector2(40, 40), PanelDark, 10f);
        Place(profile, new Vector2(1, 1), new Vector2(-610, 0), new Vector2(40, 40));
    }

    // =====================================================================
    //  MENU BAWAH (TOMBOL CHAMFER)
    // =====================================================================
    static void BuildBottomBar(Transform root)
    {
        var bar = Rect("BottomBar", root);
        // Posisi di tengah bawah
        Place(bar, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1200, 70));

        // Tombol-tombol diletakkan secara manual (bukan pakai LayoutGroup) agar jarak & ukuran konsisten
        string[] menuNames = { "SHOP", "SETTINGS", "QUIT", "CHARACTER", "INVENTORY" };
        float btnWidth = 160f;
        float spacing = 20f;
        
        // Hitung total lebar untuk posisi tengah
        float totalWidth = (btnWidth * menuNames.Length) + (spacing * (menuNames.Length - 1));
        float startX = -(totalWidth / 2f) + (btnWidth / 2f);

        for (int i = 0; i < menuNames.Length; i++)
        {
            var btn = ChamferPanel(bar, menuNames[i], new Vector2(btnWidth, 60), PanelDark, 12f);
            Place(btn, new Vector2(0.5f, 0.5f), new Vector2(startX + (i * (btnWidth + spacing)), 0), new Vector2(btnWidth, 60));
            
            Label(btn, "Text", menuNames[i], 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
                  new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(btnWidth, 60));
        }

        // Tombol START (Biru Khusus)
        var startBtn = ChamferPanel(bar, "START", new Vector2(220, 60), BtnBlue, 12f);
        // Diletakkan di sebelah kanan INVENTORY
        Place(startBtn, new Vector2(0.5f, 0.5f), new Vector2(startX + (menuNames.Length * (btnWidth + spacing)), 0), new Vector2(220, 60));
        
        Label(startBtn, "Text", "START", 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 60));
    }

    // =====================================================================
    //  CUSTOM UI COMPONENT: CHAMFER PANEL (SUDUT TUMPUL)
    // =====================================================================
    class UIShapeChamfer : MaskableGraphic
    {
        public float ChamferSize = 15f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            float c = Mathf.Min(ChamferSize, r.width / 2, r.height / 2);

            // Titik-titik (Kiri Bawah -> Kanan Bawah -> Kanan Atas -> Kiri Atas)
            // Kita potong sudut Kiri Atas dan Kanan Bawah (seperti gambar)
            Vector2 p0 = new Vector2(r.xMin, r.yMin); // Kiri Bawah
            Vector2 p1 = new Vector2(r.xMax - c, r.yMin); // Kanan Bawah (sebelum potong)
            Vector2 p2 = new Vector2(r.xMax, r.yMin + c); // Kanan Bawah (sesudah potong)
            Vector2 p3 = new Vector2(r.xMax, r.yMax); // Kanan Atas
            Vector2 p4 = new Vector2(r.xMin + c, r.yMax); // Kiri Atas (sebelum potong)
            Vector2 p5 = new Vector2(r.xMin, r.yMax - c); // Kiri Atas (sesudah potong)

            // Tambah Vertex
            vh.AddVert(p0, color, Vector2.zero);
            vh.AddVert(p1, color, Vector2.zero);
            vh.AddVert(p2, color, Vector2.zero);
            vh.AddVert(p3, color, Vector2.zero);
            vh.AddVert(p4, color, Vector2.zero);
            vh.AddVert(p5, color, Vector2.zero);

            // Gambar Segitiga
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
            vh.AddTriangle(0, 3, 4);
            vh.AddTriangle(0, 4, 5);
        }
    }

    static RectTransform ChamferPanel(Transform parent, string name, Vector2 size, Color color, float chamfer)
    {
        var rt = Rect(name, parent);
        rt.sizeDelta = size;

        var shape = rt.gameObject.AddComponent<UIShapeChamfer>();
        shape.color = color;
        shape.ChamferSize = chamfer;

        // Tambah garis tepi tipis (opsional, agar mirip gambar)
        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = BorderBlue;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = shape;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.8f, 0.9f, 1f);
        btn.colors = colors;

        return rt;
    }

    // =====================================================================
    //  HELPER FUNCTIONS
    // =====================================================================
    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Image Img(Transform parent, string name, Color c)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        return img;
    }

    static void Label(Transform parent, string name, string text, float size, FontStyles style, Color color,
                      TextAlignmentOptions align, Vector2 anchor, Vector2 pos, Vector2 boxSize)
    {
        var rt = Rect(name, parent);
        Place(rt, anchor, pos, boxSize);

        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        
        #pragma warning disable 0618
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Overflow;
        #pragma warning restore 0618
    }
}
#endif
