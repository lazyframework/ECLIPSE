#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class HUDBuilderAccurate : EditorWindow
{
    Canvas canvas;
    bool clear = true;
    TMP_FontAsset font;

    // Warna mirip original
    readonly Color panel      = new Color(0.06f, 0.10f, 0.16f, 0.90f);
    readonly Color blue       = new Color(0.20f, 0.55f, 0.95f);
    readonly Color cyan       = new Color(0.25f, 0.88f, 1f);
    readonly Color gold       = new Color(1f, 0.82f, 0.25f);
    readonly Color hpCol      = new Color(0.25f, 0.95f, 0.45f);
    readonly Color armorCol   = new Color(0.35f, 0.65f, 1f);
    readonly Color fireRed    = new Color(0.90f, 0.22f, 0.18f, 0.93f);
    readonly Color btnDark    = new Color(0.14f, 0.19f, 0.28f, 0.92f);

    [MenuItem("Tools/HUD Builder - Accurate Layout")]
    static void Open() => GetWindow<HUDBuilderAccurate>("HUD Accurate");

    void OnGUI()
    {
        GUILayout.Space(8);
        canvas = (Canvas)EditorGUILayout.ObjectField("Canvas", canvas, typeof(Canvas), true);
        clear  = EditorGUILayout.Toggle("Clear Existing Children", clear);
        font   = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font (opsional)", font, typeof(TMP_FontAsset), false);

        GUILayout.Space(14);
        GUI.backgroundColor = new Color(0.15f, 0.75f, 0.35f);
        if (GUILayout.Button("BUILD HUD (Layout Mirip Gambar)", GUILayout.Height(46)))
        {
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "Pilih Canvas terlebih dahulu!", "OK");
                return;
            }
            Build();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "PENTING:\n" +
            "• Canvas Scaler → Scale With Screen Size\n" +
            "• Reference Resolution = 1920 x 1080\n" +
            "• Match = 0.5\n\n" +
            "Setelah build, jangan lupa assign sprite icon secara manual.",
            MessageType.Info);
    }

    void Build()
    {
        if (clear)
        {
            while (canvas.transform.childCount > 0)
                Undo.DestroyObjectImmediate(canvas.transform.GetChild(0).gameObject);
        }

        BuildPlayerInfo();
        BuildMission();
        BuildCompass();
        BuildResources();
        BuildMinimap();
        BuildTimer();
        BuildLeftControls();
        BuildRightControls();
        BuildWeaponBar();
        BuildHPArmor();

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("<color=#4af>[HUD Accurate]</color> Layout selesai dibangun!");
    }

    // =========================================================
    // 1. PLAYER INFO (kiri atas)
    // =========================================================
    void BuildPlayerInfo()
    {
        var root = Panel("PlayerInfo", canvas.transform, Anchor.TL, new Vector2(18, -16), new Vector2(310, 90));

        // Portrait
        var port = Img("Portrait", root, new Vector2(10, -10), new Vector2(72, 72));
        port.color = new Color(0.16f, 0.20f, 0.26f);
        Outline(port.gameObject, blue, 2.5f);

        // Rank badge
        var badge = Img("RankBadge", port.transform, new Vector2(3, 3), new Vector2(22, 22));
        badge.color = new Color(0.62f, 0.67f, 0.75f);

        // Nama
        Txt("Name", root, new Vector2(94, -12), new Vector2(200, 26), "FamaLine", 20, FontStyles.Bold);

        // Level
        Txt("Level", root, new Vector2(94, -38), new Vector2(70, 20), "Lv. 12", 15);

        // XP Bar
        var xpBg = Img("XPBG", root, new Vector2(165, -40), new Vector2(130, 12));
        xpBg.color = new Color(0.12f, 0.16f, 0.22f);
        var xpFill = Img("XPFill", xpBg.transform, Vector2.zero, Vector2.zero);
        xpFill.color = blue;
        Fill(xpFill, 0.36f);
        Txt("XPText", xpBg.transform, Vector2.zero, Vector2.zero, "720 / 2,000", 10, FontStyles.Normal, TextAlignmentOptions.Center);

        // Rank text
        Txt("Rank", root, new Vector2(94, -63), new Vector2(130, 18), "Silver III", 13.5f);
    }

    // =========================================================
    // 2. MISSION
    // =========================================================
    void BuildMission()
    {
        var root = Panel("Mission", canvas.transform, Anchor.TL, new Vector2(18, -120), new Vector2(330, 82));

        var icon = Img("Icon", root, new Vector2(12, -12), new Vector2(26, 26));
        icon.color = new Color(1f, 0.84f, 0.2f);

        var title = Txt("Title", root, new Vector2(48, -11), new Vector2(265, 24), "Main Mission", 15.5f, FontStyles.Bold);
        title.color = new Color(1f, 0.88f, 0.28f);

        Txt("Desc", root, new Vector2(12, -38), new Vector2(300, 20), "Infiltrate the Research Facility", 14);

        var obj = Txt("Obj", root, new Vector2(12, -58), new Vector2(300, 18), "◆ Reach the Control Room (0/1)", 13.5f);
        obj.color = new Color(0.65f, 0.82f, 1f);
    }

    // =========================================================
    // 3. COMPASS
    // =========================================================
    void BuildCompass()
    {
        var root = Panel("Compass", canvas.transform, Anchor.TC, new Vector2(0, -10), new Vector2(500, 38));
        root.color = new Color(0, 0, 0, 0.40f);

        string[] dirs = { "W", "285", "300", "NW", "330", "N", "15", "30", "NE", "45" };
        float spacing = 49f;
        float startX = -spacing * (dirs.Length - 1) / 2f;

        for (int i = 0; i < dirs.Length; i++)
        {
            var t = Txt($"Dir{i}", root.transform, new Vector2(startX + i * spacing, 1), new Vector2(46, 34), dirs[i], 14.5f);
            bool isNorth = dirs[i] == "N";
            t.color = isNorth ? cyan : new Color(0.88f, 0.92f, 1f);
            if (isNorth) t.fontStyle = FontStyles.Bold;
        }
    }

    // =========================================================
    // 4. RESOURCES (kanan atas)
    // =========================================================
    void BuildResources()
    {
        // Diamond
        var dBox = Panel("Diamond", canvas.transform, Anchor.TR, new Vector2(-205, -14), new Vector2(105, 34));
        var dIcon = Img("DIcon", dBox.transform, new Vector2(8, 6), new Vector2(22, 22));
        dIcon.color = new Color(0.4f, 0.7f, 1f);
        Txt("DText", dBox.transform, new Vector2(35, 5), new Vector2(65, 24), "1,200", 15.5f, FontStyles.Bold);

        // Gold
        var gBox = Panel("Gold", canvas.transform, Anchor.TR, new Vector2(-88, -14), new Vector2(115, 34));
        var gIcon = Img("GIcon", gBox.transform, new Vector2(8, 6), new Vector2(22, 22));
        gIcon.color = gold;
        Txt("GText", gBox.transform, new Vector2(35, 5), new Vector2(75, 24), "15,340", 15.5f, FontStyles.Bold);

        // Bell + Settings
        var bell = Img("Bell", canvas.transform, Anchor.TR, new Vector2(-42, -16), new Vector2(30, 30));
        bell.color = new Color(0.9f, 0.25f, 0.25f);
        var setting = Img("Setting", canvas.transform, Anchor.TR, new Vector2(-6, -16), new Vector2(30, 30));
        setting.color = new Color(0.7f, 0.75f, 0.82f);
    }

    // =========================================================
    // 5. MINIMAP
    // =========================================================
    void BuildMinimap()
    {
        var root = Panel("Minimap", canvas.transform, Anchor.TR, new Vector2(-16, -60), new Vector2(155, 155));
        Outline(root.gameObject, blue, 2.2f);

        var map = Img("MapContent", root.transform, Vector2.zero, Vector2.zero);
        map.color = new Color(0.09f, 0.16f, 0.24f);
        var rt = map.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.055f, 0.055f);
        rt.anchorMax = new Vector2(0.945f, 0.945f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Player arrow
        var player = Img("PlayerArrow", map.transform, Vector2.zero, new Vector2(16, 16));
        player.color = cyan;
        Center(player);

        // Objective marker
        var obj = Img("ObjMarker", map.transform, new Vector2(36, 48), new Vector2(14, 14));
        obj.color = new Color(1f, 0.85f, 0.2f);
    }

    // =========================================================
    // 6. TIMER
    // =========================================================
    void BuildTimer()
    {
        var root = Panel("Timer", canvas.transform, Anchor.TR, new Vector2(-16, -228), new Vector2(96, 34));
        var clock = Img("Clock", root.transform, new Vector2(8, 6), new Vector2(20, 20));
        clock.color = Color.white;
        Txt("Time", root.transform, new Vector2(33, 5), new Vector2(58, 24), "14:32", 16.5f, FontStyles.Bold);
    }

    // =========================================================
    // 7. LEFT CONTROLS (Joystick + Bullet + Sprint)
    // =========================================================
    void BuildLeftControls()
    {
        // Joystick Outer
        var outer = Img("JoystickOuter", canvas.transform, Anchor.BL, new Vector2(125, 140), new Vector2(175, 175));
        outer.color = new Color(1f, 1f, 1f, 0.13f);

        // Joystick Inner
        var inner = Img("JoystickInner", outer.transform, Vector2.zero, new Vector2(75, 75));
        inner.color = new Color(1f, 1f, 1f, 0.32f);
        Center(inner);

        // Bullet button
        Circle("BulletBtn", canvas.transform, Anchor.BL, new Vector2(52, 305), 54, btnDark);

        // Sprint button
        Circle("SprintBtn", canvas.transform, Anchor.BL, new Vector2(148, 305), 54, btnDark);
    }

    // =========================================================
    // 8. RIGHT CONTROLS
    // =========================================================
    void BuildRightControls()
    {
        // FIRE (besar)
        Circle("FireBtn", canvas.transform, Anchor.BR, new Vector2(-105, 140), 128, fireRed);

        // AIM
        Circle("AimBtn", canvas.transform, Anchor.BR, new Vector2(-105, 295), 68, btnDark);

        // Abilities Q - E - R
        AbilityBtn("Q", new Vector2(-215, 365));
        AbilityBtn("E", new Vector2(-132, 405));
        AbilityBtn("R", new Vector2(-52, 365));

        // Crouch & Jump
        Circle("CrouchBtn", canvas.transform, Anchor.BR, new Vector2(-215, 235), 54, btnDark);
        Circle("JumpBtn",   canvas.transform, Anchor.BR, new Vector2(-52, 235), 54, btnDark);
    }

    // =========================================================
    // 9. WEAPON BAR
    // =========================================================
    void BuildWeaponBar()
    {
        var root = Empty("WeaponBar", canvas.transform, Anchor.BC, new Vector2(0, 82), new Vector2(680, 62));

        // Primary
        var prim = Panel("Primary", root.transform, Anchor.MC, new Vector2(-215, 0), new Vector2(200, 52));
        Outline(prim.gameObject, blue, 1.8f);
        Txt("PrimaryAmmo", prim.transform, new Vector2(14, 6), new Vector2(120, 40), "30 / 120", 21, FontStyles.Bold);

        // Secondary
        var sec = Panel("Secondary", root.transform, Anchor.MC, new Vector2(-15, 0), new Vector2(135, 48));
        Txt("SecAmmo", sec.transform, new Vector2(12, 5), new Vector2(110, 38), "15 / 45", 17);

        // Grenade
        var gr = Panel("Grenade", root.transform, Anchor.MC, new Vector2(120, 0), new Vector2(68, 48));
        Txt("GrCount", gr.transform, Vector2.zero, Vector2.zero, "2", 19, FontStyles.Bold, TextAlignmentOptions.Center);

        // Medkit
        var med = Panel("Medkit", root.transform, Anchor.MC, new Vector2(198, 0), new Vector2(68, 48));
        var medTxt = Txt("MedCount", med.transform, Vector2.zero, Vector2.zero, "1", 19, FontStyles.Bold, TextAlignmentOptions.Center);
        medTxt.color = new Color(0.4f, 1f, 0.55f);
    }

    // =========================================================
    // 10. HP + ARMOR
    // =========================================================
    void BuildHPArmor()
    {
        var root = Empty("HPArmor", canvas.transform, Anchor.BC, new Vector2(0, 18), new Vector2(400, 48));

        // HP
        Txt("HPLabel", root.transform, new Vector2(-175, 14), new Vector2(60, 18), "HP 100", 13.5f);
        var hpBg = Img("HPBG", root.transform, new Vector2(0, 16), new Vector2(270, 13));
        hpBg.color = new Color(0.12f, 0.16f, 0.21f);
        var hpFill = Img("HPFill", hpBg.transform, Vector2.zero, Vector2.zero);
        hpFill.color = hpCol;
        Fill(hpFill, 1f);

        // Armor
        Txt("ArmorLabel", root.transform, new Vector2(-175, -7), new Vector2(75, 18), "Armor 100", 13.5f);
        var arBg = Img("ArmorBG", root.transform, new Vector2(0, -5), new Vector2(270, 12));
        arBg.color = new Color(0.12f, 0.16f, 0.21f);
        var arFill = Img("ArmorFill", arBg.transform, Vector2.zero, Vector2.zero);
        arFill.color = armorCol;
        Fill(arFill, 1f);
    }

    // ====================== HELPERS ======================
    enum Anchor { TL, TR, TC, BL, BR, BC, MC }

    Image Panel(string name, Transform parent, Anchor a, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        SetAnchor(go.GetComponent<RectTransform>(), a, pos, size);
        var img = go.GetComponent<Image>();
        img.color = panel;
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

    Image Img(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
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

    TextMeshProUGUI Txt(string name, Transform parent, Vector2 pos, Vector2 size, string content, float fontSize,
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
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    GameObject Circle(string name, Transform parent, Anchor a, Vector2 pos, float diameter, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        SetAnchor(go.GetComponent<RectTransform>(), a, pos, new Vector2(diameter, diameter));
        go.GetComponent<Image>().color = color;
        return go;
    }

    void AbilityBtn(string key, Vector2 pos)
    {
        var btn = Circle("Ability_" + key, canvas.transform, Anchor.BR, pos, 56, new Color(0.12f, 0.17f, 0.26f, 0.93f));
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
            case Anchor.TL: rt.anchorMin = rt.anchorMax = new Vector2(0, 1);     rt.pivot = new Vector2(0, 1);     break;
            case Anchor.TR: rt.anchorMin = rt.anchorMax = new Vector2(1, 1);     rt.pivot = new Vector2(1, 1);     break;
            case Anchor.TC: rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);  rt.pivot = new Vector2(0.5f, 1);  break;
            case Anchor.BL: rt.anchorMin = rt.anchorMax = new Vector2(0, 0);     rt.pivot = new Vector2(0, 0);     break;
            case Anchor.BR: rt.anchorMin = rt.anchorMax = new Vector2(1, 0);     rt.pivot = new Vector2(1, 0);     break;
            case Anchor.BC: rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);  rt.pivot = new Vector2(0.5f, 0);  break;
            case Anchor.MC: rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); break;
        }
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    void Fill(Image img, float amount)
    {
        var rt = img.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void Center(Image img)
    {
        var rt = img.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    void Outline(GameObject go, Color c, float thickness)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = c;
        o.effectDistance = new Vector2(thickness, -thickness);
    }
}
#endif
