// Taruh di: Assets/Scripts/HUD/VirtualJoystick.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform knob;
    public float radius = 90f;

    /// <summary>Nilai -1..1 pada kedua sumbu. Baca dari script movement Anda.</summary>
    public Vector2 Value { get; private set; }

    RectTransform _rt;

    void Awake() => _rt = (RectTransform)transform;

    public void OnPointerDown(PointerEventData e) => OnDrag(e);

    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out var local);
        local = Vector2.ClampMagnitude(local, radius);
        if (knob) knob.anchoredPosition = local;
        Value = local / radius;
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (knob) knob.anchoredPosition = Vector2.zero;
        Value = Vector2.zero;
    }
}
