using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class PlayerGrowthBar {
    //private const float TRANSFORM_MIN_SCALE_MULT = .5f;
    //private const float TRANSFORM_DEFAULT_SCALE_MULT = 1f;
    private const float MAX_TEXT_SIZE = 1.2F;
    private const float DEFAULT_TEXT_SIZE = 1f;

    public Transform bar;
    public Image mackerelIco;
    public Image morayIco;
    public Image lionfishIco;
    public Image barracudaIco;
    public Image tunaIco;
    public Image hammerheadlIco;
    public Image tigerIco;
    public Image whiteIco;

    public Image mackarelProgress;
    public Image morayProgress;
    public Image lionfishProgress;
    public Image barracudaProgress;
    public Image tunaProgress;
    public Image hammerheadlProgress;
    public Image tigerProgress;
    public Image whiteProgress;

    public TextMeshProUGUI progress;
    public TextMeshProUGUI progressPercent;

    public Coroutine Coroutine { get; set; }

    public IEnumerator Animation() {
        float timer = PlayerUIController.VIEW_HUD_TIME;
        float scaleUpTime = timer * .7f;
        float scaleDownTime = timer * .3f;
        //bar.localScale = Vector3.one * TRANSFORM_MIN_SCALE_MULT;

        progress.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
        progressPercent.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
        ChangeTransparancy(1f);
        while (timer > 0) {
            timer -= DJTime.UnscaledDeltaTime;

            if (timer < scaleUpTime) {
                float delta = timer / scaleUpTime /*/ PlayerUIController.VIEW_HUD_TIME*/;
                progress.transform.localScale = Vector3.one * Mathf.Lerp(DEFAULT_TEXT_SIZE, MAX_TEXT_SIZE, delta);
                progressPercent.transform.localScale = Vector3.one * Mathf.Lerp(DEFAULT_TEXT_SIZE, MAX_TEXT_SIZE, delta);
            }
            else if (timer > scaleDownTime) {
                float delta = (timer - scaleUpTime) / (scaleDownTime - scaleUpTime) /*/ PlayerUIController.VIEW_HUD_TIME*/;
                progress.transform.localScale = Vector3.one * Mathf.Lerp(MAX_TEXT_SIZE, DEFAULT_TEXT_SIZE, delta);
                progressPercent.transform.localScale = Vector3.one * Mathf.Lerp(MAX_TEXT_SIZE, DEFAULT_TEXT_SIZE, delta);
            }

            yield return null;
        }
        progress.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
        progressPercent.transform.localScale = Vector3.one * DEFAULT_TEXT_SIZE;
        //bar.localScale = Vector3.one * TRANSFORM_DEFAULT_SCALE_MULT;

        timer = PlayerUIController.FADE_HUD_TIME;
        while (timer > 0) {
            timer -= DJTime.UnscaledDeltaTime;

            if (SettingsUI.IsAutoHideUI)
                ChangeTransparancy(timer / PlayerUIController.FADE_HUD_TIME);
            yield return null;
        }

        if (SettingsUI.IsAutoHideUI)
            ChangeTransparancy(0f);
    }

    public void ActivateIcon(FishLevel index) {
        mackerelIco.gameObject.SetActive(false);
        morayIco.gameObject.SetActive(false);
        lionfishIco.gameObject.SetActive(false);
        barracudaIco.gameObject.SetActive(false);
        tunaIco.gameObject.SetActive(false);
        hammerheadlIco.gameObject.SetActive(false);
        tigerIco.gameObject.SetActive(false);
        whiteIco.gameObject.SetActive(false);

        GetIcon(index).SetActive(true);
    }

    public GameObject GetIcon(FishLevel index) =>
        index switch {
            FishLevel.SeaBass => mackerelIco.gameObject,
            FishLevel.Mackerel => morayIco.gameObject,
            FishLevel.Moray => lionfishIco.gameObject,
            FishLevel.Lionfish => barracudaIco.gameObject,
            FishLevel.Barracuda => tunaIco.gameObject,
            FishLevel.Tuna => hammerheadlIco.gameObject,
            FishLevel.HammerShark => tigerIco.gameObject,
            FishLevel.TigerShark => whiteIco.gameObject,
            _ => whiteIco.gameObject
        };

    public Image GetProgress(FishLevel index) =>
    index switch {
        FishLevel.SeaBass => mackarelProgress,
        FishLevel.Mackerel => morayProgress,
        FishLevel.Moray => lionfishProgress,
        FishLevel.Lionfish => barracudaProgress,
        FishLevel.Barracuda => tunaProgress,
        FishLevel.Tuna => hammerheadlProgress,
        FishLevel.HammerShark => tigerProgress,
        FishLevel.TigerShark => whiteProgress,
        _ => whiteProgress
    };

    public void ChangeTransparancy(float a) {
        mackerelIco.color = new Color(mackerelIco.color.r, mackerelIco.color.g, mackerelIco.color.b, a);
        morayIco.color = new Color(morayIco.color.r, morayIco.color.g, morayIco.color.b, a);
        lionfishIco.color = new Color(lionfishIco.color.r, lionfishIco.color.g, lionfishIco.color.b, a);
        barracudaIco.color = new Color(barracudaIco.color.r, barracudaIco.color.g, barracudaIco.color.b, a);
        tunaIco.color = new Color(tunaIco.color.r, tunaIco.color.g, tunaIco.color.b, a);
        hammerheadlIco.color = new Color(hammerheadlIco.color.r, hammerheadlIco.color.g, hammerheadlIco.color.b, a);
        tigerIco.color = new Color(tigerIco.color.r, tigerIco.color.g, tigerIco.color.b, a);
        whiteIco.color = new Color(whiteIco.color.r, whiteIco.color.g, whiteIco.color.b, a);
        mackarelProgress.color = new Color(mackarelProgress.color.r, mackarelProgress.color.g, mackarelProgress.color.b, a);
        morayProgress.color = new Color(morayProgress.color.r, morayProgress.color.g, morayProgress.color.b, a);
        lionfishProgress.color = new Color(lionfishProgress.color.r, lionfishProgress.color.g, lionfishProgress.color.b, a);
        barracudaProgress.color = new Color(barracudaProgress.color.r, barracudaProgress.color.g, barracudaProgress.color.b, a);
        tunaProgress.color = new Color(tunaProgress.color.r, tunaProgress.color.g, tunaProgress.color.b, a);
        hammerheadlProgress.color = new Color(hammerheadlProgress.color.r, hammerheadlProgress.color.g, hammerheadlProgress.color.b, a);
        tigerProgress.color = new Color(tigerProgress.color.r, tigerProgress.color.g, tigerProgress.color.b, a);
        whiteProgress.color = new Color(whiteProgress.color.r, whiteProgress.color.g, whiteProgress.color.b, a);
        progress.color = new Color(progress.color.r, progress.color.g, progress.color.b, a);
        progressPercent.color = new Color(progressPercent.color.r, progressPercent.color.g, progressPercent.color.b, a);
    }
}

[System.Serializable]
public class HealthBar {
    private const float HEART_ADD_SCALE_MULT = .2f;
    private const float HEART_BEAT_FREQ = 4f;

    public Image/*GameObject*/ healthBar;
    public Image filler;
    public TextMeshProUGUI currProgress;
    public TextMeshProUGUI maxProgress;
    public TextMeshProUGUI slashTxt;

    public Coroutine Coroutine { get; set; }

    public IEnumerator Animation(float healthProg) {
        float timer = PlayerUIController.VIEW_HEALTH_TIME;
        float heartBeatSpeed;
        float scaleNoiseMult;
        float heartBeatFreqMult = HEART_BEAT_FREQ * (2f - healthProg);

        ChangeTransparancy(1f);
        while (timer > 0) {
            timer -= DJTime.UnscaledDeltaTime;
            heartBeatSpeed = Time.time * heartBeatFreqMult;
            scaleNoiseMult = Mathf.PerlinNoise(heartBeatSpeed, 0) * HEART_ADD_SCALE_MULT;
            healthBar.transform.localScale = Vector3.one + Vector3.one * scaleNoiseMult;
            yield return null;
        }

        timer = PlayerUIController.VIEW_HEALTH_TIME;
        while (timer > 0) {
            timer -= DJTime.UnscaledDeltaTime;
            heartBeatSpeed = Time.time * heartBeatFreqMult;
            scaleNoiseMult = Mathf.PerlinNoise(heartBeatSpeed, 0) * HEART_ADD_SCALE_MULT;
            healthBar.transform.localScale = Vector3.one + Vector3.one * scaleNoiseMult;

            if (SettingsUI.IsAutoHideUI)
                ChangeTransparancy(timer / PlayerUIController.FADE_HUD_TIME);
            yield return null;
        }

        if (SettingsUI.IsAutoHideUI)
            ChangeTransparancy(0f);
    }

    public void ChangeTransparancy(float a) {
        healthBar.color = new Color(healthBar.color.r, healthBar.color.g, healthBar.color.b, a);
        filler.color = new Color(filler.color.r, filler.color.g, filler.color.b, a);
        currProgress.color = new Color(currProgress.color.r, currProgress.color.g, currProgress.color.b, a);
        maxProgress.color = new Color(maxProgress.color.r, maxProgress.color.g, maxProgress.color.b, a);
        slashTxt.color = new Color(slashTxt.color.r, slashTxt.color.g, slashTxt.color.b, a);
    }
}

[System.Serializable]
public class ScoreBar {
    private const float MAX_JAW_Y_SCALE = 2.4f;
    private const float MIN_JAW_Y_SCALE = 1f;
    private const float DEFAULT_JAW_SCALE = 2f;
    private const float DEFAULT_TEXT_SCALE = 1f;
    private const float MAX_TEXT_SCALE = 1.3f;
    private const float MIN_TEXT_SCALE = .5f;

    public Image ico;
    public TextMeshProUGUI score;

    public Coroutine Coroutine { get; set; }

    public IEnumerator Animation() {
        float timer = 0;
        float jawOpenTime = PlayerUIController.VIEW_JAW_TIME * .3f;
        float jawCloseTime = PlayerUIController.VIEW_JAW_TIME * .6f;
        float t, newYscale;

        ChangeTransparancy(1f);
        //Jaw open
        ico.transform.localScale = Vector3.one * DEFAULT_JAW_SCALE;
        score.transform.localScale = Vector3.one * DEFAULT_TEXT_SCALE;
        while (timer < jawOpenTime) {
            timer += DJTime.UnscaledDeltaTime;

            //if (ico.transform.localScale.y < MAX_JAW_Y_SCALE) {
            t = timer / jawOpenTime;
            newYscale = Mathf.Lerp(DEFAULT_JAW_SCALE, MAX_JAW_Y_SCALE, t);
            //Debug.Log("open delta: " + (timer - halfTime) / halfTime);
            ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, newYscale, DEFAULT_JAW_SCALE);
            score.transform.localScale = Vector3.one * Mathf.Lerp(DEFAULT_TEXT_SCALE, MAX_TEXT_SCALE, t);
            //}
            yield return null;
        }
        timer = jawOpenTime;
        ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, MAX_JAW_Y_SCALE, DEFAULT_JAW_SCALE);
        score.transform.localScale = Vector3.one * MAX_TEXT_SCALE;

        //Jaw close
        while (timer < jawCloseTime) {
            timer += DJTime.UnscaledDeltaTime;

            //if (ico.transform.localScale.y > MIN_JAW_Y_SCALE) {
            t = (timer - jawOpenTime) / (jawCloseTime - jawOpenTime);
            newYscale = Mathf.Lerp(MAX_JAW_Y_SCALE, MIN_JAW_Y_SCALE, t);
            //Debug.Log("close delta: " + timer / halfTime);
            ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, newYscale, DEFAULT_JAW_SCALE);
            score.transform.localScale = Vector3.one * Mathf.Lerp(MAX_TEXT_SCALE, MIN_TEXT_SCALE, t);
            //}
            yield return null;
        }
        timer = jawCloseTime;
        ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, MIN_JAW_Y_SCALE, DEFAULT_JAW_SCALE);
        score.transform.localScale = Vector3.one * MIN_TEXT_SCALE;

        //jaw return 
        while (timer < PlayerUIController.VIEW_JAW_TIME) {
            timer += DJTime.UnscaledDeltaTime;
            if (ico.transform.localScale.y < DEFAULT_JAW_SCALE) {
                t = (timer - jawCloseTime) / (PlayerUIController.VIEW_JAW_TIME - jawCloseTime);
                newYscale = Mathf.Lerp(MIN_JAW_Y_SCALE, DEFAULT_JAW_SCALE, t);
                ico.transform.localScale = new Vector3(DEFAULT_JAW_SCALE, newYscale, DEFAULT_JAW_SCALE);
                score.transform.localScale = Vector3.one * Mathf.Lerp(MIN_TEXT_SCALE, DEFAULT_TEXT_SCALE, t);
            }
            yield return null;
        }
        ico.transform.localScale = Vector3.one * DEFAULT_JAW_SCALE;
        score.transform.localScale = Vector3.one * DEFAULT_TEXT_SCALE;

        //FADE
        timer = PlayerUIController.FADE_HUD_TIME;
        while (timer > 0) {
            timer -= DJTime.UnscaledDeltaTime;

            if (SettingsUI.IsAutoHideUI)
                ChangeTransparancy(timer / PlayerUIController.FADE_HUD_TIME);
            yield return null;
        }

        if (SettingsUI.IsAutoHideUI)
            ChangeTransparancy(0f);
    }

    public void ChangeTransparancy(float a) {
        ico.color = new Color(ico.color.r, ico.color.g, ico.color.b, a);
        score.color = new Color(score.color.r, score.color.g, score.color.b, a);
    }
}