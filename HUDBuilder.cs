// Taruh di: Assets/Editor/HUDBuilder.cs
// Menu: Tools > HUD Builder > Build Combat HUD
// Reference: 1920x1080 (koordinat dikonversi dari screenshot 1672x941, skala 1.148)
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class HUDBuilder
{
    // ---------- Theme ----------
    static Sprite sRound, sCircle;
    static readonly Color Accent   = new Color32(46, 128, 255, 255);
    static readonly Color AccentLt = new Color32(130, 195, 255, 255);
    static readonly Color Panel    = new Color32(8, 18, 38, 175);
    static readonly Color Dark     = new Color32(10, 22, 44, 190);
    static readonly Color Gold     = new Color32(255, 196, 40, 255);
    static readonly Color Danger   = new Color32(235, 60, 60, 255);

    // ---------- Anchors ----------
    static readonly Vector2 TL = new Vector2(0, 1), TR = new Vector2(1, 1), BL = new Vector2(0, 0),
                            BR = new Vector2(1, 0), TC = new Vector2(.5f, 1), BC = new Vector2(.5f, 0),
                            C = new Vector2(.5f, .5f);

    [MenuItem("Tools/HUD Builder/Build Combat HUD")]
    public static void Build()
    {
        sRound  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sCircle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        var canvasGo = new GameObject("CombatHUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Build Combat HUD");
        canvasGo.layer = LayerMask.NameToLayer("UI");

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var hud = canvasGo.AddComponent<CombatHUD>();
        var root = Stretch("SafeArea", canvasGo.transform);
        root.gameObject.AddComponent<SafeAreaFitter>();

        BuildProfile(hud, root);
        BuildMission(hud, root);
        BuildCompass(hud, root);
        BuildTopRight(hud, root);
        BuildMinimap(hud, root);
        BuildControls(hud, root);
        BuildBottomBar(hud, root);
        BuildCrosshair(root);
        EnsureEventSystem();

        // nilai awal sesuai screenshot
        hud.SetXP(12, 720, 2000);
        hud.SetHealth(100);
        hud.SetArmor(100);
        hud.SetAmmo(30, 120);
        hud.SetSecondaryAmmo(15, 45);
        hud.SetGrenades(2);
        hud.SetMedkits(1);
        hud.SetCurrency(1200, 15340);
        hud.SetTimer(14 * 60 + 32);
        hud.SelectSlot(0);
        hud.SetHeading(330);

        Selection.activeGameObject = canvasGo;
        EditorUtility.SetDirty(canvasGo);
    }

    // =====================================================================
    //  SECTIONS
    // =====================================================================
    static void BuildProfile(CombatHUD hud, RectTransform root)
    {
        var prof = Fixed("PlayerProfile", root, TL, TL, new Vector2(29, -23), new Vector2(520, 170));

        var frame = Fixed("AvatarFrame", prof, TL, TL, Vector2.zero, new Vector2(132, 130));
        Img(frame, null, Accent);
        var av = R("Avatar", frame, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);
        av.offsetMin = new Vector2(3, 3); av.offsetMax = new Vector2(-3, -3);
        hud.avatar = Img(av, null, new Color(.12f, .16f, .26f, 1f), false);

        hud.playerName = Txt("PlayerName", prof, "FamaLine", 32, Color.white, TextAlignmentOptions.Left, TL, TL, new Vector2(153, -8), new Vector2(300, 40), FontStyles.Bold);
        hud.level      = Txt("Level", prof, "Lv. 12", 30, Color.white, TextAlignmentOptions.Left, TL, TL, new Vector2(153, -47), new Vector2(90, 40), FontStyles.Bold);

        hud.xpFill = Bar("XPBar", prof, TL, TL, new Vector2(240, -61), new Vector2(110, 8), Accent);
        hud.xpText = Txt("XPText", prof, "720 / 2,000", 22, new Color(.85f, .9f, 1f, 1), TextAlignmentOptions.Left, TL, TL, new Vector2(360, -47), new Vector2(160, 40));

        var rank = Fixed("RankIcon", prof, TL, TL, new Vector2(155, -92), new Vector2(50, 50));
        hud.rankIcon = Img(rank, sCircle, new Color(.7f, .75f, .85f, 1));
        hud.rankText = Txt("RankText", prof, "Silver III", 26, Color.white, TextAlignmentOptions.Left, TL, TL, new Vector2(218, -99), new Vector2(150, 36), FontStyles.Bold);
        var info = Fixed("InfoIcon", prof, TL, TL, new Vector2(345, -104), new Vector2(24, 24));
        Img(info, sCircle, new Color(1, 1, 1, .25f));
        Txt("i", info, "i", 16, Color.white, TextAlignmentOptions.Center, C, C, Vector2.zero, new Vector2(24, 24), FontStyles.Bold);
    }

    static void BuildMission(CombatHUD hud, RectTransform root)
    {
        var p = Fixed("MissionPanel", root, TL, TL, new Vector2(29, -184), new Vector2(360, 135));
        Img(p, sRound, Panel);
        var strip = Fixed("AccentStrip", p, TL, TL, Vector2.zero, new Vector2(4, 135)); Img(strip, null, Accent);

        var warn = Diamond("WarnIcon", p, TL, new Vector2(30, -24), 26, Gold);
        Txt("Bang", p, "!", 20, new Color(.1f, .1f, .1f, 1), TextAlignmentOptions.Center, TL, C, new Vector2(30, -24), new Vector2(26, 26), FontStyles.Bold);

        hud.missionTitle = Txt("Title", p, "Main Mission", 28, AccentLt, TextAlignmentOptions.Left, TL, TL, new Vector2(54, -6), new Vector2(290, 36), FontStyles.Bold);
        hud.objective    = Txt("Objective", p, "Infiltrate the Research Facility", 22, Color.white, TextAlignmentOptions.Left, TL, TL, new Vector2(44, -46), new Vector2(310, 30));
        Diamond("SubIcon", p, TL, new Vector2(64, -97), 14, Gold);
        hud.subObjective = Txt("SubObjective", p, "Reach the Control Room (0/1)", 22, Color.white, TextAlignmentOptions.Left, TL, TL, new Vector2(80, -82), new Vector2(275, 30));
    }

    static void BuildCompass(CombatHUD hud, RectTransform root)
    {
        var comp = Fixed("Compass", root, TC, TC, new Vector2(0, -24), new Vector2(760, 60));
        Img(comp, sRound, new Color(.03f, .08f, .18f, .45f));

        var mask = Stretch("Mask", comp);
        mask.gameObject.AddComponent<RectMask2D>();
        hud.compassStrip = Stretch("Strip", mask);

        var box = Fixed("HeadingBox", comp, C, C, Vector2.zero, new Vector2(96, 52));
        Img(box, sRound, new Color32(20, 70, 170, 235));
        var tip = Diamond("Notch", box, BC, new Vector2(0, 2), 14, new Color32(20, 70, 170, 235));
        hud.headingText = Txt("Heading", box, "330", 38, Color.white, TextAlignmentOptions.Center, C, C, Vector2.zero, new Vector2(96, 52), FontStyles.Bold);
    }

    static void BuildTopRight(CombatHUD hud, RectTransform root)
    {
        Pill("GemPill",  root, -421, 164, new Color32(80, 190, 255, 255), "1,200",  out hud.gemsText,  out hud.gemPlus);
        Pill("CoinPill", root, -230, 176, Gold,                           "15,340", out hud.coinsText, out hud.coinPlus);

        hud.notifButton = CircleBtn("NotifButton", root, TR, new Vector2(-142, -57), 62, "BELL", Color.clear, 15);
        var badge = Fixed("Badge", hud.notifButton.transform, TR, C, new Vector2(-6, -6), new Vector2(22, 22));
        Img(badge, sCircle, Danger);
        Txt("!", badge, "!", 15, Color.white, TextAlignmentOptions.Center, C, C, Vector2.zero, new Vector2(22, 22), FontStyles.Bold);

        hud.settingsButton = CircleBtn("SettingsButton", root, TR, new Vector2(-63, -57), 62, "SET", Color.clear, 15);
    }

    static void BuildMinimap(CombatHUD hud, RectTransform root)
    {
        var mm = Fixed("Minimap", root, TR, TR, new Vector2(-28, -94), new Vector2(245, 219));
        Img(mm, null, Accent);
        var view = R("MapView", mm, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);
        view.offsetMin = new Vector2(2, 2); view.offsetMax = new Vector2(-2, -2);
        var raw = view.gameObject.AddComponent<RawImage>();   // assign RenderTexture kamera minimap
        raw.color = new Color(.14f, .18f, .26f, 1f); raw.raycastTarget = false;
        hud.minimapView = raw;

        Txt("North", mm, "N", 20, Color.white, TextAlignmentOptions.Center, TL, C, new Vector2(123, -15), new Vector2(26, 26), FontStyles.Bold);
        Diamond("ObjectiveMarker", mm, TL, new Vector2(171, -52), 18, Gold);
        var arrow = Fixed("PlayerArrow", mm, TL, C, new Vector2(125, -110), new Vector2(22, 22));
        hud.minimapPlayer = arrow;
        Img(arrow, sCircle, AccentLt);

        var tm = Fixed("TimerBox", mm, BL, TL, new Vector2(0, -8), new Vector2(160, 36));
        Img(tm, sRound, Dark);
        var clk = Fixed("Clock", tm, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(8, 0), new Vector2(24, 24));
        Img(clk, sCircle, Color.white);
        hud.timerText = Txt("Time", tm, "14:32", 28, Color.white, TextAlignmentOptions.Left, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(46, 0), new Vector2(110, 36), FontStyles.Bold);
    }

    static void BuildControls(CombatHUD hud, RectTransform root)
    {
        // ---- kiri ----
        hud.reloadButton = CircleBtn("ReloadButton", root, BL, new Vector2(133, 550), 124, "RLD", AccentLt, 24);
        hud.sprintButton = CircleBtn("SprintButton", root, BL, new Vector2(282, 483), 101, "RUN", AccentLt, 22);

        var jb = Fixed("Joystick", root, BL, C, new Vector2(261, 279), new Vector2(257, 257));
        var bi = jb.gameObject.AddComponent<Image>(); bi.sprite = sCircle; bi.color = new Color(.05f, .1f, .2f, .45f);
        var ring = R("Ring", jb, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);
        ring.offsetMin = new Vector2(6, 6); ring.offsetMax = new Vector2(-6, -6);
        Img(ring, sCircle, new Color(1, 1, 1, .08f));
        Txt("Up",    jb, "^", 26, new Color(1, 1, 1, .6f), TextAlignmentOptions.Center, TC, TC, new Vector2(0, -6),  new Vector2(40, 40));
        Txt("Down",  jb, "v", 22, new Color(1, 1, 1, .6f), TextAlignmentOptions.Center, BC, BC, new Vector2(0, 6),   new Vector2(40, 40));
        Txt("Left",  jb, "<", 26, new Color(1, 1, 1, .6f), TextAlignmentOptions.Center, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(6, 0),  new Vector2(40, 40));
        Txt("Right", jb, ">", 26, new Color(1, 1, 1, .6f), TextAlignmentOptions.Center, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(-6, 0), new Vector2(40, 40));
        var knob = Fixed("Knob", jb, C, C, Vector2.zero, new Vector2(103, 103));
        Img(knob, sCircle, new Color(.82f, .87f, .95f, .85f));
        var js = jb.gameObject.AddComponent<VirtualJoystick>();
        js.knob = knob; js.radius = 257f * 0.35f;
        hud.joystick = js;

        // ---- kanan: skill Q/E/R ----
        hud.skillQ = CircleBtn("SkillQ", root, TR, new Vector2(-71, -432), 87, "ATK",  Accent, 20, "Q");
        hud.skillE = CircleBtn("SkillE", root, TR, new Vector2(-71, -528), 87, "HEAL", AccentLt, 20, "E");
        hud.skillR = CircleBtn("SkillR", root, TR, new Vector2(-71, -626), 87, "DASH", Accent, 20, "R");

        // ---- kanan: aim, fire, crouch, prone ----
        hud.aimButton    = CircleBtn("AimButton",    root, TR, new Vector2(-186, -667), 100, "AIM",  AccentLt, 22);
        hud.fireButton   = CircleBtn("FireButton",   root, BR, new Vector2(-316, 267),  186, "FIRE", AccentLt, 34);
        hud.crouchButton = CircleBtn("CrouchButton", root, BR, new Vector2(-275, 95),   112, "SIT",  AccentLt, 22);
        hud.proneButton  = CircleBtn("ProneButton",  root, BR, new Vector2(-113, 107),  110, "PRN",  AccentLt, 22);
    }

    static void BuildBottomBar(CombatHUD hud, RectTransform root)
    {
        var bar = Stretch("BottomBar", root);
        var bg = new Image[4];
        var ol = new Outline[4];

        // slot 0: senjata utama
        var s0 = Slot("Slot_Primary", bar, -282, new Vector2(276, 86), "MP5", out hud.primaryAmmo, TextAlignmentOptions.Left, new Vector2(18, 6), 40, out bg[0], out ol[0]);
        var s1 = Slot("Slot_Secondary", bar, 8, new Vector2(188, 80), "PISTOL", out hud.secondaryAmmo, TextAlignmentOptions.Left, new Vector2(16, 6), 30, out bg[1], out ol[1]);
        var s2 = Slot("Slot_Grenade", bar, 211, new Vector2(103, 75), "GRN", out hud.grenadeText, TextAlignmentOptions.Right, new Vector2(-10, 4), 28, out bg[2], out ol[2]);
        var s3 = Slot("Slot_Medkit", bar, 329, new Vector2(103, 75), "MED", out hud.medkitText, TextAlignmentOptions.Right, new Vector2(-10, 4), 28, out bg[3], out ol[3]);
        hud.slotBackgrounds = bg; hud.slotOutlines = ol;
        hud.grenadeButton = s2.gameObject.AddComponent<Button>(); hud.grenadeButton.targetGraphic = bg[2];
        hud.medkitButton  = s3.gameObject.AddComponent<Button>(); hud.medkitButton.targetGraphic = bg[3];
        hud.primaryButton = s0.gameObject.AddComponent<Button>(); hud.primaryButton.targetGraphic = bg[0];
        hud.secondaryButton = s1.gameObject.AddComponent<Button>(); hud.secondaryButton.targetGraphic = bg[1];
        foreach (var im in bg) im.raycastTarget = true;

        // HP & Armor
        Txt("HPIcon", bar, "+", 40, Color.white, TextAlignmentOptions.Center, BC, C, new Vector2(-361, 78), new Vector2(40, 40), FontStyles.Bold);
        hud.hpText = Txt("HPText", bar, "HP 100", 28, Color.white, TextAlignmentOptions.Left, BC, new Vector2(0, .5f), new Vector2(-336, 78), new Vector2(100, 34), FontStyles.Bold);
        hud.hpFill = Bar("HPBar", bar, BC, BL, new Vector2(-235, 71), new Vector2(331, 14), new Color32(235, 242, 255, 255));

        var sh = Fixed("ArmorIcon", bar, BC, C, new Vector2(-361, 46), new Vector2(20, 22)); Img(sh, sCircle, new Color32(235, 242, 255, 255));
        hud.armorText = Txt("ArmorText", bar, "Armor 100", 22, Accent, TextAlignmentOptions.Left, BC, new Vector2(0, .5f), new Vector2(-336, 46), new Vector2(110, 30), FontStyles.Bold);
        hud.armorFill = Bar("ArmorBar", bar, BC, BL, new Vector2(-235, 39), new Vector2(331, 14), Accent);
    }

    static void BuildCrosshair(RectTransform root)
    {
        var ch = Fixed("Crosshair", root, C, C, Vector2.zero, new Vector2(80, 80));
        Color red = new Color(1f, .22f, .22f, .95f);
        Img(Fixed("Dot",   ch, C, C, Vector2.zero,           new Vector2(4, 4)),   sCircle, red);
        Img(Fixed("Up",    ch, C, C, new Vector2(0, 24),     new Vector2(3, 20)),  null, red);
        Img(Fixed("Down",  ch, C, C, new Vector2(0, -24),    new Vector2(3, 20)),  null, red);
        Img(Fixed("Left",  ch, C, C, new Vector2(-24, 0),    new Vector2(20, 3)),  null, red);
        Img(Fixed("Right", ch, C, C, new Vector2(24, 0),     new Vector2(20, 3)),  null, red);
    }

    // =====================================================================
    //  WIDGET HELPERS
    // =====================================================================
    static void Pill(string name, RectTransform p, float rightX, float width, Color iconCol, string value,
                     out TextMeshProUGUI txt, out Button plus)
    {
        var rt = Fixed(name, p, TR, TR, new Vector2(rightX, -34), new Vector2(width, 46));
        Img(rt, sRound, Dark);
        var ic = Fixed("Icon", rt, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(8, 0), new Vector2(32, 32));
        Img(ic, sCircle, iconCol);
        txt = Txt("Value", rt, value, 26, Color.white, TextAlignmentOptions.Right, new Vector2(0, .5f), new Vector2(0, .5f),
                  new Vector2(46, 0), new Vector2(width - 46 - 46, 46), FontStyles.Bold);

        var pr = Fixed("Plus", rt, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(-6, 0), new Vector2(34, 34));
        var pi = Img(pr, sCircle, Accent); pi.raycastTarget = true;
        plus = pr.gameObject.AddComponent<Button>(); plus.targetGraphic = pi;
        Txt("+", pr, "+", 28, Color.white, TextAlignmentOptions.Center, C, C, Vector2.zero, new Vector2(34, 34), FontStyles.Bold);
    }

    static RectTransform Slot(string name, RectTransform p, float x, Vector2 size, string weaponName,
                              out TextMeshProUGUI ammo, TextAlignmentOptions ammoAlign, Vector2 ammoPos, float ammoSize,
                              out Image bg, out Outline outline)
    {
        var rt = Fixed(name, p, BC, BL, new Vector2(x, 105), size);
        bg = Img(rt, sRound, Panel);
        outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = Accent; outline.effectDistance = new Vector2(2, 2);

        var icon = R("WeaponIcon", rt, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);
        icon.offsetMin = new Vector2(size.x * .12f, size.y * .38f); icon.offsetMax = new Vector2(-size.x * .12f, -6);
        var ic = Img(icon, null, Color.white, false); ic.enabled = false; ic.preserveAspect = true;  // assign sprite senjata

        Txt("WeaponName", rt, weaponName, 22, new Color(.85f, .9f, 1f, .9f), TextAlignmentOptions.Center, TC, TC, new Vector2(0, -8), new Vector2(size.x, 30), FontStyles.Bold);
        Vector2 anc = ammoAlign == TextAlignmentOptions.Right ? BR : BL;
        Vector2 piv = ammoAlign == TextAlignmentOptions.Right ? BR : BL;
        ammo = Txt("Ammo", rt, "0", ammoSize, Color.white, ammoAlign, anc, piv, ammoPos, new Vector2(size.x - 20, ammoSize + 8), FontStyles.Bold);
        ammo.richText = true;
        return rt;
    }

    static Button CircleBtn(string name, Transform p, Vector2 anchor, Vector2 pos, float size, string label,
                            Color ring, float fontSize = 22, string key = null)
    {
        var rt = Fixed(name, p, anchor, C, pos, new Vector2(size, size));
        var ringImg = rt.gameObject.AddComponent<Image>();
        ringImg.sprite = sCircle; ringImg.color = ring.a == 0 ? new Color(1, 1, 1, .01f) : ring;
        var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = ringImg;

        var inner = R("Fill", rt, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);
        inner.offsetMin = new Vector2(3, 3); inner.offsetMax = new Vector2(-3, -3);
        Img(inner, sCircle, new Color(.04f, .09f, .18f, .8f));

        var icon = R("Icon", inner, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);
        icon.offsetMin = new Vector2(size * .22f, size * .22f); icon.offsetMax = -icon.offsetMin;
        var ic = Img(icon, null, Color.white, false); ic.enabled = false; ic.preserveAspect = true;   // assign sprite ikon

        Txt("Label", inner, label, fontSize, Color.white, TextAlignmentOptions.Center, C, C, Vector2.zero, new Vector2(size, size), FontStyles.Bold);

        if (key != null)
        {
            var kb = Fixed("KeyBadge", rt, BR, C, new Vector2(-size * .12f, size * .12f), new Vector2(28, 28));
            Img(kb, sCircle, Accent);
            Txt("Key", kb, key, 17, Color.white, TextAlignmentOptions.Center, C, C, Vector2.zero, new Vector2(28, 28), FontStyles.Bold);
        }
        return btn;
    }

    // Bar: background + Fill (anchorMax.x dikontrol CombatHUD)
    static RectTransform Bar(string name, Transform p, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color fill)
    {
        var bg = Fixed(name, p, anchor, pivot, pos, size);
        Img(bg, sRound, new Color(1, 1, 1, .2f));
        var f = R("Fill", bg, Vector2.zero, Vector2.one, new Vector2(0, .5f), Vector2.zero, Vector2.zero);
        Img(f, sRound, fill);
        return f;
    }

    static RectTransform Diamond(string name, Transform p, Vector2 anchor, Vector2 pos, float size, Color c)
    {
        var rt = Fixed(name, p, anchor, C, pos, new Vector2(size, size));
        rt.localEulerAngles = new Vector3(0, 0, 45);
        Img(rt, null, c);
        return rt;
    }

    // =====================================================================
    //  LOW LEVEL
    // =====================================================================
    static RectTransform R(string n, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return rt;
    }

    static RectTransform Fixed(string n, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        => R(n, parent, anchor, anchor, pivot, pos, size);

    static RectTransform Stretch(string n, Transform parent)
        => R(n, parent, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);

    static Image Img(RectTransform rt, Sprite s, Color c, bool sliced = true)
    {
        var i = rt.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c;
        i.type = (s != null && sliced && s == sRound) ? Image.Type.Sliced : Image.Type.Simple;
        i.raycastTarget = false;
        return i;
    }

    static TextMeshProUGUI Txt(string name, Transform p, string text, float size, Color col, TextAlignmentOptions al,
                               Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 sz, FontStyles fs = FontStyles.Normal)
    {
        var rt = Fixed(name, p, anchor, pivot, pos, sz);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = col; t.alignment = al; t.fontStyle = fs;
        t.raycastTarget = false; t.overflowMode = TextOverflowModes.Overflow;
        return t;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }
}
