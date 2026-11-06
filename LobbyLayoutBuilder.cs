#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

public static class LobbyLayoutBuilder
{
    // ==================== WARNA ====================
    static readonly Color BgDark       = new Color(0.02f, 0.03f, 0.06f, 1f);
    static readonly Color PanelDark    = new Color(0.04f, 0.07f, 0.14f, 0.92f);
    static readonly Color Cyan         = new Color(0.25f, 0.75f, 1f, 1f);
    static readonly Color BlueButton   = new Color(0.0f, 0.45f, 0.95f, 1f);
    static readonly Color WhiteSoft    = new Color(0.9f, 0.95f, 1f, 1f);
    static readonly Color GrayText     = new Color(0.65f, 0.75f, 0.9f, 1f);

    [MenuItem("Tools/Lobby/Build Clean Lobby (Valorant Style)")]
    public static void Build()
    {
        var old = GameObject.Find("LobbyCanvas");
        if (old) Undo.DestroyObjectImmediate(old);

        var canvasGO = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build Clean Lobby");

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (!Object.FindObjectOfType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var root = canvasGO.transform;

        // ==================== BACKGROUND ====================
        var bg = Img(root, "Background", BgDark);
        Stretch(bg.rectTransform);

        // Overlay biar lebih dalam
        var overlay = Img(root, "Overlay", new Color(0, 0, 0, 0.25f));
        Stretch(overlay.rectTransform);

        // Tempat hologram globe (nanti diganti sprite)
        var globe = Img(root, "HologramGlobe", new Color(0.1f, 0.4f, 0.8f, 0.15f));
        Place(globe.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(900, 900));

        BuildProfile(root);
        BuildTopRight(root);
        BuildBottomBar(root);

        Selection.activeGameObject = canvasGO;
        Debug.Log("✅ Clean Lobby (style baru) berhasil dibuat!");
    }

    // =====================================================================
    //  PROFILE CARD (KIRI ATAS)
    // =====================================================================
    static void BuildProfile(Transform root)
    {
        var card = Panel(root, "ProfileCard", new Vector2(380, 110), PanelDark);
        Place(card, new Vector2(0, 1), new Vector2(40, -40), new Vector2(380, 110));

        // Avatar
        var avatar = Panel(card, "Avatar", new Vector2(78, 78), new Color(0.08f, 0.15f, 0.3f));
        Place(avatar, new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(78, 78));

        // Level badge
        var lvl = Panel(avatar, "LevelBadge", new Vector2(50, 22), new Color(0.05f, 0.1f, 0.2f));
        Place(lvl, new Vector2(0, 0), new Vector2(4, 4), new Vector2(50, 22));
        Label(lvl, "LvlText", "LVL 87", 12, FontStyles.Bold, Cyan, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50, 22));

        // Nama & Rank
        Label(card, "Name", "CYPHER  |  AGENT 013", 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(110, -14), new Vector2(250, 28));

        Label(card, "Rank", "IMMORTAL 2", 16, FontStyles.Bold, Cyan, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(110, -44), new Vector2(200, 24));

        Label(card, "Peak", "PEAK: RADIANT", 14, FontStyles.Normal, GrayText, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(110, -70), new Vector2(200, 22));
    }

    // =====================================================================
    //  TOP RIGHT
    // =====================================================================
    static void BuildTopRight(Transform root)
    {
        var group = Group(root, "TopRight", true, 18, TextAnchor.MiddleRight);
        Place(group, new Vector2(1, 1), new Vector2(-40, -45), new Vector2(700, 50));

        // Signal
        Label(group, "Signal", "SIGNAL 4 BARS", 15, FontStyles.Normal, GrayText, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130, 30));

        Currency(group, "VP", "VP  1,200", Cyan);
        Currency(group, "Coins", "COINS  15,340", new Color(1f, 0.85f, 0.3f));

        IconBtn(group, "BtnBell");
        IconBtn(group, "BtnFriends");
    }

    static void Currency(Transform parent, string name, string text, Color iconColor)
    {
        var p = Panel(parent, name, new Vector2(150, 40), PanelDark);
        
        Label(p, "Text", text, 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 40));
    }

    static void IconBtn(Transform parent, string name)
    {
        var b = Panel(parent, name, new Vector2(42, 42), PanelDark);
        // Icon bisa diganti nanti
        var icon = Img(b, "Icon", Cyan);
        Place(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(22, 22));
    }

    // =====================================================================
    //  BOTTOM BAR
    // =====================================================================
    static void BuildBottomBar(Transform root)
    {
        var bar = Group(root, "BottomBar", true, 14, TextAnchor.LowerCenter);
        Place(bar, new Vector2(0.5f, 0), new Vector2(0, 45), new Vector2(1100, 70));

        BottomButton(bar, "Shop", "SHOP", false);
        BottomButton(bar, "Settings", "SETTINGS", false);
        BottomButton(bar, "Quit", "QUIT", false);
        BottomButton(bar, "Character", "CHARACTER", false);
        BottomButton(bar, "Inventory", "INVENTORY", false);

        // Tombol START (lebih besar & biru)
        var start = Panel(bar, "StartButton", new Vector2(180, 64), BlueButton);
        Label(start, "Text", "START", 26, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 64));
    }

    static void BottomButton(Transform parent, string name, string title, bool isActive)
    {
        var btn = Panel(parent, name, new Vector2(150, 56), PanelDark);
        
        Label(btn, "Title", title, 18, FontStyles.Bold, isActive ? Color.white : GrayText, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 56));
    }

    // =====================================================================
    //  HELPERS
    // =====================================================================
    static RectTransform Panel(Transform parent, string name, Vector2 size, Color color)
    {
        var rt = Rect(name, parent);
        rt.sizeDelta = size;

        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        // Outline tipis
        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.5f, 0.9f, 0.4f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        return rt;
    }

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

    static RectTransform Group(Transform parent, string name, bool horizontal, float spacing, TextAnchor align)
    {
        var rt = Rect(name, parent);
        var layout = horizontal ? 
            (HorizontalOrVerticalLayoutGroup)rt.gameObject.AddComponent<HorizontalLayoutGroup>() :
            rt.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.spacing = spacing;
        layout.childAlignment = align;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        return rt;
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
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Overflow;

        // Assign font
        if (TMP_Settings.defaultFontAsset != null)
            t.font = TMP_Settings.defaultFontAsset;
        else
        {
            var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null) t.font = font;
        }
    }
}
#endif
