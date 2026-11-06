#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

public static class LobbyLayoutBuilder
{
    // ==================== PALET WARNA ====================
    static readonly Color BgDark     = new Color(0.015f, 0.03f, 0.07f, 1f);
    static readonly Color PanelNavy  = new Color(0.02f, 0.08f, 0.2f, 0.88f);
    static readonly Color CyanGlow   = new Color(0.2f, 0.7f, 1f, 1f);
    static readonly Color Gold       = new Color(1f, 0.75f, 0.2f, 1f);
    static readonly Color RedBadge   = new Color(0.9f, 0.1f, 0.1f, 1f);
    static readonly Color ActiveBlue = new Color(0.05f, 0.3f, 0.85f, 0.95f);

    [MenuItem("Tools/Lobby/Build Lobby Layout (Final Chamfer)")]
    public static void Build()
    {
        var old = GameObject.Find("LobbyCanvas");
        if (old) Undo.DestroyObjectImmediate(old);

        var canvasGO = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build Lobby Layout");

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (!Object.FindObjectOfType<EventSystem>())
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        var root = canvasGO.transform;

        // ==================== BACKGROUND ====================
        // Background utama (bisa diganti sprite nanti)
        var bg = Img(root, "Background", BgDark);
        Stretch(bg.rectTransform);

        // Overlay gelap biar lebih dalam (opsional)
        var overlay = Img(root, "BackgroundOverlay", new Color(0, 0, 0, 0.25f));
        Stretch(overlay.rectTransform);

        BuildProfile(root);
        BuildTopRight(root);
        BuildLeftColumn(root);
        BuildRightColumn(root);
        BuildBottomBar(root);
        BuildDeploy(root);

        Selection.activeGameObject = canvasGO;
        Debug.Log("✅ Layout selesai! Ukuran sudah disesuaikan + Background ditambahkan.");
    }

    // =====================================================================
    //  HELPER: BUAT SPRITE CHAMFER
    // =====================================================================
    static Sprite CreateChamferedSprite(int width, int height, int cutSize)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool cutTL = (x + y) < cutSize;
                bool cutBR = ((width - 1 - x) + (height - 1 - y)) < cutSize;

                if (cutTL || cutBR)
                    pixels[y * width + x] = Color.clear;
                else
                    pixels[y * width + x] = Color.white;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    // =====================================================================
    //  PANEL PROFIL (KIRI ATAS) - sedikit lebih kecil
    // =====================================================================
    static void BuildProfile(Transform root)
    {
        var p = ChamferPanel(root, "ProfilePanel", new Vector2(420, 130), PanelNavy, 16);
        Place(p, new Vector2(0, 1), new Vector2(28, -28), new Vector2(420, 130));

        var avatar = ChamferPanel(p, "AvatarFrame", new Vector2(100, 100), new Color(0.1f, 0.2f, 0.4f), 12);
        Place(avatar, new Vector2(0, 1), new Vector2(14, -14), new Vector2(100, 100));

        Label(p, "NameText", "FamaLine", 26, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(130, -14), new Vector2(270, 32));

        Label(p, "LevelText", "Lv. 12", 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(130, -50), new Vector2(80, 28));

        // EXP Bar
        var expBar = Img(p, "ExpBar_BG", new Color(0.1f, 0.15f, 0.3f));
        Place(expBar.rectTransform, new Vector2(0, 1), new Vector2(200, -56), new Vector2(190, 9));

        var expFill = Img(expBar.transform, "ExpBar_Fill", CyanGlow);
        expFill.rectTransform.anchorMin = Vector2.zero;
        expFill.rectTransform.anchorMax = new Vector2(0.36f, 1f);
        expFill.rectTransform.offsetMin = expFill.rectTransform.offsetMax = Vector2.zero;

        Label(p, "ExpText", "EXP 720 / 2,000", 13, FontStyles.Normal, CyanGlow, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(200, -68), new Vector2(190, 18));

        var rankIcon = Img(p, "RankIcon", Gold);
        Place(rankIcon.rectTransform, new Vector2(0, 1), new Vector2(130, -90), new Vector2(26, 26));

        Label(p, "RankText", "Silver III", 18, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(165, -90), new Vector2(150, 26));
    }

    // =====================================================================
    //  MATA UANG & TOMBOL (KANAN ATAS)
    // =====================================================================
    static void BuildTopRight(Transform root)
    {
        var g = Group(root, "TopRightGroup", true, 12, TextAnchor.MiddleRight);
        Place(g, new Vector2(1, 1), new Vector2(-28, -36), new Vector2(580, 46));

        CurrencyPill(g, "GemPill", "1,200", CyanGlow);
        CurrencyPill(g, "CoinPill", "15,340", Gold);

        IconButton(g, "Btn_Bell", true);
        IconButton(g, "Btn_Friends", false);
        IconButton(g, "Btn_Settings", false);
    }

    static void CurrencyPill(Transform parent, string name, string value, Color iconColor)
    {
        var p = ChamferPanel(parent, name, new Vector2(158, 40), PanelNavy, 9);

        var icon = Img(p, "Icon", iconColor);
        Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(11, 0), new Vector2(22, 22));

        Label(p, "Value", value, 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 0.5f), new Vector2(42, 0), new Vector2(75, 28));

        Label(p, "Plus", "+", 24, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-8, 0), new Vector2(28, 28));
    }

    static void IconButton(Transform parent, string name, bool hasBadge)
    {
        var b = ChamferPanel(parent, name, new Vector2(44, 44), PanelNavy, 9);

        var icon = Img(b, "Icon", CyanGlow);
        Place(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24, 24));

        if (hasBadge)
        {
            var dot = Img(b, "Badge", RedBadge);
            Place(dot.rectTransform, new Vector2(1, 1), new Vector2(-2, -2), new Vector2(14, 14));
        }
    }

    // =====================================================================
    //  KOLOM KIRI
    // =====================================================================
    static void BuildLeftColumn(Transform root)
    {
        // New Agent Banner
        var banner = ChamferPanel(root, "NewAgentBanner", new Vector2(380, 140), new Color(0.05f, 0.15f, 0.35f, 0.92f), 14);
        Place(banner, new Vector2(0, 0), new Vector2(28, 340), new Vector2(380, 140));

        var charaArt = Img(banner, "AgentArt", new Color(1, 1, 1, 0));
        Place(charaArt.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(165, 140));

        Label(banner, "Title", "NEW AGENT\n\"VYRA\"", 24, FontStyles.Bold | FontStyles.Italic, Color.white, TextAlignmentOptions.Left,
              new Vector2(1, 1), new Vector2(-14, -14), new Vector2(190, 55));

        var getBtn = ChamferPanel(banner, "GetNowButton", new Vector2(130, 34), new Color(0.1f, 0.4f, 0.9f), 9);
        Place(getBtn, new Vector2(1, 0), new Vector2(-14, 14), new Vector2(130, 34));
        Label(getBtn, "Text", "GET NOW »", 15, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130, 34));

        // Notice
        var notice = ChamferPanel(root, "NoticePanel", new Vector2(380, 100), PanelNavy, 12);
        Place(notice, new Vector2(0, 0), new Vector2(28, 220), new Vector2(380, 100));

        var iconNotice = Img(notice, "Icon", CyanGlow);
        Place(iconNotice.rectTransform, new Vector2(0, 1), new Vector2(14, -14), new Vector2(22, 22));

        Label(notice, "Title", "NOTICE", 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(46, -14), new Vector2(140, 22));

        Label(notice, "Body", "Maintenance will be on\nNov 10, 08:00 (UTC+7).", 15, FontStyles.Normal, new Color(0.8f, 0.9f, 1f), TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(14, 14), new Vector2(300, 45));

        Label(notice, "Arrow", ">", 22, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(28, 28));
    }

    // =====================================================================
    //  KOLOM KANAN
    // =====================================================================
    static void BuildRightColumn(Transform root)
    {
        // Daily Mission
        var dm = ChamferPanel(root, "DailyMission", new Vector2(340, 92), PanelNavy, 12);
        Place(dm, new Vector2(1, 1), new Vector2(-28, -95), new Vector2(340, 92));

        var dmi = Img(dm, "Icon", CyanGlow);
        Place(dmi.rectTransform, new Vector2(0, 1), new Vector2(14, -14), new Vector2(22, 22));

        Label(dm, "Title", "DAILY MISSION", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(46, -14), new Vector2(180, 22));

        Label(dm, "Progress", "0 / 3", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Right,
              new Vector2(1, 1), new Vector2(-36, -14), new Vector2(70, 22));

        Label(dm, "Desc", "Play 3 matches", 15, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(14, -46), new Vector2(230, 22));

        Label(dm, "Reward", "x100", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(1, 0), new Vector2(-75, 14), new Vector2(55, 22));

        Label(dm, "Arrow", ">", 22, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(28, 28));

        // Season Rank
        var sr = ChamferPanel(root, "SeasonRank", new Vector2(340, 110), PanelNavy, 12);
        Place(sr, new Vector2(1, 1), new Vector2(-28, -200), new Vector2(340, 110));

        var srIcon = Img(sr, "Icon_Badge", Gold);
        Place(srIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(36, 0), new Vector2(54, 54));

        Label(sr, "Label", "SEASON RANK", 13, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(105, -16), new Vector2(180, 18));

        Label(sr, "RankName", "Bronze I", 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(105, -40), new Vector2(180, 28));

        Label(sr, "RankPoint", "0 / 1000", 13, FontStyles.Normal, CyanGlow, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(105, 16), new Vector2(140, 18));

        Label(sr, "Arrow", ">", 22, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(28, 28));

        // New Map
        var nm = ChamferPanel(root, "NewMap", new Vector2(340, 140), new Color(0.05f, 0.1f, 0.2f), 12);
        Place(nm, new Vector2(1, 0), new Vector2(-28, 160), new Vector2(340, 140));

        var mapArt = Img(nm, "MapArt", new Color(0.1f, 0.2f, 0.4f));
        Stretch(mapArt.rectTransform, 4);

        Label(nm, "Tag", "NEW MAP", 13, FontStyles.Bold, Gold, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(14, 38), new Vector2(180, 18));

        Label(nm, "MapName", "RESEARCH FACILITY »", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(14, 14), new Vector2(280, 24));
    }

    // =====================================================================
    //  BOTTOM BAR (ukuran lebih pas)
    // =====================================================================
    static void BuildBottomBar(Transform root)
    {
        var g = Group(root, "BottomBarGroup", true, 11, TextAnchor.LowerLeft);
        Place(g, new Vector2(0, 0), new Vector2(28, 28), new Vector2(1050, 92));

        MenuButton(g, "Operations", "OPERATIONS", "Play Missions", true, true);
        MenuButton(g, "Agents", "AGENTS", "Select Character", false, false);
        MenuButton(g, "Armory", "ARMORY", "Upgrade Weapons", false, false);
        MenuButton(g, "Inventory", "INVENTORY", "Manage Items", false, false);
        MenuButton(g, "Store", "STORE", "Buy Items", false, false);
        MenuButton(g, "Settings", "SETTINGS", "Game Settings", false, false);
    }

    static void MenuButton(Transform parent, string name, string title, string sub, bool isActive, bool hasBadge)
    {
        Color baseColor = isActive ? ActiveBlue : PanelNavy;

        var container = Rect(name + "_Container", parent);
        container.sizeDelta = new Vector2(148, 92);          // ← ukuran lebih pas

        var sprite = CreateChamferedSprite(148, 92, 20);
        var bgImg = container.gameObject.AddComponent<Image>();
        bgImg.sprite = sprite;
        bgImg.color = baseColor;
        bgImg.type = Image.Type.Simple;
        bgImg.raycastTarget = true;

        var outline = container.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.65f, 1f, 0.75f);
        outline.effectDistance = new Vector2(2, -2);

        // Accent line
        var line = Img(container, "AccentLine", isActive ? CyanGlow : new Color(0.2f, 0.5f, 1f, 0.55f));
        Place(line.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -3), new Vector2(110, 3));

        // Icon
        var icon = Img(container, "Icon", isActive ? Color.white : CyanGlow);
        Place(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(32, 32));

        // Texts
        Label(container, "Title", title, 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(140, 22));

        Label(container, "Sub", sub, 12, FontStyles.Normal, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(140, 18));

        if (hasBadge)
        {
            var dot = Img(container, "Badge", RedBadge);
            Place(dot.rectTransform, new Vector2(0.82f, 1), new Vector2(0, -5), new Vector2(14, 14));
        }

        var btn = container.gameObject.AddComponent<Button>();
        btn.targetGraphic = bgImg;
    }

    // =====================================================================
    //  DEPLOY BUTTON
    // =====================================================================
    static void BuildDeploy(Transform root)
    {
        var d = ChamferPanel(root, "DeployButton", new Vector2(380, 100), new Color(0.1f, 0.4f, 0.95f), 16);
        Place(d, new Vector2(1, 0), new Vector2(-28, 48), new Vector2(380, 100));

        var topLine = Img(d, "TopLine", CyanGlow);
        Place(topLine.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -2), new Vector2(360, 4));

        Label(d, "Title", "DEPLOY »", 44, FontStyles.Bold | FontStyles.Italic, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(360, 55));

        Label(d, "Sub", "ENTER THE BATTLEFIELD", 15, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(360, 22));
    }

    // =====================================================================
    //  HELPER FUNCTIONS
    // =====================================================================
    static RectTransform ChamferPanel(Transform parent, string name, Vector2 size, Color fill, int cutSize)
    {
        var rt = Rect(name, parent);
        rt.sizeDelta = size;

        var sprite = CreateChamferedSprite((int)size.x, (int)size.y, cutSize);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = fill;
        img.type = Image.Type.Simple;
        img.raycastTarget = true;

        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

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
    }
}
#endif
