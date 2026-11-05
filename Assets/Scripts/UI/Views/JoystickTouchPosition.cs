using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;


[RequireComponent(typeof(RectTransform))]
public class JoystickTouchPosition : MonoBehaviour/*, IPointerClickHandler,*/ /*IPointerDownHandler*/ {
    [SerializeField] RectTransform joystickBackground;
    [SerializeField] OnScreenStick stickHandler;
    [SerializeField] RectTransform stickHandlerRect;
    [SerializeField] RectTransform stickHandlerParentRect;
    private Rect handleRectBuffer;
    private RectTransform currentRect;
    private Vector2 initialJoystickCenter;


    private void Awake() {
        handleRectBuffer = stickHandlerRect.rect;
        currentRect = GetComponent<RectTransform>();
        initialJoystickCenter = joystickBackground.anchoredPosition;
    }

    private void Update() {
#if UNITY_EDITOR
        if (Mouse.current != null) {
            // get the world position of the stick center
            Vector3 stickWorldCenter = stickHandlerParentRect.TransformPoint(stickHandlerRect.localPosition);
            // project to screen coordinates
            Vector2 stickScreenCenter = RectTransformUtility.WorldToScreenPoint(null, stickWorldCenter);

            if (Mouse.current.leftButton.wasPressedThisFrame) {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                float distance = Vector2.Distance(screenPos, stickScreenCenter);

                if (distance < stickHandler.dynamicOriginRange) {
                    // trasform the screen position to local position in the currentRect
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(currentRect, screenPos, null, out Vector2 localPoint)) {
                        joystickBackground.anchoredPosition = localPoint - currentRect.sizeDelta * .5f;
                        handleRectBuffer = stickHandlerRect.rect;
                    }
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
                joystickBackground.anchoredPosition = initialJoystickCenter;
        }
#endif
        /*#elif*/
#if UNITY_ANDROID
        if (Touchscreen.current != null) {
            // Отримаємо світову позицію центра стіка
            Vector3 stickWorldCenter = stickHandlerParentRect.TransformPoint(stickHandlerRect.localPosition);
            // Проєктуємо в екранні координати
            Vector2 stickScreenCenter = RectTransformUtility.WorldToScreenPoint(null, stickWorldCenter);

            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame) {
                Vector2 screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                float distance = Vector2.Distance(screenPos, stickScreenCenter);

                if (distance < stickHandler.dynamicOriginRange) {
                    // Переводимо screenPos у локальні координати області натиску (currentRect)
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(currentRect, screenPos, null, out Vector2 localPoint)) {
                        joystickBackground.anchoredPosition = localPoint - currentRect.sizeDelta * .5f;
                        handleRectBuffer = stickHandlerRect.rect;
                    }
                }
            }
            else if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
                joystickBackground.anchoredPosition = initialJoystickCenter;
        }
#endif
    }


}

