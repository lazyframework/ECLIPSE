// Taruh di: Assets/Scripts/HUD/CombatHUD.cs
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatHUD : MonoBehaviour
{
    [Header("Profile")]
    public Image avatar, rankIcon;
    public TMP_Text playerName, level, xpText, rankText;
    public RectTransform xpFill;

    [Header("Mission")]
    public TMP_Text missionTitle, objective, subObjective;

    [Header("Currency")]
    public TMP_Text gemsText, coinsText;
    public Button gemPlus, coinPlus, notifButton, settingsButton;

    [Header("Compass")]
    public RectTransform compassStrip;
    public TMP_Text headingText;
    public Transform headingSource;               // isi dengan kamera/pemain (opsional)
    public float pixelsPerDegree = 4.07f;
    public float halfWidth = 340f;

    [Header("Minimap")]
    public RawImage minimapView;                  // assign RenderTexture kamera top-down
    public RectTransform minimapPlayer;
    public TMP_Text timerText;

    [Header("Vitals")]
    public TMP_Text hpText, armorText;
    public RectTransform hpFill, armorFill;

    [Header("Weapons")]
    public TMP_Text primaryAmmo, secondaryAmmo, grenadeText, medkitText;
    public Image[] slotBackgrounds;
    public Outline[] slotOutlines;
    public Button primaryButton, secondaryButton, grenadeButton, medkitButton;

    [Header("Controls")]
    public VirtualJoystick joystick;
    public Button fireButton, reloadButton, sprintButton, aimButton, crouchButton, proneButton;
    public Button skillQ, skillE, skillR;

    readonly List<(TextMeshProUGUI t, int angle)> _labels = new List<(TextMeshProUGUI, int)>();

    void Awake() => BuildCompassLabels();

    void Update()
    {
        if (headingSource) SetHeading(headingSource.eulerAngles.y);
    }

    // ------------------------------------------------------------------ API
    public void SetHealth(int value, int max = 100)
    {
        if (hpText) hpText.text = "HP " + value;
        SetFill(hpFill, (float)value / max);
    }

    public void SetArmor(int value, int max = 100)
    {
        if (armorText) armorText.text = "Armor " + value;
        SetFill(armorFill, (float)value / max);
    }

    public void SetXP(int lv, int xp, int next)
    {
        if (level) level.text = "Lv. " + lv;
        if (xpText) xpText.text = xp.ToString("N0", CultureInfo.InvariantCulture) + " / " + next.ToString("N0", CultureInfo.InvariantCulture);
        SetFill(xpFill, (float)xp / next);
    }

    public void SetAmmo(int mag, int reserve)
    {
        if (primaryAmmo) primaryAmmo.text = mag + "<size=60%><color=#9FB4D6> / " + reserve + "</color></size>";
    }

    public void SetSecondaryAmmo(int mag, int reserve)
    {
        if (secondaryAmmo) secondaryAmmo.text = mag + "<size=70%><color=#9FB4D6> / " + reserve + "</color></size>";
    }

    public void SetGrenades(int n) { if (grenadeText) grenadeText.text = n.ToString(); }
    public void SetMedkits(int n)  { if (medkitText)  medkitText.text  = n.ToString(); }

    public void SetCurrency(int gems, int coins)
    {
        if (gemsText)  gemsText.text  = gems.ToString("N0", CultureInfo.InvariantCulture);
        if (coinsText) coinsText.text = coins.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void SetMission(string title, string main, string sub)
    {
        if (missionTitle) missionTitle.text = title;
        if (objective) objective.text = main;
        if (subObjective) subObjective.text = sub;
    }

    public void SetTimer(float seconds)
    {
        int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
        if (timerText) timerText.text = (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
    }

    public void SelectSlot(int index)
    {
        if (slotBackgrounds == null) return;
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            bool on = i == index;
            slotBackgrounds[i].color = on ? new Color32(20, 60, 140, 230) : new Color32(8, 18, 38, 175);
            if (slotOutlines != null && i < slotOutlines.Length) slotOutlines[i].enabled = on;
        }
    }

    public void SetMinimapPlayerRotation(float yaw)
    {
        if (minimapPlayer) minimapPlayer.localEulerAngles = new Vector3(0, 0, -yaw);
    }

    // -------------------------------------------------------------- compass
    public void SetHeading(float heading)
    {
        heading = Mathf.Repeat(heading, 360f);
        if (headingText) headingText.text = (Mathf.RoundToInt(heading) % 360).ToString();

        foreach (var (t, angle) in _labels)
        {
            float x = Mathf.DeltaAngle(heading, angle) * pixelsPerDegree;
            float ax = Mathf.Abs(x);
            t.rectTransform.anchoredPosition = new Vector2(x, 0);
            t.alpha = (ax < halfWidth && ax > 48f) ? 1f - ax / halfWidth : 0f;
        }
    }

    void BuildCompassLabels()
    {
        if (!compassStrip || _labels.Count > 0) return;
        string[] card = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        for (int a = 0; a < 360; a += 15)
        {
            bool cardinal = a % 45 == 0;
            var go = new GameObject("L" + a, typeof(RectTransform));
            go.transform.SetParent(compassStrip, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = cardinal ? card[a / 45] : a.ToString();
            t.fontSize = cardinal ? 30 : 24;
            t.fontStyle = cardinal ? FontStyles.Bold : FontStyles.Normal;
            t.alignment = TextAlignmentOptions.Center;
            t.color = cardinal ? Color.white : new Color(1, 1, 1, .7f);
            t.raycastTarget = false;
            if (headingText) t.font = headingText.font;
            t.rectTransform.sizeDelta = new Vector2(70, 40);
            _labels.Add((t, a));
        }
        SetHeading(0);
    }

    static void SetFill(RectTransform fill, float normalized)
    {
        if (fill) fill.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
    }
}
