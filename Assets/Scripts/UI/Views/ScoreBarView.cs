using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class ScoreBarView : MonoBehaviour {
    [field: SerializeField] public Image Ico { get; private set; }
    [field: SerializeField] public TextMeshProUGUI Score { get; private set; }

    private const float MAX_JAW_Y_SCALE = 2.4f;
    private const float MIN_JAW_Y_SCALE = 1f;
    private const float DEFAULT_JAW_SCALE = 2f;
    private const float DEFAULT_TEXT_SCALE = 1f;
    private const float MAX_TEXT_SCALE = 1.3f;
    private const float MIN_TEXT_SCALE = .5f;

    private Coroutine _coroutine;


    public void UpdateScoreInfo(long score) {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        if (gameObject == null)
            return;

        if (gameObject.activeInHierarchy)
            _coroutine = StartCoroutine(Animation());
        else
            ChangeTransparancy(SettingsWnd.IsAutoHideUI ? 0f : 1f);

        Score.text = score.ToString();
    }

    IEnumerator Animation() {
        try {
            float timer = 0;
            float jawOpenTime = PlayerUIController.VIEW_JAW_TIME * .3f;
            float jawCloseTime = PlayerUIController.VIEW_JAW_TIME * .6f;
            float t, newYscale;

            ChangeTransparancy(1f);
            //Jaw open
            Ico.transform.localScale = Vector3.one * DEFAULT_JAW_SCALE;
            Score.transform.localScale = Vector3.one * DEFAULT_TEXT_SCALE;
            while (timer < jawOpenTime) {
                timer += DJTime.UnscaledDeltaTime;

                //if (ico.transform.localScale.y < MAX_JAW_Y_SCALE) {
                t = timer / jawOpenTime;
                newYscale = Mathf.Lerp(DEFAULT_JAW_SCALE, MAX_JAW_Y_SCALE, t);
                //Debug.Log("open delta: " + (timer - halfTime) / halfTime);
                Ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, newYscale, DEFAULT_JAW_SCALE);
                Score.transform.localScale = Vector3.one * Mathf.Lerp(DEFAULT_TEXT_SCALE, MAX_TEXT_SCALE, t);
                //}
                yield return null;
            }
            timer = jawOpenTime;
            Ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, MAX_JAW_Y_SCALE, DEFAULT_JAW_SCALE);
            Score.transform.localScale = Vector3.one * MAX_TEXT_SCALE;

            //Jaw close
            while (timer < jawCloseTime) {
                timer += DJTime.UnscaledDeltaTime;

                //if (ico.transform.localScale.y > MIN_JAW_Y_SCALE) {
                t = (timer - jawOpenTime) / (jawCloseTime - jawOpenTime);
                newYscale = Mathf.Lerp(MAX_JAW_Y_SCALE, MIN_JAW_Y_SCALE, t);
                //Debug.Log("close delta: " + timer / halfTime);
                Ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, newYscale, DEFAULT_JAW_SCALE);
                Score.transform.localScale = Vector3.one * Mathf.Lerp(MAX_TEXT_SCALE, MIN_TEXT_SCALE, t);
                //}
                yield return null;
            }
            timer = jawCloseTime;
            Ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, MIN_JAW_Y_SCALE, DEFAULT_JAW_SCALE);
            Score.transform.localScale = Vector3.one * MIN_TEXT_SCALE;

            //jaw return 
            while (timer < PlayerUIController.VIEW_JAW_TIME) {
                timer += DJTime.UnscaledDeltaTime;
                if (Ico.transform.localScale.y < DEFAULT_JAW_SCALE) {
                    t = (timer - jawCloseTime) / (PlayerUIController.VIEW_JAW_TIME - jawCloseTime);
                    newYscale = Mathf.Lerp(MIN_JAW_Y_SCALE, DEFAULT_JAW_SCALE, t);
                    Ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, newYscale, DEFAULT_JAW_SCALE);
                    Score.transform.localScale = Vector3.one * Mathf.Lerp(MIN_TEXT_SCALE, DEFAULT_TEXT_SCALE, t);
                }
                yield return null;
            }
            Ico.transform.localScale = Vector3.one * DEFAULT_JAW_SCALE;
            Score.transform.localScale = Vector3.one * DEFAULT_TEXT_SCALE;

            //FADE
            timer = PlayerUIController.FADE_HUD_TIME;
            while (timer > 0) {
                timer -= DJTime.UnscaledDeltaTime;

                if (SettingsWnd.IsAutoHideUI)
                    ChangeTransparancy(timer / PlayerUIController.FADE_HUD_TIME);
                yield return null;
            }
        }
        finally {
            Ico.transform.localScale = Vector3.one * DEFAULT_JAW_SCALE;
            Score.transform.localScale = Vector3.one * DEFAULT_TEXT_SCALE;

            if (SettingsWnd.IsAutoHideUI)
                ChangeTransparancy(0f);
        }
    }

    public void ChangeTransparancy(float a) {
        Ico.color = new Color(Ico.color.r, Ico.color.g, Ico.color.b, a);
        Score.color = new Color(Score.color.r, Score.color.g, Score.color.b, a);
    }
}
