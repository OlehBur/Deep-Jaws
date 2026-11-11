using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SprintArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] RectTransform area;
    [SerializeField] RectTransform sprintButtonRect;
    [SerializeField] Button sprintB;

    void Awake() {
        SettingsWnd.OnHudHideChanged += SetSprintHide;
    }

    private void Start() {
        SetSprintHide();
    }

    void SetSprintHide() {
        if (sprintB != null)
            sprintB.gameObject.SetActive(!SettingsWnd.IsAutoHideUI);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (GameStaticInfo.IsRunningOnPlayGamesPC()) {
            //convert click pos to area pos
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            area,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

            //center to click pos
            sprintButtonRect.anchoredPosition = localPoint;

            if (sprintB.interactable)
                sprintB.onClick.Invoke();

            if (SettingsWnd.IsAutoHideUI)
                sprintB.gameObject.SetActive(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (GameStaticInfo.IsRunningOnPlayGamesPC()) {
            if (SettingsWnd.IsAutoHideUI)
                sprintB.gameObject.SetActive(false);
        }
    }

    private void OnDisable() {
        if (SettingsWnd.IsAutoHideUI)
            sprintB.gameObject.SetActive(true);
    }

    private void OnDestroy() {
        SettingsWnd.OnHudHideChanged -= SetSprintHide;
    }
}
