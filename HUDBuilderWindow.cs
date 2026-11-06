#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class HUDBuilderWindow : EditorWindow
{
    private Canvas targetCanvas;
    private bool clearExisting = true;
    private TMP_FontAsset fontRegular;
    private TMP_FontAsset fontBold;

    // Colors
    private Color primaryBlue = new Color(0.15f, 0.55f, 0.95f);
    private Color accentCyan = new Color(0.2f, 0.85f, 1f);
    private Color darkPanel = new Color(0.08f, 0.12f, 0.18f, 0.85f);
    private Color hpGreen = new Color(0.2f, 0.9f, 0.4f);
    private Color armorBlue = new Color(0.3f, 0.6f, 1f);

    [MenuItem("Tools/HUD Builder")]
    public static void ShowWindow()
    {
        GetWindow<HUDBuilderWindow>("HUD Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("FPS HUD Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space(8);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
        clearExisting = EditorGUILayout.Toggle("Clear Existing Children", clearExisting);

        EditorGUILayout.Space(6);
        fontRegular = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Regular", fontRegular, typeof(TMP_FontAsset), false);
        fontBold = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Bold", fontBold, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space(15);

        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
        if (GUILayout.Button("Build HUD", GUILayout.Height(40)))
        {
            if (targetCanvas == null)
            {
                EditorUtility.DisplayDialog("Error", "Pilih Canvas dulu!", "OK");
                return;
            }
            BuildHUD();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Hasil HUD akan dibuat sebagai child dari Canvas yang dipilih.\nGunakan Tools → HUD Builder untuk membuka window ini.", MessageType.Info);
    }

    void BuildHUD()
    {
        if (clearExisting)
        {
            for (int i = targetCanvas.transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(targetCanvas.transform.GetChild(i).gameObject);
            }
        }

        // ========== BUILD SEMUA ELEMEN ==========
        BuildTopLeftPlayerInfo();
        BuildMissionPanel();
        BuildCompass();
        BuildTopRightResources();
        BuildMinimap();
        BuildTimer();
        BuildLeftJoystickArea();
        BuildRightActionButtons();
        BuildBottomWeaponBar();
        BuildHPArmorBar();

        // Tambahkan component referensi
        var refs = targetCanvas.GetComponent<HUDReferences>();
        if (refs == null)
            refs = Undo.AddComponent<HUDReferences>(targetCanvas.gameObject);

        EditorUtility.SetDirty(targetCanvas.gameObject);
        Debug.Log("<color=cyan>[HUD Builder]</color> Selesai membangun HUD!");
    }

    // ==================== TOP LEFT ====================
    void BuildTopLeftPlayerInfo()
    {
        var root = CreatePanel("PlayerInfo", targetCanvas.transform, new Vector2(20, -20), new Vector2(320, 90), TextAnchor.UpperLeft);

        // Portrait
        var portrait = CreateImage("Portrait", root.transform, new Vector2(8, -8), new Vector2(74, 74));
        portrait.color = new Color(0.2f, 0.25f, 0.3f);
        AddOutline(portrait.gameObject, primaryBlue, 2f);

        // Rank badge
        var rankBadge = CreateImage("RankBadge", portrait.transform, new Vector2(2, 2), new Vector2(22, 22));
        rankBadge.color = new Color(0.7f, 0.75f, 0.8f);

        // Name
        var nameText = CreateText("PlayerName", root.transform, new Vector2(95, -12), new Vector2(200, 28), "FamaLine", 20, TextAlignmentOptions.Left);
        nameText.fontStyle = FontStyles.Bold;

        // Level
        CreateText("LevelText", root.transform, new Vector2(95, -38), new Vector2(80, 22), "Lv. 12", 16, TextAlignmentOptions.Left);

        // XP Bar
        var xpBg = CreateImage("XPBarBG", root.transform, new Vector2(170, -40), new Vector2(130, 12));
        xpBg.color = new Color(0.15f, 0.2f, 0.25f);

        var xpFill = CreateImage("XPFill", xpBg.transform, Vector2.zero, Vector2.zero);
        xpFill.color = primaryBlue;
        var xpRect = xpFill.GetComponent<RectTransform>();
        xpRect.anchorMin = Vector2.zero;
        xpRect.anchorMax = new Vector2(0.36f, 1f);
        xpRect.offsetMin = xpRect.offsetMax = Vector2.zero;

        CreateText("XPText", xpBg.transform, Vector2.zero, Vector2.zero, "720 / 2,000", 11, TextAlignmentOptions.Center);

        // Rank
        CreateText("RankText", root.transform, new Vector2(95, -62), new Vector2(140, 20), "Silver III", 14, TextAlignmentOptions.Left);
    }

    // ==================== MISSION ====================
    void BuildMissionPanel()
    {
        var root = CreatePanel("MissionPanel", targetCanvas.transform, new Vector2(20, -125), new Vector2(340, 85), TextAnchor.UpperLeft);

        var icon = CreateImage("MissionIcon", root.transform, new Vector2(12, -12), new Vector2(28, 28));
        icon.color = new Color(1f, 0.85f, 0.2f);

        var title = CreateText("MissionTitle", root.transform, new Vector2(50, -10), new Vector2(270, 26), "Main Mission", 16, TextAlignmentOptions.Left);
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(1f, 0.9f, 0.3f);

        CreateText("MissionDesc", root.transform, new Vector2(12, -40), new Vector2(310, 22), "Infiltrate the Research Facility", 15, TextAlignmentOptions.Left);

        var objective = CreateText("Objective", root.transform, new Vector2(12, -62), new Vector2(310, 20), "◆ Reach the Control Room (0/1)", 14, TextAlignmentOptions.Left);
        objective.color = new Color(0.7f, 0.85f, 1f);
    }

    // ==================== COMPASS ====================
    void BuildCompass()
    {
        var root = CreatePanel("Compass", targetCanvas.transform, new Vector2(0, -15), new Vector2(480, 40), TextAnchor.UpperCenter);
        root.color = new Color(0, 0, 0, 0.35f);

        string[] dirs = { "W", "285", "300", "NW", "330", "N", "15", "30", "NE", "45" };
        float spacing = 48f;
        float startX = -spacing * (dirs.Length - 1) / 2f;

        for (int i = 0; i < dirs.Length; i++)
        {
            var t = CreateText($"Dir_{i}", root.transform, new Vector2(startX + i * spacing, 0), new Vector2(40, 36), dirs[i], 15, TextAlignmentOptions.Center);
            t.color = dirs[i] == "N" ? accentCyan : new Color(0.85f, 0.9f, 1f);
            if (dirs[i] == "N") t.fontStyle = FontStyles.Bold;
        }

        var indicator = CreateImage("CompassIndicator", root.transform, new Vector2(0, -18), new Vector2(12, 8));
        indicator.color = accentCyan;
    }

    // ==================== TOP RIGHT ====================
    void BuildTopRightResources()
    {
        var root = CreateEmpty("TopRightResources", targetCanvas.transform, new Vector2(-20, -15), new Vector2(380, 50), TextAnchor.UpperRight);

        // Diamonds
        var diamondBox = CreatePanel("DiamondBox", root.transform, new Vector2(-200, 0), new Vector2(110, 36), TextAnchor.UpperRight);
        var dIcon = CreateImage("DIcon", diamondBox.transform, new Vector2(8, 6), new Vector2(24, 24));
        dIcon.color = new Color(0.4f, 0.7f, 1f);
        var dText = CreateText("DiamondText", diamondBox.transform, new Vector2(38, 6), new Vector2(65, 24), "1,200", 16, TextAlignmentOptions.Left);
        dText.fontStyle = FontStyles.Bold;

        // Gold
        var goldBox = CreatePanel("GoldBox", root.transform, new Vector2(-80, 0), new Vector2(120, 36), TextAnchor.UpperRight);
        var gIcon = CreateImage("GIcon", goldBox.transform, new Vector2(8, 6), new Vector2(24, 24));
        gIcon.color = new Color(1f, 0.85f, 0.3f);
        var gText = CreateText("GoldText", goldBox.transform, new Vector2(38, 6), new Vector2(75, 24), "15,340", 16, TextAlignmentOptions.Left);
        gText.fontStyle = FontStyles.Bold;

        // Notification + Settings
        var notif = CreateImage("Notification", root.transform, new Vector2(-30, 5), new Vector2(32, 32));
        notif.color = new Color(0.9f, 0.3f, 0.3f);
        var settings = CreateImage("Settings", root.transform, new Vector2(5, 5), new Vector2(32, 32));
        settings.color = new Color(0.7f, 0.75f, 0.8f);
    }

    // ==================== MINIMAP ====================
    void BuildMinimap()
    {
        var root = CreatePanel("Minimap", targetCanvas.transform, new Vector2(-20, -70), new Vector2(160, 160), TextAnchor.UpperRight);
        AddOutline(root.gameObject, primaryBlue, 2f);

        var mapContent = CreateImage("MapContent", root.transform, Vector2.zero, Vector2.zero);
        mapContent.color = new Color(0.12f, 0.2f, 0.28f);
        var mapRect = mapContent.GetComponent<RectTransform>();
        mapRect.anchorMin = new Vector2(0.05f, 0.05f);
        mapRect.anchorMax = new Vector2(0.95f, 0.95f);
        mapRect.offsetMin = mapRect.offsetMax = Vector2.zero;

        var playerArrow = CreateImage("PlayerArrow", mapContent.transform, Vector2.zero, new Vector2(18, 18));
        playerArrow.color = accentCyan;

        var objMarker = CreateImage("ObjMarker", mapContent.transform, new Vector2(40, 50), new Vector2(16, 16));
        objMarker.color = new Color(1f, 0.85f, 0.2f);
    }

    // ==================== TIMER ====================
    void BuildTimer()
    {
        var root = CreatePanel("Timer", targetCanvas.transform, new Vector2(-20, -245), new Vector2(100, 36), TextAnchor.UpperRight);

        var clockIcon = CreateImage("ClockIcon", root.transform, new Vector2(8, 6), new Vector2(22, 22));
        clockIcon.color = Color.white;

        var timeText = CreateText("TimeText", root.transform, new Vector2(36, 5), new Vector2(60, 26), "14:32", 18, TextAlignmentOptions.Left);
        timeText.fontStyle = FontStyles.Bold;
    }

    // ==================== LEFT JOYSTICK ====================
    void BuildLeftJoystickArea()
    {
        var outer = CreateImage("JoystickOuter", targetCanvas.transform, new Vector2(140, 160), new Vector2(180, 180), TextAnchor.LowerLeft);
        outer.color = new Color(1, 1, 1, 0.15f);

        var inner = CreateImage("JoystickInner", outer.transform, Vector2.zero, new Vector2(80, 80));
        inner.color = new Color(1, 1, 1, 0.35f);

        CreateCircleButton("BulletBtn", targetCanvas.transform, new Vector2(60, 320), 55, TextAnchor.LowerLeft);
        CreateCircleButton("SprintBtn", targetCanvas.transform, new Vector2(160, 320), 55, TextAnchor.LowerLeft);
    }

    // ==================== RIGHT BUTTONS ====================
    void BuildRightActionButtons()
    {
        var fireBtn = CreateCircleButton("FireBtn", targetCanvas.transform, new Vector2(-110, 160), 130, TextAnchor.LowerRight);
        fireBtn.GetComponent<Image>().color = new Color(0.9f, 0.25f, 0.2f, 0.9f);

        CreateCircleButton("AimBtn", targetCanvas.transform, new Vector2(-110, 310), 70, TextAnchor.LowerRight);

        CreateAbilityButton("AbilityQ", targetCanvas.transform, new Vector2(-220, 380), "Q", TextAnchor.LowerRight);
        CreateAbilityButton("AbilityE", targetCanvas.transform, new Vector2(-140, 420), "E", TextAnchor.LowerRight);
        CreateAbilityButton("AbilityR", targetCanvas.transform, new Vector2(-60, 380), "R", TextAnchor.LowerRight);

        CreateCircleButton("CrouchBtn", targetCanvas.transform, new Vector2(-220, 250), 55, TextAnchor.LowerRight);
        CreateCircleButton("JumpBtn", targetCanvas.transform, new Vector2(-60, 250), 55, TextAnchor.LowerRight);
    }

    // ==================== WEAPON BAR ====================
    void BuildBottomWeaponBar()
    {
        var root = CreateEmpty("WeaponBar", targetCanvas.transform, new Vector2(0, 90), new Vector2(700, 70), TextAnchor.LowerCenter);

        var primary = CreatePanel("PrimaryWeapon", root.transform, new Vector2(-220, 0), new Vector2(200, 55), TextAnchor.MiddleCenter);
        AddOutline(primary.gameObject, primaryBlue, 1.5f);
        var pAmmo = CreateText("PrimaryAmmo", primary.transform, new Vector2(15, 8), new Vector2(100, 40), "30 / 120", 22, TextAlignmentOptions.Left);
        pAmmo.fontStyle = FontStyles.Bold;

        var secondary = CreatePanel("SecondaryWeapon", root.transform, new Vector2(-20, 0), new Vector2(140, 50), TextAnchor.MiddleCenter);
        CreateText("SecAmmo", secondary.transform, new Vector2(10, 5), new Vector2(90, 40), "15 / 45", 18, TextAlignmentOptions.Left);

        var grenade = CreatePanel("Grenade", root.transform, new Vector2(120, 0), new Vector2(70, 50), TextAnchor.MiddleCenter);
        var gCount = CreateText("GrenadeCount", grenade.transform, Vector2.zero, Vector2.zero, "2", 20, TextAlignmentOptions.Center);
        gCount.fontStyle = FontStyles.Bold;

        var medkit = CreatePanel("Medkit", root.transform, new Vector2(200, 0), new Vector2(70, 50), TextAnchor.MiddleCenter);
        var mCount = CreateText("MedkitCount", medkit.transform, Vector2.zero, Vector2.zero, "1", 20, TextAlignmentOptions.Center);
        mCount.fontStyle = FontStyles.Bold;
        mCount.color = new Color(0.4f, 1f, 0.5f);
    }

    // ==================== HP + ARMOR ====================
    void BuildHPArmorBar()
    {
        var root = CreateEmpty("HPArmorRoot", targetCanvas.transform, new Vector2(0, 25), new Vector2(420, 50), TextAnchor.LowerCenter);

        CreateText("HPLabel", root.transform, new Vector2(-180, 15), new Vector2(60, 20), "HP 100", 14, TextAlignmentOptions.Left);

        var hpBg = CreateImage("HPBG", root.transform, new Vector2(0, 18), new Vector2(280, 14));
        hpBg.color = new Color(0.15f, 0.2f, 0.25f);
        var hpFill = CreateImage("HPFill", hpBg.transform, Vector2.zero, Vector2.zero);
        hpFill.color = hpGreen;
        SetFill(hpFill, 1f);

        CreateText("ArmorLabel", root.transform, new Vector2(-180, -8), new Vector2(80, 20), "Armor 100", 14, TextAlignmentOptions.Left);

        var armorBg = CreateImage("ArmorBG", root.transform, new Vector2(0, -5), new Vector2(280, 12));
        armorBg.color = new Color(0.15f, 0.2f, 0.25f);
        var armorFill = CreateImage("ArmorFill", armorBg.transform, Vector2.zero, Vector2.zero);
        armorFill.color = armorBlue;
        SetFill(armorFill, 1f);
    }

    // ==================== HELPERS ====================
    Image CreatePanel(string name, Transform parent, Vector2 anchoredPos, Vector2 size, TextAnchor anchor = TextAnchor.UpperLeft)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.color = darkPanel;
        return img;
    }

    GameObject CreateEmpty(string name, Transform parent, Vector2 anchoredPos, Vector2 size, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    Image CreateImage(string name, Transform parent, Vector2 anchoredPos, Vector2 size, TextAnchor anchor = TextAnchor.UpperLeft)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go.GetComponent<Image>();
    }

    TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string content, float fontSize, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        if (fontRegular != null) tmp.font = fontRegular;
        tmp.raycastTarget = false;
        return tmp;
    }

    GameObject CreateCircleButton(string name, Transform parent, Vector2 anchoredPos, float diameter, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(diameter, diameter);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.25f, 0.35f, 0.85f);
        return go;
    }

    void CreateAbilityButton(string name, Transform parent, Vector2 pos, string key, TextAnchor anchor)
    {
        var btn = CreateCircleButton(name, parent, pos, 58, anchor);
        btn.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.9f);

        var keyText = CreateText("Key", btn.transform, Vector2.zero, new Vector2(58, 58), key, 18, TextAlignmentOptions.Center);
        keyText.fontStyle = FontStyles.Bold;
        keyText.color = accentCyan;

        var keyRT = keyText.GetComponent<RectTransform>();
        keyRT.anchorMin = keyRT.anchorMax = new Vector2(0.5f, 0.5f);
        keyRT.anchoredPosition = Vector2.zero;
    }

    void SetAnchor(RectTransform rt, TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                break;
            case TextAnchor.UpperRight:
                rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
            case TextAnchor.UpperCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
                break;
            case TextAnchor.LowerLeft:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                break;
            case TextAnchor.LowerRight:
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                break;
            case TextAnchor.LowerCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
                rt.pivot = new Vector2(0.5f, 0);
                break;
            case TextAnchor.MiddleCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }
    }

    void SetFill(Image img, float amount)
    {
        var rt = img.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void AddOutline(GameObject go, Color color, float thickness)
    {
        var outline = go.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(thickness, -thickness);
    }
}
#endif
