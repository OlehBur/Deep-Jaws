using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class GrowthBarView : MonoBehaviour {
    [field: SerializeField] public Transform Bar { get; private set; }
    [field: SerializeField] public Image MackerelIco { get; private set; }
    [field: SerializeField] public Image MorayIco { get; private set; }
    [field: SerializeField] public Image LionfishIco { get; private set; }
    [field: SerializeField] public Image BarracudaIco { get; private set; }
    [field: SerializeField] public Image TunaIco { get; private set; }
    [field: SerializeField] public Image HammerheadlIco { get; private set; }
    [field: SerializeField] public Image TigerIco { get; private set; }
    [field: SerializeField] public Image WhiteIco { get; private set; }
    [field: SerializeField] public Image MackarelProgress { get; private set; }
    [field: SerializeField] public Image MorayProgress { get; private set; }
    [field: SerializeField] public Image LionfishProgress { get; private set; }
    [field: SerializeField] public Image BarracudaProgress { get; private set; }
    [field: SerializeField] public Image TunaProgress { get; private set; }
    [field: SerializeField] public Image HammerheadlProgress { get; private set; }
    [field: SerializeField] public Image TigerProgress { get; private set; }
    [field: SerializeField] public Image WhiteProgress { get; private set; }
    [field: SerializeField] public TextMeshProUGUI Progress { get; private set; }
    [field: SerializeField] public TextMeshProUGUI ProgressPercent { get; private set; }

    private Coroutine _coroutine;

    private const float MAX_TEXT_SIZE = 1.2F;
    private const float DEFAULT_TEXT_SIZE = 1f;


    public void UpdateProgressInfo(int lvlProgress, Fish playerFish) {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        if (gameObject.activeInHierarchy)
            _coroutine = StartCoroutine(Animation());
        else
            ChangeTransparancy(SettingsUI.IsAutoHideUI ? 0f : 1f);

        Progress.text = lvlProgress.ToString();
        GetProgress(playerFish.Level).fillAmount = playerFish.Progress;
    }

    IEnumerator Animation() {
        try {
            float timer = PlayerUIController.VIEW_HUD_TIME;
            float scaleUpTime = timer * .7f;
            float scaleDownTime = timer * .3f;

            Progress.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
            ProgressPercent.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
            ChangeTransparancy(1f);
            while (timer > 0) {
                timer -= DJTime.UnscaledDeltaTime;

                if (timer < scaleUpTime) {
                    float delta = timer / scaleUpTime /*/ PlayerUIController.VIEW_HUD_TIME*/;
                    Progress.transform.localScale = Vector3.one * Mathf.Lerp(DEFAULT_TEXT_SIZE, MAX_TEXT_SIZE, delta);
                    ProgressPercent.transform.localScale = Vector3.one * Mathf.Lerp(DEFAULT_TEXT_SIZE, MAX_TEXT_SIZE, delta);
                }
                else if (timer > scaleDownTime) {
                    float delta = (timer - scaleUpTime) / (scaleDownTime - scaleUpTime) /*/ PlayerUIController.VIEW_HUD_TIME*/;
                    Progress.transform.localScale = Vector3.one * Mathf.Lerp(MAX_TEXT_SIZE, DEFAULT_TEXT_SIZE, delta);
                    ProgressPercent.transform.localScale = Vector3.one * Mathf.Lerp(MAX_TEXT_SIZE, DEFAULT_TEXT_SIZE, delta);
                }

                yield return null;
            }
            Progress.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
            ProgressPercent.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;

            timer = PlayerUIController.FADE_HUD_TIME;
            while (timer > 0) {
                timer -= DJTime.UnscaledDeltaTime;

                if (SettingsUI.IsAutoHideUI)
                    ChangeTransparancy(timer / PlayerUIController.FADE_HUD_TIME);
                yield return null;
            }
        }
        finally {
            Progress.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
            ProgressPercent.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;

            if (SettingsUI.IsAutoHideUI)
                ChangeTransparancy(0f);
        }
    }

    public void ActivateIcon(FishLevel index) {
        MackerelIco.gameObject.SetActive(false);
        MorayIco.gameObject.SetActive(false);
        LionfishIco.gameObject.SetActive(false);
        BarracudaIco.gameObject.SetActive(false);
        TunaIco.gameObject.SetActive(false);
        HammerheadlIco.gameObject.SetActive(false);
        TigerIco.gameObject.SetActive(false);
        WhiteIco.gameObject.SetActive(false);

        GetIcon(index).SetActive(true);
    }

    public GameObject GetIcon(FishLevel index) =>
        index switch {
            FishLevel.SeaBass => MackerelIco.gameObject,
            FishLevel.Mackerel => MorayIco.gameObject,
            FishLevel.Moray => LionfishIco.gameObject,
            FishLevel.Lionfish => BarracudaIco.gameObject,
            FishLevel.Barracuda => TunaIco.gameObject,
            FishLevel.Tuna => HammerheadlIco.gameObject,
            FishLevel.HammerShark => TigerIco.gameObject,
            FishLevel.TigerShark => WhiteIco.gameObject,
            _ => WhiteIco.gameObject
        };

    public Image GetProgress(FishLevel index) =>
    index switch {
        FishLevel.SeaBass => MackarelProgress,
        FishLevel.Mackerel => MorayProgress,
        FishLevel.Moray => LionfishProgress,
        FishLevel.Lionfish => BarracudaProgress,
        FishLevel.Barracuda => TunaProgress,
        FishLevel.Tuna => HammerheadlProgress,
        FishLevel.HammerShark => TigerProgress,
        FishLevel.TigerShark => WhiteProgress,
        _ => WhiteProgress
    };

    public void ChangeTransparancy(float a) {
        MackerelIco.color = new Color(MackerelIco.color.r, MackerelIco.color.g, MackerelIco.color.b, a);
        MorayIco.color = new Color(MorayIco.color.r, MorayIco.color.g, MorayIco.color.b, a);
        LionfishIco.color = new Color(LionfishIco.color.r, LionfishIco.color.g, LionfishIco.color.b, a);
        BarracudaIco.color = new Color(BarracudaIco.color.r, BarracudaIco.color.g, BarracudaIco.color.b, a);
        TunaIco.color = new Color(TunaIco.color.r, TunaIco.color.g, TunaIco.color.b, a);
        HammerheadlIco.color = new Color(HammerheadlIco.color.r, HammerheadlIco.color.g, HammerheadlIco.color.b, a);
        TigerIco.color = new Color(TigerIco.color.r, TigerIco.color.g, TigerIco.color.b, a);
        WhiteIco.color = new Color(WhiteIco.color.r, WhiteIco.color.g, WhiteIco.color.b, a);
        MackarelProgress.color = new Color(MackarelProgress.color.r, MackarelProgress.color.g, MackarelProgress.color.b, a);
        MorayProgress.color = new Color(MorayProgress.color.r, MorayProgress.color.g, MorayProgress.color.b, a);
        LionfishProgress.color = new Color(LionfishProgress.color.r, LionfishProgress.color.g, LionfishProgress.color.b, a);
        BarracudaProgress.color = new Color(BarracudaProgress.color.r, BarracudaProgress.color.g, BarracudaProgress.color.b, a);
        TunaProgress.color = new Color(TunaProgress.color.r, TunaProgress.color.g, TunaProgress.color.b, a);
        HammerheadlProgress.color = new Color(HammerheadlProgress.color.r, HammerheadlProgress.color.g, HammerheadlProgress.color.b, a);
        TigerProgress.color = new Color(TigerProgress.color.r, TigerProgress.color.g, TigerProgress.color.b, a);
        WhiteProgress.color = new Color(WhiteProgress.color.r, WhiteProgress.color.g, WhiteProgress.color.b, a);
        Progress.color = new Color(Progress.color.r, Progress.color.g, Progress.color.b, a);
        ProgressPercent.color = new Color(ProgressPercent.color.r, ProgressPercent.color.g, ProgressPercent.color.b, a);
    }
}
