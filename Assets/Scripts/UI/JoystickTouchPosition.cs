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
        //  if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) //is screen touched
        //{
        //      if (RectTransformUtility.ScreenPointToLocalPointInRectangle(currentRect, Mouse.current.position.ReadValue(), null, out Vector2 localPoint)
        //          //&& Vector2.Distance(Mouse.current.position.ReadValue(), stickHandlerParentRect.rect.position) <= stickHandler.dynamicOriginRange
        //          ) {

        //          handleRectBuffer = stickHandlerRect.rect;
        //          joystickBackground.anchoredPosition = localPoint - joystickBackground.rect.size * .5f - stickHandlerRect.rect.size;
        //      }
        //  }
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

    }
#endif

    //public void OnPointerDown(PointerEventData eventData) {
    //    if (handleRectBuffer != stickHandler.rect &&
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, null, out Vector2 localPoint)) {

    //        handleRectBuffer = stickHandler.rect;
    //        joystickBackground.anchoredPosition = localPoint;
    //        //Debug.Log($"Joystick position set to: {joystick.anchoredPosition}");

    //        //ExecuteEvents.Execute<IPointerDownHandler>(//immitate pointer down event
    //        //    stickHandler,
    //        //    eventData,
    //        //    ExecuteEvents.pointerDownHandler
    //        //);

    //    }
    //}
}

