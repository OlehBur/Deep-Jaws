using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
    public RectTransform handle;
    public float maxRadius = 100f;

    public void OnPointerDown(PointerEventData eventData) {
        UpdateJoystick(eventData);
        //Debug.Log("Click");
    }

    public void OnDrag(PointerEventData eventData) {
        UpdateJoystick(eventData);

        if (JoyAxis.isReset) {
            eventData.pointerDrag = null;
            JoyAxis.isReset = false;
        }
        //Debug.Log("Drag");
    }

    public void OnPointerUp(PointerEventData eventData) {
        JoyAxis.Axis = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        //Debug.Log("Release");
    }

    private void UpdateJoystick(PointerEventData eventData) {
        Vector2 pointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerPos
        );

        Vector2 clamped = Vector2.ClampMagnitude(pointerPos, maxRadius);
        handle.anchoredPosition = clamped;

        JoyAxis.Axis = clamped / maxRadius;
        Debug.Log($"Joystick Input: {JoyAxis.Axis} ");
    }

    private void OnDisable() {//like user release joystick (in case when joystick immediately disabled)
        JoyAxis.Axis = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}

/// <summary>
/// Клас що зберігає значення осей джойстика, автоматично сигналізує подією про зміну значення. 
/// class that stores joystick axis values and automatically signals an event when the value changes.
/// </summary>
public static class JoyAxis {
    private static Vector2 axis { get; set; } = Vector2.zero;
    public static Vector2 Axis {
        get {
            return axis;
        }
        set {
            axis = value;
            OnAxisChanged?.Invoke(value);
        }
    }
    /// <summary>
    /// reset dragging etc events
    /// </summary>
    public static bool isReset { get; set; } = false;
    public static event Action<Vector2> OnAxisChanged;
}
