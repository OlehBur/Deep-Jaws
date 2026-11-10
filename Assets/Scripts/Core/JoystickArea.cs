using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class JoystickArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
    [SerializeField] RectTransform joystickBackgr;
    [SerializeField] RectTransform handle;
    [SerializeField] RectTransform touchArea;
    [SerializeField] float maxRadius = 100f;


    void Awake() {
        SettingsWnd.OnHudHideChanged += SetJoystickHide;
    }

    private void Start() {
        SetJoystickHide();
    }

    void SetJoystickHide() {
        if (joystickBackgr != null)
            joystickBackgr.gameObject.SetActive(!SettingsWnd.IsAutoHideUI);
    }

    public void OnPointerDown(PointerEventData eventData) {
        //convert click pos to area pos
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            touchArea,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        //center to click pos
        joystickBackgr.anchoredPosition = localPoint;
        if (SettingsWnd.IsAutoHideUI)
            joystickBackgr.gameObject.SetActive(true);

        //reset handler
        handle.anchoredPosition = Vector2.zero;
        JoyAxis.Axis = Vector2.zero;

        //UpdateJoystick(eventData);
        //Debug.Log("Click");
    }

    public void OnDrag(PointerEventData eventData) {
        //cals fing offset relat to new joystick center
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackgr,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 handlePos
        );

        Vector2 clamped = Vector2.ClampMagnitude(handlePos, maxRadius);
        handle.anchoredPosition = clamped;

        JoyAxis.Axis = clamped / maxRadius;

        if (JoyAxis.isReset) {
            eventData.pointerDrag = null;
            JoyAxis.isReset = false;
        }

        //UpdateJoystick(eventData);
        //Debug.Log("Drag");
    }

    public void OnPointerUp(PointerEventData eventData) {
        JoyAxis.Axis = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;

        if (SettingsWnd.IsAutoHideUI)
            joystickBackgr.gameObject.SetActive(false);

        //Debug.Log("Release");
    }

    //private void UpdateJoystick(PointerEventData eventData) {
    //    Vector2 pointerPos;
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //        transform as RectTransform,
    //        eventData.position,
    //        eventData.pressEventCamera,
    //        out pointerPos
    //    );

    //    Vector2 clamped = Vector2.ClampMagnitude(pointerPos, maxRadius);
    //    handle.anchoredPosition = clamped;

    //    JoyAxis.Axis = clamped / maxRadius;
    //    //Debug.Log($"Joystick Input: {JoyAxis.Axis} ");
    //}

    private void OnDisable() {//like user release joystick (in case when joystick immediately disabled)
        JoyAxis.Axis = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        if (SettingsWnd.IsAutoHideUI)
            joystickBackgr.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        SettingsWnd.OnHudHideChanged -= SetJoystickHide;
    }
}

/// <summary>
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
