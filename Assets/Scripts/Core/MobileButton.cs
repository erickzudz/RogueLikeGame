using UnityEngine;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public bool IsHeld { get; private set; }
    public bool WasClicked { get; private set; } // se pone true un frame

    public void OnPointerDown(PointerEventData eventData) { IsHeld = true; }
    public void OnPointerUp(PointerEventData eventData) { IsHeld = false; }

    public void OnPointerClick(PointerEventData eventData) { WasClicked = true; }

    void LateUpdate() { WasClicked = false; } // limpia click cada frame
}
