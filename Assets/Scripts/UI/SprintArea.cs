using PlasticGui.WorkspaceWindow.NotificationBar;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SprintArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] RectTransform area;
    [SerializeField] RectTransform sprintButtonRect;
    [SerializeField] Button sprintB;

    void Awake() {
        SettingsUI.OnHudHideChanged += SetSprintHide;
    }

    private void Start() {
        SetSprintHide();
    }

    void SetSprintHide() {
        if (sprintB != null)
            sprintB.gameObject.SetActive(!SettingsUI.IsAutoHideUI);
    }

    public void OnPointerDown(PointerEventData eventData) {
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

        if (SettingsUI.IsAutoHideUI)
            sprintB.gameObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (SettingsUI.IsAutoHideUI)
            sprintB.gameObject.SetActive(false);
    }

    private void OnDisable() {
        if (SettingsUI.IsAutoHideUI)
            sprintB.gameObject.SetActive(true);
    }

    private void OnDestroy() {
        SettingsUI.OnHudHideChanged -= SetSprintHide;
    }
}
