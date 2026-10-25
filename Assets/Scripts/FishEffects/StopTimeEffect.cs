
using UnityEngine;

public class StopTimeEffect : Effect {
    Renderer _fishRenderer;
    MaterialPropertyBlock _fishMatProp;
    Animator _fishAnimator;//for freeze fish animation
    float _currentTime = 0f;

    const float STOP_TIME_SCALE = .2f;
    const float STOPTIME_SPEED_MULTIPLIER = (1f / STOP_TIME_SCALE) * SLOW_MULT;
    const float SLOW_MULT = .9f;
    /// <summary>
    /// значення при якому шейдер починає знову брати значення скаледТайм замість нашого з ефекту 
    /// value we set when shader starts to take scaledTime value instead of our from effect
    /// </summary>
    const float STOP_VALUE_FOR_SHADER = 0f;

    public StopTimeEffect(Renderer fishWaveMat, Animator fishAnimator) {
        _fishRenderer = fishWaveMat;
        _fishAnimator = fishAnimator;
        Type = FishEffectType.FreezeTime;
        ////Debug.LogWarning("Init StopTimeEffect");

        InitPropBlock();
    }

    private void InitPropBlock() {
        _fishMatProp = new MaterialPropertyBlock();
        FishAnim.SetMaterialProp(ref _fishRenderer, ref _fishMatProp, /*"_UnscaledTime"*/GameStaticInfo.FISH_WAVING_UNSCAL_TIME_PROP, _fishRenderer.sharedMaterial.GetFloat(GameStaticInfo.FISH_WAVING_UNSCAL_TIME_PROP));
    }

    protected override void OnApply() {
        //Debug.Log($"{target.name} has applied StopTime Effect for {Duration} s.");
        GameManager.ChangeGameTime(STOP_TIME_SCALE);
        GameManager.SetFixedDeltaTime(false, GameManager.CurrentFixedDeltaTime * STOP_TIME_SCALE);
        targetFish.ChangeSpeedMultiplier(STOPTIME_SPEED_MULTIPLIER, FishSpeedModifierType.StopTime);//for smoothest animation of objects
        _currentTime = Time.time;
        _fishAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

    }

    protected override void OnExpire() {
        //Debug.Log($"{target.name} has expired StopTime Effect after {Duration} s.");
        targetFish.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.StopTime);
        FishAnim.SetMaterialProp(ref _fishRenderer, ref _fishMatProp, /*"_UnscaledTime"*/GameStaticInfo.FISH_WAVING_UNSCAL_TIME_PROP, STOP_VALUE_FOR_SHADER);
        //fishRenderer.sharedMaterial.SetFloat("_UnscaledTime", STOP_VALUE_FOR_SHADER); /*wfwf*/
        GameManager.SetFixedDeltaTime(true);
        GameManager.ChangeGameTime(GameManager.DEFAULT_GAME_TIME);
        _fishAnimator.updateMode = AnimatorUpdateMode.Fixed;
    }

    public override void UpdateTimer() {
        _currentTime += DJTime.UnscaledDeltaTime;
        FishAnim.SetMaterialProp(ref _fishRenderer, ref _fishMatProp, GameStaticInfo.FISH_WAVING_UNSCAL_TIME_PROP, _currentTime);

        base.UpdateTimer();
    }
}
