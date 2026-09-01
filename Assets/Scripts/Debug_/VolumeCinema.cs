using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class VolumeCinema : MonoBehaviour {
    [SerializeField] Volume mainVolume;
    [SerializeField] Image flash;

    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    private DepthOfField depthOfField;

    private void Awake() {
        if (mainVolume.sharedProfile.TryGet(out LensDistortion lensDistortion)) {
            this.lensDistortion = lensDistortion;
            lensDistortion.intensity.value = -1;
        }
        if (mainVolume.sharedProfile.TryGet(out ColorAdjustments colorAdjustments)) {
            this.colorAdjustments = colorAdjustments;
            colorAdjustments.postExposure.value = 0;
        }
        if (mainVolume.sharedProfile.TryGet(out DepthOfField depthOfField)) {
            this.depthOfField = depthOfField;
            depthOfField.gaussianMaxRadius.value = 1.5f;
        }
    }

    // Update is called once per frame
    void Update() {
        if (lensDistortion != null && lensDistortion.intensity.value < 0) {
            lensDistortion.intensity.value += DJTime.DeltaTime;
            if (lensDistortion.intensity.value > 0)
                lensDistortion.intensity.value = 0;
        }
    }

    public void OnStartWhiteFlash() {
        StartCoroutine(FlashAnim());
    }


    public void OnStartUnblur() {
        StartCoroutine(UnBlur());
    }

    IEnumerator UnBlur() {
        //const float defaultRadius = .5f;
        float secondTime = 0;
        while (secondTime < 1) {
            secondTime += Time.deltaTime;
            depthOfField.gaussianMaxRadius.value -= Time.deltaTime;
            yield return null;
        }
        depthOfField.gaussianMaxRadius.value = 0.5f;
    }
     
    IEnumerator FlashAnim() {
        float secondTime = 0;
        Color buffer = Color.white;
        while (secondTime < 1) {
            secondTime += Time.deltaTime;
            colorAdjustments.postExposure.value = Mathf.Lerp(0, 10, secondTime);
            buffer.a = secondTime;
            flash.color = buffer;
            yield return null;
        }
        colorAdjustments.postExposure.value = 10;
        flash.color = Color.white;
    }

    private void OnDisable() {
        if (lensDistortion != null)
            lensDistortion.intensity.value = 0;
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = 0;
        if (depthOfField != null)
            depthOfField.gaussianMaxRadius.value = 1.5f;
    }
}
