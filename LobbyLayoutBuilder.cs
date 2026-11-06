#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

/// <summary>
/// Letakkan di folder Assets/Editor/ lalu jalankan: Tools > Lobby > Build Lobby Layout
/// Membuat seluruh UI lobby (tanpa logo PROJECT ECLIPSE) dengan Canvas 1920x1080.
/// Semua panel memakai Layout Group + spacing, jadi tidak berdempetan.
/// </summary>
public static class LobbyLayoutBuilder
{
    // ---------- Palet warna (gaya biru cyber) ----------
    static readonly Color Navy   = new Color(0.02f, 0.07f, 0.17f, 0.88f);
    static readonly Color Border = new Color(0.16f, 0.46f, 1.00f, 1f);
    static readonly Color Cyan   = new Color(0.45f, 0.80f, 1.00f, 1f);
    static readonly Color Gold   = new Color(1.00f, 0.80f, 0.25f, 1f);
    static readonly Color Yellow = new Color(1.00f, 0.88f, 0.20f, 1f);
    static readonly Color Red    = new Color(1.00f, 0.20f, 0.25f, 1f);
    static readonly Color Bright = new Color(0.10f, 0.38f, 0.95f, 1f);

    [MenuItem("Tools/Lobby/Build Lobby Layout")]
    public static void Build()
    {
        var old = GameObject.Find("LobbyCanvas");
        if (old) Undo.DestroyObjectImmediate(old);

        // ---------- Canvas ----------
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
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Build Lobby Layout");
        }

        var root = canvasGO.transform;

        // ---------- Background & slot karakter ----------
        var bg = Img(root, "Background", new Color(0.01f, 0.03f, 0.09f, 1f));
        Stretch(bg.rectTransform);
        var chara = Img(root, "CharacterSlot", new Color(1, 1, 1, 0.0f));
        Place(chara.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(520, 820));

        BuildProfile(root);
        BuildTopRight(root);
        BuildLeftColumn(root);
        BuildRightColumn(root);
        BuildBottomBar(root);
        BuildDeploy(root);

        Selection.activeGameObject = canvasGO;
        Debug.Log("Lobby layout selesai dibuat. Isi sprite pada Background, CharacterSlot, dan semua objek bernama 'Icon'.");
    }

    // =====================================================================
    //  PROFIL (kiri atas)
    // =====================================================================
    static void BuildProfile(Transform root)
    {
        var p = Panel(root, "ProfilePanel", new Vector2(420, 120));
        Place(p, new Vector2(0, 1), new Vector2(30, -30), new Vector2(420, 120));

        var avatar = Panel(p, "Avatar", new Vector2(92, 92), new Color(0.05f, 0.12f, 0.25f, 1f));
        Place(avatar, new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(92, 92));

        Label(p, "Name", "FamaLine", 26, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(120, -12), new Vector2(260, 32));
        Label(p, "Level", "Lv. 12", 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(120, -46), new Vector2(70, 28));

        // EXP bar
        var barBg = Img(p, "ExpBar", new Color(0.1f, 0.2f, 0.4f, 1f));
        Place(barBg.rectTransform, new Vector2(0, 1), new Vector2(196, -56), new Vector2(200, 8));
        var fill = Img(barBg.transform, "Fill", Border);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0.36f, 1f);
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;

        Label(p, "ExpText", "EXP 720 / 2,000", 13, FontStyles.Normal, Cyan, TextAlignmentOptions.Right,
              new Vector2(0, 1), new Vector2(196, -68), new Vector2(200, 18));

        var rankIcon = Img(p, "Icon", Cyan);
        Place(rankIcon.rectTransform, new Vector2(0, 1), new Vector2(120, -88), new Vector2(26, 26));
        Label(p, "RankText", "Silver III", 18, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(154, -88), new Vector2(150, 26));
    }

    // =====================================================================
    //  MATA UANG + TOMBOL IKON (kanan atas)
    // =====================================================================
    static void BuildTopRight(Transform root)
    {
        var g = Group(root, "TopRight", true, 12, TextAnchor.MiddleRight);
        Place(g, new Vector2(1, 1), new Vector2(-30, -30), new Vector2(560, 46));

        CurrencyPill(g, "GemPill", "1,200", 170, Cyan);
        CurrencyPill(g, "CoinPill", "15,340", 190, Gold);
        IconButton(g, "BellButton", true);
        IconButton(g, "FriendsButton", false);
        IconButton(g, "SettingsButton", false);
    }

    static void CurrencyPill(Transform parent, string name, string value, float width, Color iconColor)
    {
        var p = Panel(parent, name, new Vector2(width, 46));
        var icon = Img(p, "Icon", iconColor);
        Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(26, 26));
        Label(p, "Value", value, 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 0.5f), new Vector2(48, 0), new Vector2(width - 100, 30));
        var plus = Panel(p, "PlusButton", new Vector2(30, 30), Navy, true);
        Place(plus, new Vector2(1, 0.5f), new Vector2(-8, 0), new Vector2(30, 30));
        Label(plus, "Plus", "+", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
    }

    static void IconButton(Transform parent, string name, bool badge)
    {
        var b = Panel(parent, name, new Vector2(46, 46), Navy, true);
        var icon = Img(b, "Icon", Cyan);
        Place(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24, 24));
        if (badge)
        {
            var dot = Img(b, "Badge", Red);
            Place(dot.rectTransform, new Vector2(1, 1), new Vector2(2, 2), new Vector2(14, 14));
        }
    }

    // =====================================================================
    //  KOLOM KIRI: NEW AGENT + NOTICE
    // =====================================================================
    static void BuildLeftColumn(Transform root)
    {
        var col = Group(root, "LeftColumn", false, 16, TextAnchor.LowerLeft);
        Place(col, new Vector2(0, 0), new Vector2(30, 150), new Vector2(380, 246));

        // Banner agen baru
        var banner = Panel(col, "NewAgentBanner", new Vector2(380, 130), new Color(0.04f, 0.12f, 0.3f, 0.9f), true);
        var art = Img(banner, "AgentArt", new Color(0.1f, 0.2f, 0.45f, 1f));
        Place(art.rectTransform, new Vector2(0, 0.5f), new Vector2(6, 0), new Vector2(150, 118));
        Label(banner, "Title", "NEW AGENT\n\"VYRA\"", 24, FontStyles.Bold | FontStyles.Italic, Color.white,
              TextAlignmentOptions.Left, new Vector2(1, 1), new Vector2(-14, -14), new Vector2(200, 64));
        var get = Panel(banner, "GetNowButton", new Vector2(130, 30), Bright, true);
        Place(get, new Vector2(1, 0), new Vector2(-14, 12), new Vector2(130, 30));
        Label(get, "Text", "GET NOW »", 15, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130, 30));

        // Notice
        var notice = Panel(col, "NoticePanel", new Vector2(380, 100), Navy, true);
        var ic = Img(notice, "Icon", Cyan);
        Place(ic.rectTransform, new Vector2(0, 1), new Vector2(14, -14), new Vector2(22, 22));
        Label(notice, "Title", "NOTICE", 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(44, -12), new Vector2(200, 26));
        Label(notice, "Body", "Maintenance will be on\nNov 10, 08:00 (UTC+7).", 16, FontStyles.Normal, Color.white,
              TextAlignmentOptions.Left, new Vector2(0, 0), new Vector2(14, 12), new Vector2(320, 52));
        Label(notice, "Arrow", ">", 22, FontStyles.Bold, Cyan, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-12, 0), new Vector2(24, 30));
    }

    // =====================================================================
    //  KOLOM KANAN: DAILY MISSION, SEASON RANK, NEW MAP
    // =====================================================================
    static void BuildRightColumn(Transform root)
    {
        var col = Group(root, "RightColumn", false, 16, TextAnchor.UpperRight);
        Place(col, new Vector2(1, 1), new Vector2(-30, -110), new Vector2(340, 392));

        // Daily mission
        var dm = Panel(col, "DailyMission", new Vector2(340, 118), Navy, true);
        var dmIcon = Img(dm, "Icon", Cyan);
        Place(dmIcon.rectTransform, new Vector2(0, 1), new Vector2(14, -14), new Vector2(26, 26));
        Label(dm, "Title", "DAILY MISSION", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(50, -14), new Vector2(190, 26));
        Label(dm, "Progress", "0 / 3", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Right,
              new Vector2(1, 1), new Vector2(-16, -14), new Vector2(80, 26));
        Label(dm, "Desc", "Play 3 matches", 16, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(14, -50), new Vector2(220, 24));
        var gem = Img(dm, "Icon", Cyan);
        Place(gem.rectTransform, new Vector2(1, 0), new Vector2(-96, 14), new Vector2(24, 24));
        Label(dm, "Reward", "x100", 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(1, 0), new Vector2(-40, 12), new Vector2(54, 28));
        Label(dm, "Arrow", ">", 22, FontStyles.Bold, Cyan, TextAlignmentOptions.Center,
              new Vector2(1, 0), new Vector2(-10, 12), new Vector2(24, 28));

        // Season rank
        var sr = Panel(col, "SeasonRank", new Vector2(340, 112), Navy, true);
        var badge = Img(sr, "Icon", Gold);
        Place(badge.rectTransform, new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(64, 64));
        Label(sr, "Label", "SEASON RANK", 14, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(92, -14), new Vector2(200, 20));
        Label(sr, "RankName", "Bronze I", 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 1), new Vector2(92, -36), new Vector2(180, 30));
        var bar = Img(sr, "RankBar", new Color(0.1f, 0.2f, 0.4f, 1f));
        Place(bar.rectTransform, new Vector2(0, 0), new Vector2(92, 36), new Vector2(190, 6));
        Label(sr, "RankPoint", "0 / 1000", 14, FontStyles.Normal, Cyan, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(92, 10), new Vector2(150, 20));
        Label(sr, "Arrow", ">", 22, FontStyles.Bold, Cyan, TextAlignmentOptions.Center,
              new Vector2(1, 0.5f), new Vector2(-12, 0), new Vector2(24, 30));

        // New map
        var nm = Panel(col, "NewMap", new Vector2(340, 130), Navy, true);
        var mapArt = Img(nm, "MapArt", new Color(0.1f, 0.22f, 0.45f, 1f));
        Stretch(mapArt.rectTransform, 3);
        Label(nm, "Tag", "NEW MAP", 14, FontStyles.Bold, Yellow, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(14, 34), new Vector2(200, 20));
        Label(nm, "MapName", "RESEARCH FACILITY »", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
              new Vector2(0, 0), new Vector2(14, 10), new Vector2(300, 26));
    }

    // =====================================================================
    //  TOMBOL MENU BAWAH
    // =====================================================================
    static void BuildBottomBar(Transform root)
    {
        var bar = Group(root, "BottomMenu", true, 14, TextAnchor.LowerLeft);
        Place(bar, new Vector2(0, 0), new Vector2(30, 30), new Vector2(880, 92));

        MenuButton(bar, "Operations", "OPERATIONS", "Play Missions", true, true);
        MenuButton(bar, "Agents", "AGENTS", "Select Character", false, false);
        MenuButton(bar, "Armory", "ARMORY", "Upgrade Weapons", false, false);
        MenuButton(bar, "Inventory", "INVENTORY", "Manage Items", false, false);
        MenuButton(bar, "Store", "STORE", "Buy Items", false, false);
        MenuButton(bar, "Settings", "SETTINGS", "Game Settings", false, false);
    }

    static void MenuButton(Transform parent, string name, string title, string sub, bool active, bool badge)
    {
        var fill = active ? new Color(0.05f, 0.18f, 0.45f, 0.95f) : Navy;
        var b = Panel(parent, name + "Button", new Vector2(134, 92), fill, true);
        var icon = Img(b, "Icon", Cyan);
        Place(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(34, 34));
        Label(b, "Title", title, 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(130, 24));
        Label(b, "Sub", sub, 13, FontStyles.Normal, Cyan, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(130, 20));
        if (badge)
        {
            var dot = Img(b, "Badge", Red);
            Place(dot.rectTransform, new Vector2(1, 1), new Vector2(2, 2), new Vector2(16, 16));
        }
    }

    // =====================================================================
    //  TOMBOL DEPLOY (kanan bawah)
    // =====================================================================
    static void BuildDeploy(Transform root)
    {
        var d = Panel(root, "DeployButton", new Vector2(340, 92), Bright, true);
        Place(d, new Vector2(1, 0), new Vector2(-30, 30), new Vector2(340, 92));
        Label(d, "Title", "DEPLOY »", 40, FontStyles.Bold | FontStyles.Italic, Color.white, TextAlignmentOptions.Center,
              new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(320, 52));
        Label(d, "Sub", "ENTER THE BATTLEFIELD", 15, FontStyles.Bold, Cyan, TextAlignmentOptions.Center,
              new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(320, 22));
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

    // Panel dengan garis tepi biru; clickable = ditambah komponen Button
    static RectTransform Panel(Transform parent, string name, Vector2 size, Color? fill = null, bool clickable = false)
    {
        var rt = Rect(name, parent);
        rt.sizeDelta = size;
        var border = rt.gameObject.AddComponent<Image>();
        border.color = Border;
        var f = Img(rt, "Fill", fill ?? Navy);
        Stretch(f.rectTransform, 2);

        if (clickable)
        {
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = border;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.75f, 0.92f, 1f);
            cb.pressedColor = new Color(0.55f, 0.75f, 1f);
            btn.colors = cb;
        }
        else border.raycastTarget = false;
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
