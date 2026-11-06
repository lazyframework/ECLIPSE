#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

/// <summary>
/// Letakkan di Assets/Editor/ (bersama ChamferGraphic.cs di Assets/Scripts/).
/// Jalankan: Tools > Lobby > Build Main Menu Layout
/// Canvas 1920x1080: profil kiri atas, status kanan atas, garis frame,
/// 6 tombol bawah (SHOP, SETTINGS, QUIT, CHARACTER, INVENTORY, START), footer versi.
/// </summary>
public static class MainMenuLayoutBuilder
{
    // ---------- Palet ----------
    static readonly Color PanelFill   = new Color(0.03f, 0.07f, 0.16f, 0.88f);
    static readonly Color BorderMain  = new Color(0.20f, 0.50f, 1.00f, 1f);
    static readonly Color BorderThin  = new Color(0.12f, 0.27f, 0.62f, 1f);
    static readonly Color BtnFill     = new Color(0.04f, 0.09f, 0.20f, 0.92f);
    static readonly Color BtnBorder   = new Color(0.25f, 0.45f, 0.85f, 1f);
    static readonly Color StartFill   = new Color(0.05f, 0.25f, 0.95f, 1f);
    static readonly Color StartBorder = new Color(0.35f, 0.62f, 1.00f, 1f);
    static readonly Color Cyan        = new Color(0.45f, 0.78f, 1.00f, 1f);
    static readonly Color RankBlue    = new Color(0.20f, 0.50f, 1.00f, 1f);
    static readonly Color Dim         = new Color(0.55f, 0.66f, 0.84f, 1f);
    static readonly Color Line        = new Color(0.20f, 0.45f, 1.00f, 0.45f);

    [MenuItem("Tools/Lobby/Build Main Menu Layout")]
    public static void Build()
    {
        var old = GameObject.Find("MainMenuCanvas");
        if (old) Undo.DestroyObjectImmediate(old);

        var canvasGO = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build Main Menu Layout");
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

#if UNITY_2023_1_OR_NEWER
        bool hasES = Object.FindFirstObjectByType<EventSystem>() != null;
#else
        bool hasES = Object.FindObjectOfType<EventSystem>() != null;
#endif
        if (!hasES)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Build Main Menu Layout");
        }

        var root = canvasGO.transform;

        var bg = Img(root, "Background", new Color(0.01f, 0.02f, 0.06f, 1f));
        Stretch(bg.rectTransform);

        BuildFrameLines(root);
        BuildProfile(root);
        BuildTopRight(root);
        BuildBottomMenu(root);

        Label(root, "VersionFooter", "v5.08  •  REGION: NA  •  22ms", 15, FontStyles.Normal, new Color(0.45f, 0.52f, 0.70f, 1f),
              TextAlignmentOptions.Left, new Vector2(0, 0), new Vector2(46, 20), new Vector2(400, 22));

        Selection.activeGameObject = canvasGO;
        Debug.Log("Main menu layout selesai. Isi sprite di Background (gambar tanpa UI), avatar, dan objek 'Icon'.");
    }

    // =====================================================================
    //  GARIS FRAME (atas & bawah area tengah)
    // =====================================================================
    static void BuildFrameLines(Transform root)
    {
        FrameLine(root, "FrameTop", new Vector2(0.5f, 1), -236f);
        FrameLine(root, "FrameBottom", new Vector2(0.5f, 0), 182f);
    }

    static void FrameLine(Transform root, string name, Vector2 anchor, float y)
    {
        var line = Img(root, name, Line);
        Place(line.rectTransform, anchor, new Vector2(0, y), new Vector2(1780, 1));
        for (int i = 0; i < 2; i++)
        {
            float x = i == 0 ? -890f : 890f;
            var tick = Img(line.transform, "Tick" + i, Line);
            Place(tick.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, 0), new Vector2(1, 26));
        }
    }

    // =====================================================================
    //  PROFIL (kiri atas)
    // =====================================================================
    static void BuildProfile(Transform root)
    {
        var p = Shape(root, "ProfilePanel", new Vector2(420, 136), PanelFill, BorderMain, 0, 34, false, 2f);
        Place(p, new Vector2(0, 1), new Vector2(50, -50), new Vector2(420, 136));

        var avatar = Shape(p, "Avatar", new Vector2(84, 84), new Color(0.04f, 0.10f, 0.22f, 1f), BorderThin, 0, 0, false, 1f);
        Place(avatar, new Vector2(0, 1), new Vector2(14, -14), new Vector2(84, 84));
        var portrait = Img(avatar, "Portrait", Color.white);
        Stretch(portrait.rectTransform, 3);
        portrait.color = new Color(0.1f, 0.2f, 0.4f, 1f);

        var lvl = Shape(p, "LevelTag", new Vector2(56, 20), new Color(0.02f, 0.04f, 0.10f, 1f), BorderThin, 0, 0, false, 1f);
        Place(lvl, new Vector2(0, 0), new Vector2(10, 10), new Vector2(56, 20));
        Label(lvl, "Text", "LVL 87", 12, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56, 20));

        Label(p, "Name", "CYPHER | AGENT 013", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(114, -16), new Vector2(290, 30), 1f);

        var rankIcon = Img(p, "Icon", RankBlue);
        Place(rankIcon.rectTransform, new Vector2(0, 1), new Vector2(114, -54), new Vector2(24, 24));
        Label(p, "Rank", "IMMORTAL 2", 20, FontStyles.Bold, RankBlue, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(146, -53), new Vector2(200, 26), 0.5f);

        Label(p, "Peak", "PEAK: RADIANT", 17, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(114, -88), new Vector2(250, 24));
    }

    // =====================================================================
    //  STATUS KANAN ATAS
    // =====================================================================
    static void BuildTopRight(Transform root)
    {
        var g = Group(root, "TopRight", true, 6, TextAnchor.MiddleRight);
        Place(g, new Vector2(1, 1), new Vector2(-50, -50), new Vector2(700, 48));

        // Signal
        var sig = InfoBox(g, "SignalBox", 190);
        for (int i = 0; i < 4; i++)
        {
            float h = 6f + 4f * i;
            var bar = Img(sig, "Bar" + i, Cyan);
            Place(bar.rectTransform, new Vector2(0, 0.5f), new Vector2(14 + i * 7, -9 + h / 2f), new Vector2(4, h));
        }
        Label(sig, "Text", "SIGNAL 4 BARS", 15, FontStyles.Normal, Dim, TextAlignmentOptions.Left,
              new Vector2(0, 0.5f), new Vector2(50, 0), new Vector2(130, 24));

        // VP
        var vp = InfoBox(g, "VPBox", 180);
        var vpIcon = Img(vp, "Icon", Cyan);
        Place(vpIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(22, 22));
        Label(vp, "Text", "<color=#8FA9D6>VP</color>  <b>1,200</b>", 18, FontStyles.Normal, Color.white,
              TextAlignmentOptions.Left, new Vector2(0, 0.5f), new Vector2(44, 0), new Vector2(130, 26));

        // Coins
        var coin = InfoBox(g, "CoinBox", 210);
        var coinIcon = Img(coin, "Icon", new Color(0.30f, 0.55f, 1f, 1f));
        Place(coinIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(22, 22));
        Label(coin, "Text", "<color=#8FA9D6>COINS</color>  <b>15,340</b>", 18, FontStyles.Normal, Color.white,
              TextAlignmentOptions.Left, new Vector2(0, 0.5f), new Vector2(44, 0), new Vector2(160, 26));

        // Lonceng & profil
        SquareIconButton(g, "NotificationButton");
        SquareIconButton(g, "ProfileButton");
    }

    static RectTransform InfoBox(Transform parent, string name, float width)
    {
        return Shape(parent, name, new Vector2(width, 48), PanelFill, BorderThin, 0, 0, false, 1f);
    }

    static void SquareIconButton(Transform parent, string name)
    {
        var b = Shape(parent, name, new Vector2(48, 48), PanelFill, BorderThin, 0, 0, true, 1f);
        var icon = Img(b, "Icon", Dim);
        Place(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(22, 22));
    }

    // =====================================================================
    //  TOMBOL BAWAH
    // =====================================================================
    static void BuildBottomMenu(Transform root)
    {
        var g = Group(root, "BottomMenu", true, 22, TextAnchor.MiddleCenter);
        Place(g, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(1540, 64));

        MenuButton(g, "ShopButton", "SHOP", false);
        MenuButton(g, "SettingsButton", "SETTINGS", false);
        MenuButton(g, "QuitButton", "QUIT", false);
        MenuButton(g, "CharacterButton", "CHARACTER", false);
        MenuButton(g, "InventoryButton", "INVENTORY", false);
        MenuButton(g, "StartButton", "START", true);
    }

    static void MenuButton(Transform parent, string name, string text, bool primary)
    {
        var b = Shape(parent, name, new Vector2(232, 64),
                      primary ? StartFill : BtnFill, primary ? StartBorder : BtnBorder, 18, 18, true, primary ? 0f : 1.5f);
        Label(b, "Text", text, 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 30), 1f);
    }

    // =====================================================================
    //  HELPER
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

    static void Stretch(RectTransform rt, float inset = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    static Image Img(Transform parent, string name, Color c)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        return img;
    }

    // Bentuk dengan garis tepi (outer = border, inner = fill). Sudut kiri-atas / kanan-bawah bisa dipotong.
    static RectTransform Shape(Transform parent, string name, Vector2 size, Color fill, Color border,
                               float cutTL, float cutBR, bool clickable, float thickness)
    {
        var rt = Rect(name, parent);
        rt.sizeDelta = size;

        var outer = rt.gameObject.AddComponent<ChamferGraphic>();
        outer.color = border;
        outer.cutTopLeft = cutTL;
        outer.cutBottomRight = cutBR;
        outer.raycastTarget = clickable;

        var innerRt = Rect("Fill", rt);
        Stretch(innerRt, thickness);
        var inner = innerRt.gameObject.AddComponent<ChamferGraphic>();
        inner.color = fill;
        inner.cutTopLeft = Mathf.Max(0, cutTL - 0.586f * thickness);
        inner.cutBottomRight = Mathf.Max(0, cutBR - 0.586f * thickness);
        inner.raycastTarget = false;

        if (clickable)
        {
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = outer;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.75f, 0.92f, 1f);
            cb.pressedColor = new Color(0.55f, 0.75f, 1f);
            btn.colors = cb;
        }
        return rt;
    }

    static RectTransform Group(Transform parent, string name, bool horizontal, float spacing, TextAnchor align)
    {
        var rt = Rect(name, parent);
        HorizontalOrVerticalLayoutGroup g = horizontal
            ? (HorizontalOrVerticalLayoutGroup)rt.gameObject.AddComponent<HorizontalLayoutGroup>()
            : rt.gameObject.AddComponent<VerticalLayoutGroup>();
        g.spacing = spacing;
        g.childAlignment = align;
        g.childControlWidth = g.childControlHeight = false;
        g.childForceExpandWidth = g.childForceExpandHeight = false;
        return rt;
    }

    static void Label(Transform parent, string name, string text, float size, FontStyles style, Color color,
                      TextAlignmentOptions align, Vector2 anchor, Vector2 pos, Vector2 boxSize, float spacing = 0f)
    {
        var rt = Rect(name, parent);
        Place(rt, anchor, pos, boxSize);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        t.characterSpacing = spacing;
        t.raycastTarget = false;
    }
}
#endif
