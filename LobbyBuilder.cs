using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum At { TL, TR, BL, BR, BC }

public class LobbyBuilder : MonoBehaviour
{
    [Header("Assets (logo & teks PROJECT ECLIPSE TIDAK dibuat)")]
    public Sprite background;       // Ruangan + globe, TANPA karakter & UI
    public Sprite character;        // Karakter PNG transparan
    public Sprite avatar, agentBanner, mapBanner, rankIcon;
    public Sprite gemIcon, coinIcon, bellIcon, friendsIcon, settingsIcon, speakerIcon;
    public Sprite[] navIcons = new Sprite[6];
    public TMP_FontAsset font;

    [Header("Tema tunggal (semua UI membaca dari sini)")]
    public Color accent = new Color(0.20f, 0.55f, 1f);
    public Color accentBright = new Color(0.45f, 0.85f, 1f);
    public Color panelColor = new Color(0.03f, 0.08f, 0.17f, 0.85f);
    public Color borderColor = new Color(0.30f, 0.70f, 1f, 1f);
    public Color textMain = Color.white;
    public Color textDim = new Color(0.70f, 0.82f, 1f);
    public Color alert = new Color(1f, 0.2f, 0.2f);

    [Header("Bentuk (clip path)")]
    public float chamfer = 16f;
    public float borderWidth = 2f;

    const float W = 1920f, H = 1080f;

    // ================= ENTRY POINT =================

    public void Build()
    {
        Clear();

        var go = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);

        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(W, H);
        sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        sc.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(transform, false);
        }

        var root = (RectTransform)go.transform;
        BuildBackground(root);
        BuildCharacter(root);

        var safe = Fill("SafeArea", root);
        ApplySafeArea(safe);

        BuildTopBar(safe);
        BuildLeftColumn(safe);
        BuildRightColumn(safe);
        BuildBottomBar(safe);
    }

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    // ================= SECTIONS =================

    void BuildBackground(RectTransform root)
    {
        var clip = Fill("BackgroundClip", root);
        clip.gameObject.AddComponent<RectMask2D>();
        var bg = Pin("Background", clip, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, Vector2.one);
        Img(bg, background).preserveAspect = false;
        bg.gameObject.AddComponent<CoverFit>().Init(background);
    }

    void BuildCharacter(RectTransform root)
    {
        var c = Place("Character", root, At.BC, new Vector2(0, 170), new Vector2(460, 800));
        Img(c, character);
    }

    void BuildTopBar(RectTransform s)
    {
        Pill(s, At.TR, new Vector2(536, 28), new Vector2(184, 52), gemIcon, "1,200");
        Pill(s, At.TR, new Vector2(324, 28), new Vector2(201, 52), coinIcon, "15,340");

        var bell = IconBtn(s, At.TR, new Vector2(250, 28), bellIcon);
        Badge(bell.GetComponent<RectTransform>());
        IconBtn(s, At.TR, new Vector2(158, 28), friendsIcon);
        IconBtn(s, At.TR, new Vector2(66, 28), settingsIcon);
    }

    void BuildLeftColumn(RectTransform s)
    {
        var prof = Place("Profile", s, At.TL, new Vector2(34, 143), new Vector2(430, 160));
        Panel(prof);
        Img(Place("Avatar", prof, At.TL, new Vector2(8, 8), new Vector2(120, 144)), avatar);
        Txt(Place("Name", prof, At.TL, new Vector2(150, 14), new Vector2(260, 36)), "FamaLine", 26, textMain, TextAlignmentOptions.Left, FontStyles.Bold);
        Txt(Place("Level", prof, At.TL, new Vector2(150, 56), new Vector2(120, 30)), "Lv. 12", 24, textMain, TextAlignmentOptions.Left, FontStyles.Bold);
        Img(Place("XPBg", prof, At.TL, new Vector2(230, 70), new Vector2(170, 10)), null, new Color(0.1f, 0.2f, 0.35f));
        Img(Place("XPFill", prof, At.TL, new Vector2(230, 70), new Vector2(170 * 0.36f, 10)), null, accent);
        Txt(Place("XP", prof, At.TL, new Vector2(230, 84), new Vector2(170, 20)), "EXP 720 / 2,000", 14, textDim, TextAlignmentOptions.Right);
        Img(Place("RankIcon", prof, At.TL, new Vector2(150, 108), new Vector2(40, 40)), rankIcon);
        Txt(Place("Rank", prof, At.TL, new Vector2(196, 108), new Vector2(200, 40)), "Silver III", 20, textMain);

        var ban = Place("AgentBanner", s, At.TL, new Vector2(34, 425), new Vector2(430, 172));
        Img(ban, agentBanner).preserveAspect = false;
        Btn(Place("GetNow", ban, At.BR, new Vector2(16, 16), new Vector2(160, 52)), "GET NOW »", null, null, true, 18);

        var notice = Place("Notice", s, At.TL, new Vector2(34, 618), new Vector2(430, 126));
        Panel(notice);
        Img(Place("NoticeIcon", notice, At.TL, new Vector2(22, 26), new Vector2(32, 32)), speakerIcon);
        Txt(Place("NoticeText", notice, At.TL, new Vector2(72, 22), new Vector2(330, 80)),
            "Maintenance will be on\nNov 10, 08:00 (UTC+7).", 20, textMain);
        Txt(Place("Arrow", notice, At.TR, new Vector2(18, 48), new Vector2(30, 30)), "›", 32, textMain, TextAlignmentOptions.Center);
    }

    void BuildRightColumn(RectTransform s)
    {
        var d = Place("DailyMission", s, At.TR, new Vector2(35, 207), new Vector2(375, 138));
        Panel(d);
        Txt(Place("DailyTitle", d, At.TL, new Vector2(20, 14), new Vector2(260, 30)), "DAILY MISSION", 20, textMain, TextAlignmentOptions.Left, FontStyles.Bold);
        Txt(Place("DailyProgress", d, At.TR, new Vector2(20, 14), new Vector2(120, 30)), "0 / 3", 20, textMain, TextAlignmentOptions.Right, FontStyles.Bold);
        Txt(Place("DailyDesc", d, At.TL, new Vector2(20, 52), new Vector2(330, 28)), "Play 3 matches", 18, textDim);
        Img(Place("RewardIcon", d, At.BR, new Vector2(70, 16), new Vector2(30, 30)), gemIcon);
        Txt(Place("RewardText", d, At.BR, new Vector2(20, 16), new Vector2(46, 30)), "x100", 20, textMain, TextAlignmentOptions.Right, FontStyles.Bold);

        var sr = Place("SeasonRank", s, At.TR, new Vector2(35, 367), new Vector2(375, 128));
        Panel(sr);
        Img(Place("SeasonIcon", sr, At.TL, new Vector2(14, 20), new Vector2(76, 76)), rankIcon);
        Txt(Place("SeasonLabel", sr, At.TL, new Vector2(100, 16), new Vector2(250, 24)), "SEASON RANK", 16, textDim, TextAlignmentOptions.Left, FontStyles.Bold);
        Txt(Place("SeasonTier", sr, At.TL, new Vector2(100, 38), new Vector2(250, 32)), "Bronze I", 26, textMain, TextAlignmentOptions.Left, FontStyles.Bold);
        Img(Place("SeasonBarBg", sr, At.TL, new Vector2(100, 82), new Vector2(180, 6)), null, new Color(0.1f, 0.2f, 0.35f));
        Img(Place("SeasonBarFill", sr, At.TL, new Vector2(100, 82), new Vector2(0, 6)), null, accent);
        Txt(Place("SeasonProg", sr, At.TL, new Vector2(100, 94), new Vector2(180, 22)), "0 / 1000", 15, textDim);

        var map = Place("MapCard", s, At.TR, new Vector2(35, 519), new Vector2(375, 239));
        Img(map, mapBanner).preserveAspect = false;
        var mapLbl = Place("MapLabelBg", map, At.BL, Vector2.zero, new Vector2(375, 84));
        Panel(mapLbl, new Color(0.02f, 0.05f, 0.12f, 0.9f));
        Txt(Place("NewMap", mapLbl, At.TL, new Vector2(16, 14), new Vector2(340, 24)), "NEW MAP", 18, accentBright, TextAlignmentOptions.Left, FontStyles.Bold);
        Txt(Place("MapName", mapLbl, At.TL, new Vector2(16, 38), new Vector2(340, 30)), "RESEARCH FACILITY »", 22, textMain, TextAlignmentOptions.Left, FontStyles.Bold);
    }

    void BuildBottomBar(RectTransform s)
    {
        string[] titles = { "OPERATIONS", "AGENTS", "ARMORY", "INVENTORY", "STORE", "SETTINGS" };
        string[] subs   = { "Play Missions", "Select Character", "Upgrade Weapons", "Manage Items", "Buy Items", "Game Settings" };

        // Ukuran lebih kecil dan jarak antar tombol lebih lebar
        const float navW = 180f, navH = 104f, gap = 26f, startX = 34f, navY = 47f;

        for (int i = 0; i < 6; i++)
        {
            var rt = Place("Nav_" + titles[i], s, At.BL, new Vector2(startX + i * (navW + gap), navY), new Vector2(navW, navH));
            Btn(rt, titles[i], subs[i], navIcons[i], i == 0, 17f, 12f);
            if (i == 0) Badge(rt);
        }

        var deploy = Place("Deploy", s, At.BR, new Vector2(31, 44), new Vector2(444, 163));
        Btn(deploy, "DEPLOY »", "ENTER THE BATTLEFIELD", null, true, 40, 18);
    }

    // ================= BUILDING BLOCKS =================

    // Tombol: SATU style untuk semua (clip path + border). Hanya "filled" yang berbeda.
    Button Btn(RectTransform rt, string title, string sub, Sprite icon, bool filled = false, float titleSize = 18f, float subSize = 13f)
    {
        var g = Shape(rt, filled ? accent : panelColor);
        g.raycastTarget = true;

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = g;
        var cb = btn.colors;
        cb.normalColor = g.color;
        cb.highlightedColor = Color.Lerp(g.color, accentBright, 0.45f);
        cb.pressedColor = Color.Lerp(g.color, Color.black, 0.25f);
        cb.selectedColor = cb.highlightedColor;
        cb.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        btn.colors = cb;

        bool iconOnly = string.IsNullOrEmpty(title);
        bool hasSub = !string.IsNullOrEmpty(sub);

        if (icon)
        {
            var ir = Pin("Icon", rt,
                iconOnly ? new Vector2(.5f, .5f) : new Vector2(.5f, .78f),
                new Vector2(.5f, .5f), Vector2.zero,
                iconOnly ? new Vector2(28, 28) : new Vector2(32, 32));
            Img(ir, icon);
        }

        if (!iconOnly)
        {
            var tr = Fill("Title", rt, 8, 0, 8, 0);
            tr.anchorMin = new Vector2(0, hasSub ? .36f : .15f);
            tr.anchorMax = new Vector2(1, hasSub ? .62f : .85f);
            Txt(tr, title, titleSize, textMain, TextAlignmentOptions.Center, FontStyles.Bold);
        }

        if (hasSub)
        {
            var sr = Fill("Sub", rt, 8, 0, 8, 0);
            sr.anchorMin = new Vector2(0, .10f);
            sr.anchorMax = new Vector2(1, .34f);
            Txt(sr, sub, subSize, textDim, TextAlignmentOptions.Center);
        }
        return btn;
    }

    Button IconBtn(RectTransform parent, At at, Vector2 p, Sprite icon)
    {
        var rt = Place("IconBtn", parent, at, p, new Vector2(56, 56));
        return Btn(rt, "", null, icon);
    }

    void Pill(RectTransform s, At at, Vector2 p, Vector2 size, Sprite icon, string value)
    {
        var r = Place("Pill", s, at, p, size);
        Panel(r);
        Img(Pin("Icon", r, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(12, 0), new Vector2(30, 30)), icon);
        var vr = Fill("Value", r, 50, 0, 50, 0);
        Txt(vr, value, 22, textMain, TextAlignmentOptions.Left, FontStyles.Bold);
        Btn(Pin("Plus", r, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(-6, 0), new Vector2(40, 40)), "+", null, null, false, 20);
    }

    void Badge(RectTransform host)
    {
        var b = Pin("Badge", host, new Vector2(1, 1), new Vector2(.5f, .5f), Vector2.zero, new Vector2(18, 18));
        Img(b, null, alert);
    }

    // Panel: clip path yang sama, tanpa interaksi
    ChamferedGraphic Panel(RectTransform rt, Color? tint = null)
    {
        var g = Shape(rt, tint ?? panelColor);
        g.raycastTarget = false;
        return g;
    }

    // Membuat bentuk clip path dengan parameter tema
    ChamferedGraphic Shape(RectTransform rt, Color fill)
    {
        var g = rt.gameObject.AddComponent<ChamferedGraphic>();
        g.color = fill;
        g.chamfer = chamfer;
        g.border = borderWidth;
        g.borderColor = borderColor;
        g.cutTL = true;
        g.cutBR = true;
        g.cutTR = false;
        g.cutBL = false;
        return g;
    }

    Image Img(RectTransform rt, Sprite s, Color? tint = null)
    {
        var i = rt.gameObject.AddComponent<Image>();
        i.sprite = s;
        i.preserveAspect = true;
        i.raycastTarget = false;
        i.color = tint ?? Color.white;
        i.enabled = s != null || tint.HasValue;
        return i;
    }

    TextMeshProUGUI Txt(RectTransform rt, string text, float size, Color c,
        TextAlignmentOptions align = TextAlignmentOptions.Left, FontStyles style = FontStyles.Normal)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = c;
        t.alignment = align;
        t.fontStyle = style;
        t.raycastTarget = false;
        return t;
    }

    // ---------- Layout primitives (anchor, bukan posisi absolut) ----------

    RectTransform Rect(string n, Transform parent)
    {
        var rt = new GameObject(n, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    RectTransform Pin(string n, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt = Rect(n, parent);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    RectTransform Place(string n, Transform parent, At at, Vector2 p, Vector2 size)
    {
        switch (at)
        {
            case At.TL: return Pin(n, parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(p.x, -p.y), size);
            case At.TR: return Pin(n, parent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-p.x, -p.y), size);
            case At.BL: return Pin(n, parent, Vector2.zero, Vector2.zero, p, size);
            case At.BR: return Pin(n, parent, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-p.x, p.y), size);
            default:    return Pin(n, parent, new Vector2(.5f, 0), new Vector2(.5f, 0), p, size);
        }
    }

    RectTransform Fill(string n, Transform parent, float l = 0, float b = 0, float r = 0, float t = 0)
    {
        var rt = Rect(n, parent);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
        return rt;
    }

    void ApplySafeArea(RectTransform rt)
    {
        var r = Screen.safeArea;
        rt.anchorMin = new Vector2(r.xMin / Screen.width, r.yMin / Screen.height);
        rt.anchorMax = new Vector2(r.xMax / Screen.width, r.yMax / Screen.height);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
