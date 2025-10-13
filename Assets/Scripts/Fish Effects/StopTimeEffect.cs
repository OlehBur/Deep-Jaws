
using UnityEngine;

public class StopTimeEffect : Effect {
    SkinnedMeshRenderer fishRenderer;
    MaterialPropertyBlock fishMatProp;
    Animator fishAnimator;//for freeze fish animation
    float currentTime = 0f;
    const float STOP_TIME_SCALE = .2f;
    const float STOPTIME_SPEED_MULTIPLIER = 5.4f;
    /// <summary>
    /// значення при якому шейдер починає знову брати значення скаледТайм замість нашого з ефекту 
    /// value we set when shader starts to take scaledTime value instead of our from effect
    /// </summary>
    const float STOP_VALUE_FOR_SHADER = 0f;
    //private bool isStartedDiactivation = false;

    public StopTimeEffect(SkinnedMeshRenderer fishWaveMat, Animator fishAnimator) {
        fishRenderer = fishWaveMat;
        this.fishAnimator = fishAnimator;
        type = FishEffectType.FreezeTime;
        ////Debug.LogWarning("Init StopTimeEffect");
       
        InitPropBlock();
    }

    private void InitPropBlock() {
        fishMatProp = new MaterialPropertyBlock();
        FishAnimManager.SetGetMaterialWithPropBlock(fishRenderer, ref fishMatProp, "_UnscaledTime", false, fishRenderer.sharedMaterial.GetFloat("_UnscaledTime")/*STOP_VALUE_FOR_SHADER*/);
    }

    protected override void OnApply() {
        //Debug.Log($"{target.name} has applied StopTime Effect for {Duration} s.");
        GameManager.ChangeGameTime(STOP_TIME_SCALE);
        GameManager.SetFixedDeltaTime(false, GameManager.currentFixedDeltaTime * STOP_TIME_SCALE);
        target.ChangeSpeedMultiplier(STOPTIME_SPEED_MULTIPLIER, FishSpeedModifierType.StopTime);//for smoothest animation of objects
        currentTime = Time.time;
        fishAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

    }

    protected override void OnExpire() {
        //Debug.Log($"{target.name} has expired StopTime Effect after {Duration} s.");
        target.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.StopTime);
        FishAnimManager.SetGetMaterialWithPropBlock(fishRenderer, ref fishMatProp, "_UnscaledTime", false, STOP_VALUE_FOR_SHADER/*fishRenderer.sharedMaterial.GetFloat("_UnscaledTime")*/);
        //fishRenderer.sharedMaterial.SetFloat("_UnscaledTime", STOP_VALUE_FOR_SHADER); /*wfwf*/
        GameManager.SetFixedDeltaTime(true);
        GameManager.ChangeGameTime(GameManager.DEFAULT_GAME_TIME);
        fishAnimator.updateMode = AnimatorUpdateMode.Fixed;

        //isStartedDiactivation = true;
    }

    public override void UpdateTimer() {
        //if(isStartedDiactivation)
        //    return;
        currentTime += Time.unscaledDeltaTime;
        FishAnimManager.SetGetMaterialWithPropBlock(fishRenderer, ref fishMatProp, "_UnscaledTime", false, /*isStartDeactivate ? STOP_VALUE_FOR_SHADER :*/ currentTime);

        base.UpdateTimer();
        
        //fishRenderer.sharedMaterial.SetFloat("_UnscaledTime", (isStartedDiactivation) ?//for avoid parasyte lasts frame update (called after OnExpire)
        //    STOP_VALUE_FOR_SHADER : currentTime);
    }
}
