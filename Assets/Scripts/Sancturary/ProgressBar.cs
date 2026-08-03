using TMPro;
using UnityEngine;

public class ProgressBar : MonoBehaviour {
    [SerializeField] RectTransform fillRect;
    [SerializeField] RectTransform parentRect;
    [SerializeField] TextMeshProUGUI progressLabel;
    [SerializeField][Range(0f, 1f)] float progress;

    private const float MIN_PROGRESSS_FILL = 30f;


    //private void OnValidate() {
    //    if (fillRect == null || parentRect == null)
    //        return;

    //    SetFillAmount(progress);
    //}

    public void SetFillAmount(float fillAmount) {
        fillAmount = Mathf.Clamp01(fillAmount);

        float parentWidth = parentRect.rect.width;
        float targetRight = Mathf.Lerp(parentWidth - MIN_PROGRESSS_FILL, 0f, fillAmount);

        fillRect.offsetMax = new Vector2(-targetRight, fillRect.offsetMax.y);//right
    }

    public void SetText(string text) =>
        progressLabel.text = text;
}
