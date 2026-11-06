#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

public static class LobbyLayoutBuilder
{
    // ==================== PALET WARNA ====================
    static readonly Color BgDark     = new Color(0.02f, 0.04f, 0.08f, 1f);
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

        // Background
        var bg = Img(root, "Background_Image", BgDark);
        Stretch(bg.rectTransform);

        BuildProfile(root);
        BuildTopRight(root);
        BuildLeftColumn(root);
        BuildRightColumn(root);
        BuildBottomBar(root);
        BuildDeploy(root);

        Selection.activeGameObject = canvasGO;
        Debug.Log("✅ Layout Final Selesai! Semua tombol & panel sudah memakai chamfer.");
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

                // Kalau mau 4 sudut, uncomment 2 baris di bawah:
                // bool cutTR = ((width - 1 - x) + y) < cutSize;
                // bool cutBL = (x + (height - 1 - y)) < cutSize;

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
    //  PANEL PROFIL (KIRI ATAS)
    // =====================================================================
    static void BuildProfile(Transform root)
    {
        var p = ChamferPanel(root, "ProfilePanel", new Vector2(450, 140), PanelNavy, 18);
        Place(p, new Vector2(0, 1), new Vector2(30, -30), new Vector2(450, 140));

        // Avatar frame
        var avatar = ChamferPanel(p, "AvatarFrame", new Vector2(110, 110), new Color(0.1f, 0.2f, 0.4f), 12);
        Place(avatar, new Vector2(0, 1), new Vector2(15, -15), new Vector2(110, 110));

        Label(p, "NameText", "FamaLine", 28, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(140, -15), new Vector2(280, 35));

        Label(p, "LevelText", "Lv. 12", 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(140, -55), new Vector2(80, 30));

        // EXP Bar
        var expBar = Img(p, "ExpBar_BG", new Color(0.1f, 0.15f, 0.3f));
        Place(expBar.rectTransform, new Vector2(0, 1), new Vector2(215, -62), new Vector2(200, 10));

        var expFill = Img(expBar.transform, "ExpBar_Fill", CyanGlow);
        expFill.rectTransform.anchorMin = Vector2.zero;
        expFill.rectTransform.anchorMax = new Vector2(0.36f, 1f);
        expFill.rectTransform.offsetMin = expFill.rectTransform.offsetMax = Vector2.zero;

        Label(p, "ExpText", "EXP 720 / 2,000", 14, FontStyles.Normal, CyanGlow, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(215, -75), new Vector2(200, 20));

        var rankIcon = Img(p, "RankIcon", Gold);
        Place(rankIcon.rectTransform, new Vector2(0, 1), new Vector2(140, -95), new Vector2(30, 30));

        Label(p, "RankText", "Silver III", 20, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(180, -95), new Vector2(150, 30));
    }

    // =====================================================================
    //  MATA UANG & TOMBOL (KANAN ATAS)
    // =====================================================================
    static void BuildTopRight(Transform root)
    {
        var g = Group(root, "TopRightGroup", true, 15, TextAnchor.MiddleRight);
        Place(g, new Vector2(1, 1), new Vector2(-30, -40), new Vector2(600, 50));

        CurrencyPill(g, "GemPill", "1,200", CyanGlow);
        CurrencyPill(g, "CoinPill", "15,340", Gold);

        IconButton(g, "Btn_Bell", true);
        IconButton(g, "Btn_Friends", false);
        IconButton(g, "Btn_Settings", false);
    }

    static void CurrencyPill(Transform parent, string name, string value, Color iconColor)
    {
        var p = ChamferPanel(parent, name, new Vector2(170, 42), PanelNavy, 10);
        
        var icon = Img(p, "Icon", iconColor);
        Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(24, 24));

        Label(p, "Value", value, 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 0.5f), new Vector2(45, 0), new Vector2(80, 30));

        Label(p, "Plus", "+", 26, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(30, 30));
    }

    static void IconButton(Transform parent, string name, bool hasBadge)
    {
        var b = ChamferPanel(parent, name, new Vector2(48, 48), PanelNavy, 10);

        var icon = Img(b, "Icon", CyanGlow);
        Place(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26, 26));

        if (hasBadge)
        {
            var dot = Img(b, "Badge", RedBadge);
            Place(dot.rectTransform, new Vector2(1, 1), new Vector2(-2, -2), new Vector2(16, 16));
        }
    }

    // =====================================================================
    //  KOLOM KIRI
    // =====================================================================
    static void BuildLeftColumn(Transform root)
    {
        // New Agent Banner
        var banner = ChamferPanel(root, "NewAgentBanner", new Vector2(400, 150), new Color(0.05f, 0.15f, 0.35f, 0.92f), 16);
        Place(banner, new Vector2(0, 0), new Vector2(30, 360), new Vector2(400, 150));

        var charaArt = Img(banner, "AgentArt", new Color(1, 1, 1, 0));
        Place(charaArt.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(180, 150));

        Label(banner, "Title", "NEW AGENT\n\"VYRA\"", 26, FontStyles.Bold | FontStyles.Italic, Color.white, TextAlignmentOptions.Left,
              new Vector2(1, 1), new Vector2(-15, -15), new Vector2(200, 60));

        var getBtn = ChamferPanel(banner, "GetNowButton", new Vector2(140, 36), new Color(0.1f, 0.4f, 0.9f), 10);
        Place(getBtn, new Vector2(1, 0), new Vector2(-15, 15), new Vector2(140, 36));
        Label(getBtn, "Text", "GET NOW »", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 36));

        // Notice
        var notice = ChamferPanel(root, "NoticePanel", new Vector2(400, 110), PanelNavy, 14);
        Place(notice, new Vector2(0, 0), new Vector2(30, 230), new Vector2(400, 110));

        var iconNotice = Img(notice, "Icon", CyanGlow);
        Place(iconNotice.rectTransform, new Vector2(0, 1), new Vector2(15, -15), new Vector2(24, 24));

        Label(notice, "Title", "NOTICE", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(50, -15), new Vector2(150, 24));

        Label(notice, "Body", "Maintenance will be on\nNov 10, 08:00 (UTC+7).", 16, FontStyles.Normal, new Color(0.8f, 0.9f, 1f), TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(15, 15), new Vector2(320, 50));

        Label(notice, "Arrow", ">", 24, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-15, 0), new Vector2(30, 30));
    }

    // =====================================================================
    //  KOLOM KANAN
    // =====================================================================
    static void BuildRightColumn(Transform root)
    {
        // Daily Mission
        var dm = ChamferPanel(root, "DailyMission", new Vector2(360, 100), PanelNavy, 14);
        Place(dm, new Vector2(1, 1), new Vector2(-30, -100), new Vector2(360, 100));

        var dmi = Img(dm, "Icon", CyanGlow);
        Place(dmi.rectTransform, new Vector2(0, 1), new Vector2(15, -15), new Vector2(24, 24));

        Label(dm, "Title", "DAILY MISSION", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(50, -15), new Vector2(200, 24));

        Label(dm, "Progress", "0 / 3", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Right,
              new Vector2(1, 1), new Vector2(-40, -15), new Vector2(80, 24));

        Label(dm, "Desc", "Play 3 matches", 16, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(15, -50), new Vector2(250, 24));

        Label(dm, "Reward", "x100", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(1, 0), new Vector2(-80, 15), new Vector2(60, 24));

        Label(dm, "Arrow", ">", 24, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-15, 0), new Vector2(30, 30));

        // Season Rank
        var sr = ChamferPanel(root, "SeasonRank", new Vector2(360, 120), PanelNavy, 14);
        Place(sr, new Vector2(1, 1), new Vector2(-30, -215), new Vector2(360, 120));

        var srIcon = Img(sr, "Icon_Badge", Gold);
        Place(srIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(60, 60));

        Label(sr, "Label", "SEASON RANK", 14, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(110, -18), new Vector2(200, 20));

        Label(sr, "RankName", "Bronze I", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(110, -45), new Vector2(200, 30));

        Label(sr, "RankPoint", "0 / 1000", 14, FontStyles.Normal, CyanGlow, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(110, 18), new Vector2(150, 20));

        Label(sr, "Arrow", ">", 24, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-15, 0), new Vector2(30, 30));

        // New Map
        var nm = ChamferPanel(root, "NewMap", new Vector2(360, 150), new Color(0.05f, 0.1f, 0.2f), 14);
        Place(nm, new Vector2(1, 0), new Vector2(-30, 170), new Vector2(360, 150));

        var mapArt = Img(nm, "MapArt", new Color(0.1f, 0.2f, 0.4f));
        Stretch(mapArt.rectTransform, 4);

        Label(nm, "Tag", "NEW MAP", 14, FontStyles.Bold, Gold, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(15, 42), new Vector2(200, 20));

        Label(nm, "MapName", "RESEARCH FACILITY »", 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(15, 15), new Vector2(300, 26));
    }

    // =====================================================================
    //  BOTTOM BAR
    // =====================================================================
    static void BuildBottomBar(Transform root)
    {
        var g = Group(root, "BottomBarGroup", true, 12, TextAnchor.LowerLeft);
        Place(g, new Vector2(0, 0), new Vector2(30, 30), new Vector2(1100, 100));

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
        container.sizeDelta = new Vector2(160, 100);

        // Chamfer sprite
        var sprite = CreateChamferedSprite(160, 100, 22);
        var bgImg = container.gameObject.AddComponent<Image>();
        bgImg.sprite = sprite;
        bgImg.color = baseColor;
        bgImg.type = Image.Type.Simple;
        bgImg.raycastTarget = true;

        // Outline
        var outline = container.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.65f, 1f, 0.75f);
        outline.effectDistance = new Vector2(2, -2);

        // Accent line
        var line = Img(container, "AccentLine", isActive ? CyanGlow : new Color(0.2f, 0.5f, 1f, 0.55f));
        Place(line.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -3), new Vector2(120, 4));

        // Icon
        var icon = Img(container, "Icon", isActive ? Color.white : CyanGlow);
        Place(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(36, 36));

        // Texts
        Label(container, "Title", title, 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(150, 24));

        Label(container, "Sub", sub, 13, FontStyles.Normal, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 13), new Vector2(150, 20));

        if (hasBadge)
        {
            var dot = Img(container, "Badge", RedBadge);
            Place(dot.rectTransform, new Vector2(0.82f, 1), new Vector2(0, -6), new Vector2(16, 16));
        }

        var btn = container.gameObject.AddComponent<Button>();
        btn.targetGraphic = bgImg;
    }

    // =====================================================================
    //  DEPLOY BUTTON
    // =====================================================================
    static void BuildDeploy(Transform root)
    {
        var d = ChamferPanel(root, "DeployButton", new Vector2(400, 110), new Color(0.1f, 0.4f, 0.95f), 18);
        Place(d, new Vector2(1, 0), new Vector2(-30, 50), new Vector2(400, 110));

        var topLine = Img(d, "TopLine", CyanGlow);
        Place(topLine.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -2), new Vector2(380, 5));

        Label(d, "Title", "DEPLOY »", 48, FontStyles.Bold | FontStyles.Italic, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(380, 60));

        Label(d, "Sub", "ENTER THE BATTLEFIELD", 16, FontStyles.Bold, CyanGlow, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(380, 24));
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
