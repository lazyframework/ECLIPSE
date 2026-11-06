// Taruh di: Assets/Scripts/HUD/SafeAreaFitter.cs
using UnityEngine;

public class SafeAreaFitter : MonoBehaviour
{
    Rect _last;

    void Awake() => Apply();
    void Update() { if (Screen.safeArea != _last) Apply(); }

    void Apply()
    {
        _last = Screen.safeArea;
        var rt = (RectTransform)transform;
        Vector2 min = _last.position, max = _last.position + _last.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
