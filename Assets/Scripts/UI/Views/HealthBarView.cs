using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarView : MonoBehaviour {
    private const float HEART_ADD_SCALE_MULT = .2f;
    private const float HEART_BEAT_FREQ = 4f;

    [field: SerializeField] public Image HealthBar { get; private set; }
    [field: SerializeField] public Image Filler { get; private set; }
    [field: SerializeField] public TextMeshProUGUI CurrProgress { get; private set; }
    [field: SerializeField] public TextMeshProUGUI MaxProgress { get; private set; }
    [field: SerializeField] public TextMeshProUGUI SlashTxt { get; private set; }

    public Coroutine Coroutine { get; set; }

    public IEnumerator Animation(float healthProg) {
        try {
            float timer = PlayerUIController.VIEW_HEALTH_TIME;
            float heartBeatSpeed;
            float scaleNoiseMult;
            float heartBeatFreqMult = HEART_BEAT_FREQ * (2f - healthProg);

            ChangeTransparancy(1f);
            while (timer > 0) {
                timer -= DJTime.UnscaledDeltaTime;
                heartBeatSpeed = Time.time * heartBeatFreqMult;
                scaleNoiseMult = Mathf.PerlinNoise(heartBeatSpeed, 0) * HEART_ADD_SCALE_MULT;
                HealthBar.transform.localScale = Vector3.one + Vector3.one * scaleNoiseMult;
                yield return null;
            }

            timer = PlayerUIController.VIEW_HEALTH_TIME;
            while (timer > 0) {
                timer -= DJTime.UnscaledDeltaTime;
                heartBeatSpeed = Time.time * heartBeatFreqMult;
                scaleNoiseMult = Mathf.PerlinNoise(heartBeatSpeed, 0) * HEART_ADD_SCALE_MULT;
                HealthBar.transform.localScale = Vector3.one + Vector3.one * scaleNoiseMult;

                if (SettingsUI.IsAutoHideUI)
                    ChangeTransparancy(timer / PlayerUIController.FADE_HUD_TIME);
                yield return null;
            }
        }
        finally {
            HealthBar.transform.localScale = Vector3.one;

            if (SettingsUI.IsAutoHideUI)
                ChangeTransparancy(0f);
        }
    }

    public void ChangeTransparancy(float a) {
        HealthBar.color = new Color(HealthBar.color.r, HealthBar.color.g, HealthBar.color.b, a);
        Filler.color = new Color(Filler.color.r, Filler.color.g, Filler.color.b, a);
        CurrProgress.color = new Color(CurrProgress.color.r, CurrProgress.color.g, CurrProgress.color.b, a);
        MaxProgress.color = new Color(MaxProgress.color.r, MaxProgress.color.g, MaxProgress.color.b, a);
        SlashTxt.color = new Color(SlashTxt.color.r, SlashTxt.color.g, SlashTxt.color.b, a);
    }
}
