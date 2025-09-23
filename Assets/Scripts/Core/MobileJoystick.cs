using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs")]
    public RectTransform bg;       // JoyBG
    public RectTransform handle;   // JoyHandle

    [Header("Tuning")]
    public float handleLimit = 80f;   // radio en px dentro del bg

    public Vector2 Axis { get; private set; } // (-1..1)

    Vector2 _startPos;

    void Awake()
    {
        if (!bg) bg = (RectTransform)transform;
        if (!handle) handle = transform.GetChild(0) as RectTransform;
        _startPos = handle.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bg, eventData.position, eventData.pressEventCamera, out var localPos);

        // Normaliza dentro del círculo
        var v = Vector2.ClampMagnitude(localPos, handleLimit);
        handle.anchoredPosition = _startPos + v;

        Axis = v / handleLimit; // -1..1
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = _startPos;
        Axis = Vector2.zero;
    }
}
