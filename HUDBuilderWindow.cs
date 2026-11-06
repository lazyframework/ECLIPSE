#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class HUDBuilderWindow : EditorWindow
{
    private Canvas targetCanvas;
    private bool clearExisting = true;
    private TMP_FontAsset font;

    // Warna yang lebih dekat dengan gambar
    private readonly Color panelDark = new Color(0.07f, 0.11f, 0.17f, 0.92f);
    private readonly Color blueAccent = new Color(0.22f, 0.55f, 0.95f);
    private readonly Color cyan = new Color(0.25f, 0.85f, 1f);
    private readonly Color gold = new Color(1f, 0.82f, 0.25f);
    private readonly Color hpGreen = new Color(0.25f, 0.95f, 0.45f);
    private readonly Color armorBlue = new Color(0.35f, 0.65f, 1f);

    [MenuItem("Tools/HUD Builder (Accurate)")]
    public static void Open() => GetWindow<HUDBuilderWindow>("HUD Builder");

    void OnGUI()
    {
        GUILayout.Space(8);
        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
        clearExisting = EditorGUILayout.Toggle("Clear Existing", clearExisting);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font", font, typeof(TMP_FontAsset), false);

        GUILayout.Space(12);
        GUI.backgroundColor = new Color(0.2f, 0.75f, 0.4f);
        if (GUILayout.Button("BUILD HUD (Mirip Gambar)", GUILayout.Height(42)))
        {
            if (targetCanvas == null)
            {
                EditorUtility.DisplayDialog("Error", "Pilih Canvas dulu!", "OK");
                return;
            }
            Build();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox("Hasil akan mirip screenshot. Gunakan Canvas Scaler: Scale With Screen Size 1920x1080", MessageType.Info);
    }

    void Build()
    {
        if (clearExisting)
        {
            while (targetCanvas.transform.childCount > 0)
                Undo.DestroyObjectImmediate(targetCanvas.transform.GetChild(0).gameObject);
        }

        BuildPlayerInfo();
        BuildMission();
        BuildCompass();
        BuildTopRight();
        BuildMinimap();
        BuildTimer();
        BuildLeftControls();
        BuildRightControls();
        BuildBottomBar();
        BuildHPArmor();

        EditorUtility.SetDirty(targetCanvas);
        Debug.Log("<color=#4af>[HUD Builder]</color> Selesai - mirip gambar!");
    }

    // ====================== TOP LEFT - PLAYER ======================
    void BuildPlayerInfo()
    {
        var root = Panel("PlayerInfo", targetCanvas.transform, Anchor.TopLeft, new Vector2(18, -18), new Vector2(310, 88));

        // Portrait frame
        var portrait = Img("Portrait", root, new Vector2(10, -10), new Vector2(68, 68));
        portrait.color = new Color(0.18f, 0.22f, 0.28f);
        Outline(portrait.gameObject, blueAccent, 2.2f);

        // Rank badge kecil di pojok kiri bawah portrait
        var badge = Img("RankBadge", portrait.transform, new Vector2(3, 3), new Vector2(20, 20));
        badge.color = new Color(0.65f, 0.7f, 0.78f);

        // Nama
        var name = Txt("Name", root, new Vector2(90, -11), new Vector2(200, 26), "FamaLine", 19, FontStyles.Bold);
        
        // Level
        Txt("Level", root, new Vector2(90, -36), new Vector2(70, 20), "Lv. 12", 15);

        // XP bar
        var xpBg = Img("XPBG", root, new Vector2(160, -38), new Vector2(135, 11));
        xpBg.color = new Color(0.12f, 0.16f, 0.22f);
        var xpFill = Img("XPFill", xpBg.transform, Vector2.zero, Vector2.zero);
        xpFill.color = blueAccent;
        Stretch(xpFill, 0.36f); // 720/2000

        Txt("XPText", xpBg.transform, Vector2.zero, Vector2.zero, "720 / 2,000", 10, FontStyles.Normal, TextAlignmentOptions.Center);

        // Rank text
        Txt("Rank", root, new Vector2(90, -60), new Vector2(120, 18), "Silver III", 13);
    }

    // ====================== MISSION ======================
    void BuildMission()
    {
        var root = Panel("Mission", targetCanvas.transform, Anchor.TopLeft, new Vector2(18, -120), new Vector2(330, 82));

        // Icon kuning
        var icon = Img("Icon", root, new Vector2(12, -12), new Vector2(26, 26));
        icon.color = new Color(1f, 0.84f, 0.2f);

        var title = Txt("Title", root, new Vector2(48, -11), new Vector2(260, 24), "Main Mission", 15, FontStyles.Bold);
        title.color = new Color(1f, 0.88f, 0.3f);

        Txt("Desc", root, new Vector2(12, -38), new Vector2(300, 20), "Infiltrate the Research Facility", 14);

        var obj = Txt("Obj", root, new Vector2(12, -58), new Vector2(300, 18), "◆ Reach the Control Room (0/1)", 13);
        obj.color = new Color(0.65f, 0.82f, 1f);
    }

    // ====================== COMPASS ======================
    void BuildCompass()
    {
        var root = Panel("Compass", targetCanvas.transform, Anchor.TopCenter, new Vector2(0, -12), new Vector2(460, 38));
        root.color = new Color(0, 0, 0, 0.4f);

        string[] labels = { "W", "285", "300", "NW", "330", "N", "15", "30", "NE", "45" };
        float space = 46f;
        float start = -space * (labels.Length - 1) / 2f;

        for (int i = 0; i < labels.Length; i++)
        {
            var t = Txt($"D{i}", root.transform, new Vector2(start + i * space, 1), new Vector2(42, 34), labels[i], 14);
            bool isN = labels[i] == "N";
            t.color = isN ? cyan : new Color(0.88f, 0.92f, 1f);
            if (isN) t.fontStyle = FontStyles.Bold;
        }

        // Panah kecil di bawah N
        var arrow = Img("Arrow", root.transform, new Vector2(0, -16), new Vector2(10, 7));
        arrow.color = cyan;
    }

    // ====================== TOP RIGHT ======================
    void BuildTopRight()
    {
        // Diamond
        var dBox = Panel("Diamond", targetCanvas.transform, Anchor.TopRight, new Vector2(-195, -14), new Vector2(105, 34));
        var dIcon = Img("DIcon", dBox.transform, new Vector2(7, 5), new Vector2(22, 22));
        dIcon.color = new Color(0.4f, 0.7f, 1f);
        var dTxt = Txt("DText", dBox.transform, new Vector2(34, 5), new Vector2(65, 24), "1,200", 15, FontStyles.Bold);

        // Gold
        var gBox = Panel("Gold", targetCanvas.transform, Anchor.TopRight, new Vector2(-80, -14), new Vector2(115, 34));
        var gIcon = Img("GIcon", gBox.transform, new Vector2(7, 5), new Vector2(22, 22));
        gIcon.color = gold;
        var gTxt = Txt("GText", gBox.transform, new Vector2(34, 5), new Vector2(75, 24), "15,340", 15, FontStyles.Bold);

        // Notif + Settings
        var notif = Img("Notif", targetCanvas.transform, Anchor.TopRight, new Vector2(-38, -16), new Vector2(30, 30));
        notif.color = new Color(0.9f, 0.28f, 0.28f);
        var set = Img("Settings", targetCanvas.transform, Anchor.TopRight, new Vector2(-5, -16), new Vector2(30, 30));
        set.color = new Color(0.7f, 0.75f, 0.82f);
    }

    // ====================== MINIMAP ======================
    void BuildMinimap()
    {
        var root = Panel("Minimap", targetCanvas.transform, Anchor.TopRight, new Vector2(-18, -62), new Vector2(155, 155));
        Outline(root.gameObject, blueAccent, 2f);

        var content = Img("Content", root.transform, Vector2.zero, Vector2.zero);
        content.color = new Color(0.1f, 0.18f, 0.26f);
        var crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.06f, 0.06f);
        crt.anchorMax = new Vector2(0.94f, 0.94f);
        crt.offsetMin = crt.offsetMax = Vector2.zero;

        // Player arrow (cyan)
        var player = Img("Player", content.transform, Vector2.zero, new Vector2(16, 16));
        player.color = cyan;

        // Objective (kuning)
        var obj = Img("Obj", content.transform, new Vector2(35, 45), new Vector2(14, 14));
        obj.color = new Color(1f, 0.85f, 0.2f);
    }

    // ====================== TIMER ======================
    void BuildTimer()
    {
        var root = Panel("Timer", targetCanvas.transform, Anchor.TopRight, new Vector2(-18, -232), new Vector2(96, 34));

        var icon = Img("Clock", root.transform, new Vector2(8, 6), new Vector2(20, 20));
        icon.color = Color.white;

        var t = Txt("Time", root.transform, new Vector2(34, 5), new Vector2(58, 24), "14:32", 17, FontStyles.Bold);
    }

    // ====================== LEFT CONTROLS ======================
    void BuildLeftControls()
    {
        // Joystick outer
        var outer = Img("JoyOuter", targetCanvas.transform, Anchor.BottomLeft, new Vector2(130, 145), new Vector2(170, 170));
        outer.color = new Color(1, 1, 1, 0.13f);

        // Joystick inner
        var inner = Img("JoyInner", outer.transform, Vector2.zero, new Vector2(72, 72));
        inner.color = new Color(1, 1, 1, 0.32f);
        Center(inner);

        // Bullet button (kiri atas joystick)
        CircleBtn("BulletBtn", targetCanvas.transform, Anchor.BottomLeft, new Vector2(55, 300), 52);

        // Sprint button
        CircleBtn("SprintBtn", targetCanvas.transform, Anchor.BottomLeft, new Vector2(150, 300), 52);
    }

    // ====================== RIGHT CONTROLS ======================
    void BuildRightControls()
    {
        // FIRE (besar merah)
        var fire = CircleBtn("Fire", targetCanvas.transform, Anchor.BottomRight, new Vector2(-105, 145), 125);
        fire.GetComponent<Image>().color = new Color(0.92f, 0.22f, 0.18f, 0.92f);

        // AIM
        CircleBtn("Aim", targetCanvas.transform, Anchor.BottomRight, new Vector2(-105, 295), 68);

        // Ability Q E R
        Ability("Q", targetCanvas.transform, new Vector2(-210, 365));
        Ability("E", targetCanvas.transform, new Vector2(-130, 405));
        Ability("R", targetCanvas.transform, new Vector2(-55, 365));

        // Crouch & Jump
        CircleBtn("Crouch", targetCanvas.transform, Anchor.BottomRight, new Vector2(-210, 235), 52);
        CircleBtn("Jump", targetCanvas.transform, Anchor.BottomRight, new Vector2(-55, 235), 52);
    }

    // ====================== BOTTOM WEAPON BAR ======================
    void BuildBottomBar()
    {
        var root = Empty("WeaponBar", targetCanvas.transform, Anchor.BottomCenter, new Vector2(0, 85), new Vector2(680, 65));

        // Primary
        var prim = Panel("Primary", root.transform, Anchor.MiddleCenter, new Vector2(-210, 0), new Vector2(195, 52));
        Outline(prim.gameObject, blueAccent, 1.5f);
        var pAmmo = Txt("PAmmo", prim.transform, new Vector2(14, 6), new Vector2(110, 40), "30 / 120", 21, FontStyles.Bold);

        // Secondary
        var sec = Panel("Secondary", root.transform, Anchor.MiddleCenter, new Vector2(-15, 0), new Vector2(130, 48));
        Txt("SAmmo", sec.transform, new Vector2(12, 5), new Vector2(100, 38), "15 / 45", 17);

        // Grenade
        var gre = Panel("Grenade", root.transform, Anchor.MiddleCenter, new Vector2(115, 0), new Vector2(65, 48));
        var gTxt = Txt("GCount", gre.transform, Vector2.zero, Vector2.zero, "2", 19, FontStyles.Bold, TextAlignmentOptions.Center);

        // Medkit
        var med = Panel("Medkit", root.transform, Anchor.MiddleCenter, new Vector2(190, 0), new Vector2(65, 48));
        var mTxt = Txt("MCount", med.transform, Vector2.zero, Vector2.zero, "1", 19, FontStyles.Bold, TextAlignmentOptions.Center);
        mTxt.color = new Color(0.4f, 1f, 0.55f);
    }

    // ====================== HP + ARMOR ======================
    void BuildHPArmor()
    {
        var root = Empty("HPArmor", targetCanvas.transform, Anchor.BottomCenter, new Vector2(0, 22), new Vector2(400, 48));

        Txt("HPLabel", root.transform, new Vector2(-175, 14), new Vector2(55, 18), "HP 100", 13);
        var hpBg = Img("HPBG", root.transform, new Vector2(0, 16), new Vector2(265, 13));
        hpBg.color = new Color(0.12f, 0.16f, 0.2f);
        var hpFill = Img("HPFill", hpBg.transform, Vector2.zero, Vector2.zero);
        hpFill.color = hpGreen;
        Stretch(hpFill, 1f);

        Txt("ArmorLabel", root.transform, new Vector2(-175, -7), new Vector2(70, 18), "Armor 100", 13);
        var arBg = Img("ArmorBG", root.transform, new Vector2(0, -5), new Vector2(265, 11));
        arBg.color = new Color(0.12f, 0.16f, 0.2f);
        var arFill = Img("ArmorFill", arBg.transform, Vector2.zero, Vector2.zero);
        arFill.color = armorBlue;
        Stretch(arFill, 1f);
    }

    // ====================== HELPERS ======================
    enum Anchor { TopLeft, TopRight, TopCenter, BottomLeft, BottomRight, BottomCenter, MiddleCenter }

    Image Panel(string name, Transform parent, Anchor a, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        SetAnchor(go.GetComponent<RectTransform>(), a, pos, size);
        var img = go.GetComponent<Image>();
        img.color = panelDark;
        return img;
    }

    GameObject Empty(string name, Transform parent, Anchor a, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        SetAnchor(go.GetComponent<RectTransform>(), a, pos, size);
        return go;
    }

    Image Img(string name, Transform parent, Vector2 pos, Vector2 size, Anchor a = Anchor.TopLeft)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        if (a == Anchor.TopLeft)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
        }
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go.GetComponent<Image>();
    }

    Image Img(string name, Transform parent, Anchor a, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        SetAnchor(go.GetComponent<RectTransform>(), a, pos, size);
        return go.GetComponent<Image>();
    }

    TextMeshProUGUI Txt(string name, Transform parent, Vector2 pos, Vector2 size, string text, float sizeF,
        FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = sizeF;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    GameObject CircleBtn(string name, Transform parent, Anchor a, Vector2 pos, float diameter)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        SetAnchor(go.GetComponent<RectTransform>(), a, pos, new Vector2(diameter, diameter));
        go.GetComponent<Image>().color = new Color(0.18f, 0.23f, 0.32f, 0.88f);
        return go;
    }

    void Ability(string key, Transform parent, Vector2 pos)
    {
        var btn = CircleBtn("Ability" + key, parent, Anchor.BottomRight, pos, 56);
        btn.GetComponent<Image>().color = new Color(0.12f, 0.17f, 0.26f, 0.92f);

        var t = Txt("Key", btn.transform, Vector2.zero, new Vector2(56, 56), key, 17, FontStyles.Bold, TextAlignmentOptions.Center);
        t.color = cyan;
        var rt = t.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    void SetAnchor(RectTransform rt, Anchor a, Vector2 pos, Vector2 size)
    {
        switch (a)
        {
            case Anchor.TopLeft:     rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); break;
            case Anchor.TopRight:    rt.anchorMin = rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(1, 1); break;
            case Anchor.TopCenter:   rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1); break;
            case Anchor.BottomLeft:  rt.anchorMin = rt.anchorMax = new Vector2(0, 0); rt.pivot = new Vector2(0, 0); break;
            case Anchor.BottomRight: rt.anchorMin = rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(1, 0); break;
            case Anchor.BottomCenter:rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0); rt.pivot = new Vector2(0.5f, 0); break;
            case Anchor.MiddleCenter:rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); break;
        }
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    void Stretch(Image img, float fill)
    {
        var rt = img.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(Mathf.Clamp01(fill), 1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void Center(Image img)
    {
        var rt = img.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    void Outline(GameObject go, Color c, float t)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = c;
        o.effectDistance = new Vector2(t, -t);
    }
}
#endif
