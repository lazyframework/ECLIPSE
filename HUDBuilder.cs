// HUDBuilder.cs  -  Unity 2021.3+ (uGUI + TextMeshPro)
// MODE EDITOR (disarankan): HUD dibangun & tersimpan permanen di scene.
//  1. Window > TextMeshPro > Import TMP Essential Resources (sekali saja).
//  2. Menu: GameObject > UI > Project Eclipse HUD   (atau Add Component HUDBuilder lalu klik "Build / Rebuild HUD (Editor)").
//  3. Sprite prosedural otomatis disimpan ke Assets/HUD_Generated/*.png.
//  4. Isi slot Sprite ikon di Inspector lalu klik Rebuild. Kalau kosong, dipakai placeholder teks.
//  5. Simpan scene (Ctrl+S). HUD sekarang bisa diedit manual di Scene/Hierarchy.
// MODE RUNTIME: centang buildOnAwake, tekan Play.
//
// Semua posisi diambil dari screenshot referensi 1672 x 941 (anchor normalized),
// jadi layout identik di setiap resolusi 16:9.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HUDBuilder : MonoBehaviour
{
    // ---------- Referensi layout (ukuran screenshot) ----------
    const float W = 1672f, H = 941f;
    const float FS = 1080f / 941f;              // konversi ukuran font piksel-screenshot -> canvas 1920x1080

    [Header("Build")]
    public bool buildOnAwake = true;

    [Header("Data awal")]
    public string playerName = "FamaLine";
    public int level = 12, xp = 720, xpMax = 2000;
    public string rankName = "Silver III";
    public int gems = 1200, coins = 15340;
    public string missionTitle = "Main Mission";
    public string missionObjective1 = "Infiltrate the Research Facility";
    public string missionObjective2 = "Reach the Control Room (0/1)";
    public int hp = 100, armor = 100;
    public int ammoMag = 30, ammoReserve = 120, sideMag = 15, sideReserve = 45;
    public int grenades = 2, medkits = 1;
    [Range(0, 360)] public float heading = 330f;
    public int timerSeconds = 14 * 60 + 32;

    [Header("Sprite opsional (kosong = placeholder)")]
    public Sprite avatar, rankIcon, gemIcon, coinIcon, bellIcon, gearIcon;
    public Sprite rifleIcon, pistolIcon, grenadeIcon, medkitIcon;
    public Sprite fireIcon, reloadIcon, sprintIcon, scopeIcon, crouchIcon, proneIcon;
    public Sprite skillQIcon, skillEIcon, skillRIcon;
    public Texture minimapTexture;              // isi dengan RenderTexture kamera minimap

    // Event tombol: id = "Fire", "Reload", "Sprint", "Scope", "Crouch", "Prone", "SkillQ", "SkillE", "SkillR", "Settings", "Notif", "Weapon1", ...
    public Action<string> OnButton;
    [Serializable] public class StringEvent : UnityEngine.Events.UnityEvent<string> { }
    public StringEvent onButtonEvent;               // bisa dihubungkan dari Inspector
    [SerializeField, HideInInspector] VirtualJoystick joystick;
    public VirtualJoystick Joystick
    {
        get { if (joystick == null) joystick = GetComponentInChildren<VirtualJoystick>(true); return joystick; }
    }
    public void Fire(string id) { OnButton?.Invoke(id); onButtonEvent?.Invoke(id); }

    // ---------- Warna ----------
    static readonly Color PanelBg = new Color32(8, 16, 34, 190);
    static readonly Color Cyan    = new Color32(64, 180, 255, 255);
    static readonly Color Yellow  = new Color32(255, 200, 40, 255);
    static readonly Color White   = Color.white;
    static readonly Color Dim     = new Color32(170, 185, 210, 255);
    static readonly Color BtnBg   = new Color32(10, 20, 40, 170);
    static readonly Color BtnRing = new Color32(200, 220, 255, 150);

    // ---------- Referensi untuk update runtime ----------
    [SerializeField, HideInInspector] RectTransform root, hpFill, armorFill, xpFill;
    [SerializeField, HideInInspector] TMP_Text hpTxt, armorTxt, xpTxt, ammoTxt, sideTxt, grenadeTxt, medkitTxt, timerTxt, gemTxt, coinTxt, headingTxt;
    [SerializeField, HideInInspector] TMP_Text[] compassSlots = new TMP_Text[11];
    readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    Sprite whiteSprite;

    void Awake() { if (buildOnAwake && root == null) Build(); }

    static void Kill(GameObject o)
    {
        if (Application.isPlaying) { o.SetActive(false); Destroy(o); return; }
#if UNITY_EDITOR
        UnityEditor.Undo.DestroyObjectImmediate(o);
#else
        DestroyImmediate(o);
#endif
    }

    [ContextMenu("Clear HUD")]
    public void Clear()
    {
        var old = transform.Find("HUD_Canvas");
        if (old != null) Kill(old.gameObject);
        root = null;
        MarkDirty();
    }

    void MarkDirty()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    [ContextMenu("Rebuild HUD")]
    public void Build()
    {
        Clear();
        spriteCache.Clear();
        whiteSprite = MakeSprite("white", 4, t => 1f);

        // ----- Canvas -----
        var cgo = new GameObject("HUD_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        cgo.transform.SetParent(transform, false);
        var canvas = cgo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
#endif
        }

        root = R("HUD_Root", cgo.transform, 0, 0, W, H);

        BuildProfile();
        BuildMission();
        BuildCompass();
        BuildTopRight();
        BuildMinimap();
        BuildCrosshair();
        BuildLeftControls();
        BuildRightControls();
        BuildWeaponBar();
        BuildVitals();

        SetHeading(heading);
        SetTimer(timerSeconds);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.RegisterCreatedObjectUndo(cgo, "Build HUD");
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
        MarkDirty();
    }

    // =====================================================================
    //  SECTIONS
    // =====================================================================
    void BuildProfile()
    {
        var g = R("Profile", root, 0, 0, W, H);
        Img("PanelBG", g, 25, 20, 340, 150, new Color32(8, 16, 34, 120));

        var frame = Img("AvatarFrame", g, 25, 20, 140, 132, Cyan);
        var av = Img("Avatar", g, 28, 23, 137, 129, new Color32(30, 40, 60, 255), avatar);
        if (avatar == null) Txt("AvatarLbl", g, 28, 23, 137, 129, "AVATAR", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Center);

        Txt("Name", g, 158, 30, 320, 62, playerName, 24, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        Txt("Level", g, 158, 64, 225, 92, "Lv. " + level, 22, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        Bar("XPBar", g, 232, 74, 306, 85, Cyan, new Color32(255, 255, 255, 40), out xpFill);
        xpTxt = Txt("XPText", g, 311, 68, 400, 92, "", 15, Dim, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        SetXP(xp, xpMax);

        IconOrLabel("RankIcon", g, rankIcon, "◆", 160, 98, 206, 150, Dim);
        Txt("Rank", g, 215, 112, 290, 140, rankName, 18, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        var info = Img("InfoIcon", g, 295, 116, 315, 136, Color.clear, Circle(false));
        Txt("InfoLbl", g, 295, 116, 315, 136, "i", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
    }

    void BuildMission()
    {
        var g = R("Mission", root, 0, 0, W, H);
        Img("PanelBG", g, 25, 160, 340, 278, PanelBg);
        Img("AccentLine", g, 25, 160, 28, 278, Cyan);

        Img("WarnIcon", g, 35, 169, 65, 197, Yellow, Triangle());
        Txt("WarnMark", g, 35, 175, 65, 199, "!", 18, new Color32(20, 20, 20, 255), FontStyles.Bold, TextAlignmentOptions.Center);
        Txt("Title", g, 72, 168, 220, 198, missionTitle, 22, Cyan, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        Txt("Obj1", g, 72, 202, 330, 228, missionObjective1, 18, White, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        var d = Img("Diamond", g, 74, 238, 88, 252, Color.clear);
        d.gameObject.AddComponent<Outline>().effectColor = Yellow;
        d.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        Txt("Obj2", g, 98, 232, 338, 260, missionObjective2, 18, White, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
    }

    void BuildCompass()
    {
        var g = R("Compass", root, 0, 0, W, H);
        Img("StripBG", g, 525, 22, 1150, 72, new Color32(8, 16, 34, 90));

        for (int i = 0; i < 11; i++)
        {
            float cx = 836f + (i - 5) * 54.4f;
            compassSlots[i] = Txt("Slot" + i, g, cx - 27, 30, cx + 27, 64, "", 20, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
        }
        compassSlots[5].gameObject.SetActive(false);

        var box = Img("CenterBox", g, 812, 28, 860, 64, new Color32(14, 60, 130, 235));
        box.gameObject.AddComponent<Outline>().effectColor = Cyan;
        headingTxt = Txt("Heading", g, 806, 28, 866, 64, "330", 26, White, FontStyles.Bold, TextAlignmentOptions.Center);
        var tri = Img("Pointer", g, 828, 64, 844, 74, Cyan, Triangle());
        tri.rectTransform.localRotation = Quaternion.Euler(0, 0, 180);
    }

    void BuildTopRight()
    {
        var g = R("TopRight", root, 0, 0, W, H);

        Img("GemPill", g, 1162, 27, 1306, 63, PanelBg, Pill());
        IconOrLabel("GemIcon", g, gemIcon, "◆", 1170, 30, 1202, 62, Cyan);
        gemTxt = Txt("GemTxt", g, 1210, 28, 1280, 62, gems.ToString("N0"), 22, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        Btn("GemPlus", g, null, "+", 1278, 30, 1302, 60, Color.clear, Color.clear, 26);

        Img("CoinPill", g, 1320, 27, 1470, 63, PanelBg, Pill());
        IconOrLabel("CoinIcon", g, coinIcon, "●", 1325, 28, 1359, 62, Yellow);
        coinTxt = Txt("CoinTxt", g, 1366, 28, 1442, 62, coins.ToString("N0"), 22, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        Btn("CoinPlus", g, null, "+", 1442, 30, 1466, 60, Color.clear, Color.clear, 26);

        Btn("Notif", g, bellIcon, "🔔", 1508, 25, 1546, 65, Color.clear, Color.clear, 22);
        var badge = Img("Badge", g, 1534, 16, 1556, 38, new Color32(230, 40, 40, 255), Circle(false));
        Txt("BadgeTxt", g, 1534, 16, 1556, 38, "!", 14, White, FontStyles.Bold, TextAlignmentOptions.Center);

        Btn("Settings", g, gearIcon, "⚙", 1590, 24, 1634, 68, Color.clear, Color.clear, 30);
    }

    void BuildMinimap()
    {
        var g = R("Minimap", root, 0, 0, W, H);
        var frame = Img("Frame", g, 1435, 82, 1647, 273, new Color32(120, 150, 200, 255));
        if (minimapTexture != null)
        {
            var rr = R("Map", g, 1437, 84, 1645, 271);
            var raw = rr.gameObject.AddComponent<RawImage>(); raw.texture = minimapTexture; raw.raycastTarget = false;
        }
        else Img("MapBG", g, 1437, 84, 1645, 271, new Color32(26, 34, 52, 255));

        Txt("North", g, 1525, 84, 1560, 106, "N", 16, White, FontStyles.Bold, TextAlignmentOptions.Center);
        var obj = Img("Objective", g, 1577, 120, 1591, 134, Yellow);
        obj.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        Img("Player", g, 1533, 165, 1555, 191, Cyan, Triangle());

        Img("TimerBG", g, 1436, 280, 1548, 310, PanelBG_Alpha(160));
        Img("ClockIcon", g, 1444, 283, 1470, 308, Color.clear, Circle(true));
        timerTxt = Txt("Timer", g, 1478, 280, 1548, 310, "14:32", 22, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
    }

    void BuildCrosshair()
    {
        var g = R("Crosshair", root, 0, 0, W, H);
        Color red = new Color32(255, 60, 60, 255);
        Img("H1", g, 800, 469, 826, 471, red);
        Img("H2", g, 846, 469, 872, 471, red);
        Img("V1", g, 835, 437, 837, 463, red);
        Img("V2", g, 835, 478, 837, 504, red);
        Img("Dot", g, 833, 467, 839, 473, red, Circle(false));
        // enemy health bar
        Img("EnemyBarBG", g, 935, 463, 1010, 470, new Color32(60, 10, 10, 200));
        Img("EnemyBar", g, 935, 463, 1008, 470, new Color32(230, 40, 40, 255));
    }

    void BuildLeftControls()
    {
        var g = R("LeftControls", root, 0, 0, W, H);
        Btn("Reload", g, reloadIcon, "R▮", 62, 410, 172, 515, BtnBg, BtnRing, 24);
        Btn("Sprint", g, sprintIcon, "RUN", 202, 466, 290, 565, BtnBg, BtnRing, 18);

        // Joystick
        var baseImg = Img("JoystickBase", g, 113, 588, 340, 810, new Color32(10, 20, 40, 110), Circle(false));
        var ring = Img("JoystickRing", g, 113, 588, 340, 810, BtnRing, Circle(true)); ring.raycastTarget = false;
        var up = Img("ArrowUp", g, 218, 600, 238, 616, Dim, Triangle()); up.raycastTarget = false;
        var dn = Img("ArrowDown", g, 218, 780, 238, 796, Dim, Triangle()); dn.rectTransform.localRotation = Quaternion.Euler(0, 0, 180); dn.raycastTarget = false;
        var lf = Img("ArrowLeft", g, 130, 692, 146, 712, Dim, Triangle()); lf.rectTransform.localRotation = Quaternion.Euler(0, 0, 90); lf.raycastTarget = false;
        var rt = Img("ArrowRight", g, 308, 692, 324, 712, Dim, Triangle()); rt.rectTransform.localRotation = Quaternion.Euler(0, 0, -90); rt.raycastTarget = false;
        var knob = Img("JoystickKnob", g, 182, 655, 272, 745, new Color32(190, 200, 215, 230), Circle(false)); knob.raycastTarget = false;

        joystick = baseImg.gameObject.AddComponent<VirtualJoystick>();
        joystick.Setup(baseImg.rectTransform, knob.rectTransform);
    }

    void BuildRightControls()
    {
        var g = R("RightControls", root, 0, 0, W, H);

        // Skill Q / E / R
        SkillBtn(g, "SkillQ", skillQIcon, "Q", 1573, 338, 1651, 416, new Color32(40, 120, 255, 255));
        SkillBtn(g, "SkillE", skillEIcon, "E", 1573, 424, 1651, 502, Cyan);
        SkillBtn(g, "SkillR", skillRIcon, "R", 1573, 510, 1651, 588, Cyan);

        Btn("Scope", g, scopeIcon, "◎", 1466, 540, 1556, 630, BtnBg, BtnRing, 40);
        Btn("Fire", g, fireIcon, "FIRE", 1316, 627, 1478, 790, BtnBg, BtnRing, 28);
        Btn("Crouch", g, crouchIcon, "CROUCH", 1384, 810, 1480, 908, BtnBg, BtnRing, 14);
        Btn("Prone", g, proneIcon, "PRONE", 1524, 798, 1624, 898, BtnBg, BtnRing, 14);
    }

    void BuildWeaponBar()
    {
        var g = R("WeaponBar", root, 0, 0, W, H);

        // Slot 1 (aktif)
        var s1 = Img("Slot1", g, 588, 773, 830, 850, new Color32(14, 50, 110, 235));
        s1.gameObject.AddComponent<Outline>().effectColor = Cyan;
        Hook(s1, "Weapon1");
        IconOrLabel("Rifle", g, rifleIcon, "ASSAULT RIFLE", 632, 783, 792, 835, White);
        ammoTxt = Txt("Ammo", g, 612, 808, 760, 852, "", 30, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        // Slot 2
        var s2 = Img("Slot2", g, 842, 780, 1007, 850, new Color32(20, 30, 50, 200));
        Hook(s2, "Weapon2");
        IconOrLabel("Pistol", g, pistolIcon, "PISTOL", 888, 788, 956, 830, Dim);
        sideTxt = Txt("SideAmmo", g, 852, 818, 1000, 850, "", 22, Dim, FontStyles.Normal, TextAlignmentOptions.Center);

        // Grenade
        var s3 = Img("Slot3", g, 1020, 785, 1112, 850, new Color32(20, 30, 50, 200));
        Hook(s3, "Grenade");
        IconOrLabel("Grenade", g, grenadeIcon, "●", 1042, 796, 1086, 840, Dim);
        grenadeTxt = Txt("GrenadeCnt", g, 1086, 815, 1108, 848, "", 18, White, FontStyles.Bold, TextAlignmentOptions.Center);

        // Medkit
        var s4 = Img("Slot4", g, 1124, 785, 1213, 850, new Color32(20, 30, 50, 200));
        Hook(s4, "Medkit");
        IconOrLabel("Medkit", g, medkitIcon, "✚", 1146, 796, 1190, 840, White);
        medkitTxt = Txt("MedkitCnt", g, 1188, 815, 1210, 848, "", 18, White, FontStyles.Bold, TextAlignmentOptions.Center);

        SetAmmo(ammoMag, ammoReserve);
        SetSideAmmo(sideMag, sideReserve);
        SetGrenades(grenades);
        SetMedkits(medkits);
    }

    void BuildVitals()
    {
        var g = R("Vitals", root, 0, 0, W, H);
        Img("BG", g, 505, 852, 1220, 924, new Color32(8, 16, 34, 90));

        Txt("HPCross", g, 508, 860, 538, 888, "✚", 22, White, FontStyles.Bold, TextAlignmentOptions.Center);
        hpTxt = Txt("HPTxt", g, 542, 858, 628, 888, "", 24, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        Bar("HPBar", g, 631, 866, 920, 880, new Color32(235, 240, 250, 255), new Color32(255, 255, 255, 40), out hpFill);

        Txt("ArmorIcon", g, 508, 890, 538, 916, "⛨", 20, Cyan, FontStyles.Bold, TextAlignmentOptions.Center);
        armorTxt = Txt("ArmorTxt", g, 540, 890, 628, 916, "", 19, Cyan, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        Bar("ArmorBar", g, 631, 896, 920, 909, Cyan, new Color32(255, 255, 255, 40), out armorFill);

        SetHP(hp); SetArmor(armor);
    }

    // =====================================================================
    //  PUBLIC API (panggil dari gameplay)
    // =====================================================================
    public void SetHP(int v)     { hp = Mathf.Clamp(v, 0, 100);    hpTxt.text = "HP " + hp;          SetFill(hpFill, hp / 100f); }
    public void SetArmor(int v)  { armor = Mathf.Clamp(v, 0, 100); armorTxt.text = "Armor " + armor; SetFill(armorFill, armor / 100f); }
    public void SetXP(int cur, int max) { xp = cur; xpMax = max; xpTxt.text = cur.ToString("N0") + " / " + max.ToString("N0"); SetFill(xpFill, max > 0 ? (float)cur / max : 0); }
    public void SetAmmo(int mag, int reserve)     { ammoTxt.text = mag + "<size=60%><color=#AAB8D0> / " + reserve + "</color></size>"; }
    public void SetSideAmmo(int mag, int reserve) { sideTxt.text = mag + " / " + reserve; }
    public void SetGrenades(int n) { grenadeTxt.text = n.ToString(); }
    public void SetMedkits(int n)  { medkitTxt.text = n.ToString(); }
    public void SetCurrency(int g, int c) { gemTxt.text = g.ToString("N0"); coinTxt.text = c.ToString("N0"); }
    public void SetTimer(int sec) { timerTxt.text = (sec / 60).ToString("00") + ":" + (sec % 60).ToString("00"); }

    public void SetHeading(float deg)
    {
        heading = (deg % 360f + 360f) % 360f;
        headingTxt.text = Mathf.RoundToInt(heading).ToString();
        string[] card = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        for (int i = 0; i < 11; i++)
        {
            if (i == 5) continue;
            int v = Mathf.RoundToInt(((heading + (i - 5) * 15f) % 360f + 360f) % 360f / 15f) * 15 % 360;
            compassSlots[i].text = (v % 45 == 0) ? card[v / 45] : v.ToString();
            compassSlots[i].color = (v % 45 == 0) ? White : Dim;
        }
    }

    // =====================================================================
    //  HELPERS
    // =====================================================================
    void Hook(Image img, string id)
    {
        img.raycastTarget = true;
        var hb = img.gameObject.AddComponent<HUDButton>();
        hb.id = id; hb.owner = this;
    }

    static void SetFill(RectTransform fill, float v) { fill.anchorMax = new Vector2(Mathf.Clamp01(v), 1); }
    static Color PanelBg_Alpha(byte a) { return new Color32(8, 16, 34, a); }

    // Rect dari koordinat piksel screenshot (origin kiri-atas) -> anchor normalized
    RectTransform R(string n, Transform p, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(n, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(p, false);
        rt.anchorMin = new Vector2(x0 / W, 1f - y1 / H);
