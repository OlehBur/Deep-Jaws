using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using VContainer.Unity;
//using VContainer;
using System;
using System.Threading.Tasks;
using System.Threading;


public interface IEffectsVisualizationService {
    void ActivateDamageVisualization();
    //Task DamageVignette();
    //Task ActivateFreezeTimeVisualization(EffectManager effectManager);
    //Task ActivateBlindnessVisualization(EffectManager effectManager);
    //Task ActivateStunVisualization(EffectManager effectManager);
    //Task ActivatePoisonVisualization(EffectManager effectManager);
    void EffectsVisualizationsUpdate(FishEffectType type, EffectManager effectManager, bool isAdded);
    void ResetAllEffectsVisualization();
}

public class EffectsVisualizationService : IEffectsVisualizationService, IStartable, IDisposable {
    private readonly Volume _mainVolume;

    private Vignette _vignette = null;
    private LensDistortion _lensDistortion = null;
    private ChromaticAberration _chromaticAberration = null;
    private ColorAdjustments _colorAdjustments = null;
    private DepthOfField _depthOfField = null;

    private CancellationTokenSource _damageCts;
    private /*Coroutine*/Task _damageTask;
    private Task _stunTask;
    private Task _poisonTask;
    private Task _freezeTimeTask;
    private Task _blindnessTask;

    private static readonly Color VIGNETTE_DEFAULT_COLOR = Color.black;
    private static readonly Color VIGNETTE_DAMAGE_COLOR = Color.red;

    private const float VIGNETTE_INTENS_DAMAGE = 0.56f;
    private const float VIGNETTE_INTENS_DEFAULT = 0.417f;
    private const float LENS_DISORTION_MAX_INTENS = -0.7f;
    private const float LENS_DISORTION_DEFAULT_INTENS = 0f;
    private const float LENS_DISORTION_OFFSET_MULT = .08f;
    private const float LENS_DISORTION_OFFSET_CENTER = .5f;
    private const float LENS_DISORTION_MAX_SCALE = 1.17f;
    private const float LENS_DISORTION_DEFAULT_SCALE = 1f;
    private const float LENS_DISORTION_OFFSET_SPEED = .5f;//in rad/s
    private const float CHROMATICABERRATION_DEFAULT_INTENSITY = 0;
    private const float CHROMATICABERRATION_MAX_INTENSITY = 1;
    private const float COLORADJUSTMENTS_DEFAULT_HUE = 0;
    private const float COLORADJUSTMENTS_RANGE_HUE_MULT = 180f;
    private const float COLORADJUSTMENTS_DEFAULT_SATURATION = 0f;
    private const float COLORADJUSTMENTS_MAX_SATURATION = 100f;
    private const float EFFECTS_FADE_DURATION = 1.5f;
    private const float DEPTHOFFIELD_DEFAULT_START = 26f;
    private const float DEPTHOFFIELD_DEFAULT_END = 30f;
    private const float DEPTHOFFIELD_MIN_STARTEND = 0f;


    public EffectsVisualizationService(IGraphicManager graphicManager) {
        _mainVolume = graphicManager.Volume;
    }

    void IStartable.Start() {
        if (_mainVolume.sharedProfile.TryGet(out Vignette vignette))
            _vignette = vignette;
        if (_mainVolume.sharedProfile.TryGet(out LensDistortion lensDistortion))
            _lensDistortion = lensDistortion;
        if (_mainVolume.sharedProfile.TryGet(out ChromaticAberration chromaticAberration))
            _chromaticAberration = chromaticAberration;
        if (_mainVolume.sharedProfile.TryGet(out ColorAdjustments colorAdjustments))
            _colorAdjustments = colorAdjustments;
        if (_mainVolume.sharedProfile.TryGet(out DepthOfField depthOfField)) {
            _depthOfField = depthOfField;
            _depthOfField.mode.value = DepthOfFieldMode.Gaussian;
            _depthOfField.gaussianStart.value = DEPTHOFFIELD_DEFAULT_START;
            _depthOfField.gaussianEnd.value = DEPTHOFFIELD_DEFAULT_END;
        }
    }

    void IDisposable.Dispose() {
        ResetAllEffectsVisualization();
    }

    private bool IsTaskRunning(Task task) => task != null && !task.IsCompleted;

    public void ActivateDamageVisualization() {
        //if task already started - stop
        if (_damageCts != null) {
            _damageCts.Cancel();
            _damageCts.Dispose();
        }

        _damageCts = new CancellationTokenSource();
        _damageTask = DamageVignette(_damageCts.Token);
    }

    /*IEnumerator*/
    /*public*/
    async Task DamageVignette(CancellationToken token) {
        try {
            float deltaSmooth = 0;
            float normDelta = 0;
            _vignette.color.value = VIGNETTE_DAMAGE_COLOR;
            _vignette.intensity.value = VIGNETTE_INTENS_DAMAGE;

            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

                _vignette.color.value = Color.Lerp(VIGNETTE_DAMAGE_COLOR, VIGNETTE_DEFAULT_COLOR, normDelta);
                _vignette.intensity.value = Mathf.Lerp(VIGNETTE_INTENS_DAMAGE, VIGNETTE_INTENS_DEFAULT, normDelta);

                //yield return null;
                await Task.Yield();
            }
        }
        catch (OperationCanceledException) {/*if the task canceled*/}
        finally {//always called
            _vignette.color.value = VIGNETTE_DEFAULT_COLOR;
            _vignette.intensity.value = VIGNETTE_INTENS_DEFAULT;

            if (_damageCts != null && _damageCts.Token == token) {//if the task canceled
                _damageCts.Dispose();
                _damageCts = null;
                _damageTask = null;
            }
        }
    }

    /*IEnumerator*/
    /*public*/
    async Task ActivateFreezeTimeVisualization(EffectManager effectManager) {
        try {
            bool isFreeze = true;
            float deltaSmooth = 0;
            float normDelta = 0;
            _chromaticAberration.intensity.value = CHROMATICABERRATION_MAX_INTENSITY;

            while (isFreeze) {
                isFreeze = effectManager.ActiveEffects.ContainsKey(FishEffectType.FreezeTime)
                    && effectManager.ActiveEffects[FishEffectType.FreezeTime].Enabled;
                await Task.Yield();             //yield return null;
            }

            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

                _chromaticAberration.intensity.value = Mathf.Lerp(CHROMATICABERRATION_MAX_INTENSITY, CHROMATICABERRATION_DEFAULT_INTENSITY, normDelta);

                await Task.Yield();             //yield return null;
            }
        }
        finally {
            _chromaticAberration.intensity.value = CHROMATICABERRATION_DEFAULT_INTENSITY;
        }
    }

    /*public*/
    async Task ActivateBlindnessVisualization(EffectManager effectManager) {
        try {
            bool isBlindness = true;
            float hueShift = 0f;
            float deltaSmooth = 0;
            float normDelta = 0;

            _colorAdjustments.hueShift.value = COLORADJUSTMENTS_DEFAULT_HUE;
            _depthOfField.gaussianStart.value = DEPTHOFFIELD_MIN_STARTEND;
            _depthOfField.gaussianEnd.value = DEPTHOFFIELD_MIN_STARTEND;

            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                hueShift = Mathf.Sin(Time.time) * COLORADJUSTMENTS_RANGE_HUE_MULT;
                _colorAdjustments.hueShift.value = hueShift;
                _colorAdjustments.saturation.value = Mathf.Lerp(COLORADJUSTMENTS_DEFAULT_SATURATION, COLORADJUSTMENTS_MAX_SATURATION, normDelta);

                await Task.Yield();
            }
            _colorAdjustments.saturation.value = COLORADJUSTMENTS_MAX_SATURATION;

            while (isBlindness) {
                isBlindness = effectManager.ActiveEffects.ContainsKey(FishEffectType.Blindness)
                    && effectManager.ActiveEffects[FishEffectType.Blindness].Enabled;

                hueShift = Mathf.Sin(Time.time) * COLORADJUSTMENTS_RANGE_HUE_MULT;
                _colorAdjustments.hueShift.value = hueShift;

                await Task.Yield();
            }

            deltaSmooth = 0;
            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                normDelta = deltaSmooth / EFFECTS_FADE_DURATION;
                hueShift = Mathf.Lerp(hueShift, COLORADJUSTMENTS_DEFAULT_HUE, normDelta);

                _colorAdjustments.hueShift.value = hueShift;
                _colorAdjustments.saturation.value = Mathf.Lerp(COLORADJUSTMENTS_MAX_SATURATION, COLORADJUSTMENTS_DEFAULT_SATURATION, normDelta);
                _depthOfField.gaussianStart.value = Mathf.Lerp(DEPTHOFFIELD_MIN_STARTEND, DEPTHOFFIELD_DEFAULT_START, normDelta);
                _depthOfField.gaussianEnd.value = Mathf.Lerp(DEPTHOFFIELD_MIN_STARTEND, DEPTHOFFIELD_DEFAULT_END, normDelta);

                await Task.Yield();
            }
        }
        finally {
            _depthOfField.gaussianStart.value = DEPTHOFFIELD_DEFAULT_START;
            _depthOfField.gaussianEnd.value = DEPTHOFFIELD_DEFAULT_END;
            _colorAdjustments.saturation.value = COLORADJUSTMENTS_DEFAULT_SATURATION;
            _colorAdjustments.hueShift.value = COLORADJUSTMENTS_DEFAULT_HUE;
        }
    }

    /*public*/
    async Task ActivateStunVisualization(EffectManager effectManager) {
        try {
            bool isStun = true;
            float deltaSmooth = 0;
            float normDelta = 0f;
            float time = 0;
            float x = 0;
            float y = 0;

            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                normDelta = deltaSmooth / EFFECTS_FADE_DURATION;
                time = Time.time * LENS_DISORTION_OFFSET_SPEED;
                x = Mathf.Cos(time) * LENS_DISORTION_OFFSET_MULT * normDelta;
                y = Mathf.Sin(time) * LENS_DISORTION_OFFSET_MULT * normDelta;

                _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_INTENS, LENS_DISORTION_MAX_INTENS, normDelta);
                _lensDistortion.center.value = new Vector2(LENS_DISORTION_OFFSET_CENTER + x, LENS_DISORTION_OFFSET_CENTER + y);
                _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_SCALE, LENS_DISORTION_MAX_SCALE, normDelta);

                await Task.Yield();
            }

            while (isStun) {
                isStun = effectManager.ActiveEffects.ContainsKey(FishEffectType.Stun)
                    && effectManager.ActiveEffects[FishEffectType.Stun].Enabled;

                time = Time.time * LENS_DISORTION_OFFSET_SPEED;
                x = Mathf.Cos(time) * LENS_DISORTION_OFFSET_MULT;
                y = Mathf.Sin(time) * LENS_DISORTION_OFFSET_MULT;
                _lensDistortion.intensity.value = LENS_DISORTION_MAX_INTENS;
                _lensDistortion.scale.value = LENS_DISORTION_MAX_SCALE;
                _lensDistortion.center.value = new Vector2(LENS_DISORTION_OFFSET_CENTER + x, LENS_DISORTION_OFFSET_CENTER + y);
                await Task.Yield();
            }

            //smooth fade

            deltaSmooth = 0f;
            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                normDelta = deltaSmooth / EFFECTS_FADE_DURATION;
                x = Mathf.Lerp(x, 0f, normDelta);
                y = Mathf.Lerp(y, 0f, normDelta);

                if (!effectManager.ActiveEffects.TryGetValue(FishEffectType.Poison, out Effect effect)
                    || !effect.Enabled) {//so check if stun not active
                    _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_MAX_INTENS, LENS_DISORTION_DEFAULT_INTENS, normDelta);
                    _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_MAX_SCALE, LENS_DISORTION_DEFAULT_SCALE, normDelta);
                }
                _lensDistortion.center.value.Set(LENS_DISORTION_OFFSET_CENTER + x, LENS_DISORTION_OFFSET_CENTER + y);

                await Task.Yield();
            }
        }
        finally {
            _lensDistortion.intensity.value = LENS_DISORTION_DEFAULT_INTENS;
            _lensDistortion.center.value.Set(LENS_DISORTION_OFFSET_CENTER, LENS_DISORTION_OFFSET_CENTER);
            _lensDistortion.scale.value = LENS_DISORTION_DEFAULT_SCALE;
        }
    }

    /*public*/
    async Task ActivatePoisonVisualization(EffectManager effectManager) {
        try {
            bool isPoison = true;
            float deltaSmooth = 0;
            float normDelta = 0;

            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

                _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_INTENS, LENS_DISORTION_MAX_INTENS, normDelta);
                _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_SCALE, LENS_DISORTION_MAX_SCALE, normDelta);

                await Task.Yield();
            }

            while (isPoison) {
                isPoison = effectManager.ActiveEffects.ContainsKey(FishEffectType.Poison)
                    && effectManager.ActiveEffects[FishEffectType.Poison].Enabled;
                _lensDistortion.intensity.value = LENS_DISORTION_MAX_INTENS;
                _lensDistortion.scale.value = LENS_DISORTION_MAX_SCALE;
                await Task.Yield();
            }

            //smooth fade
            if (!effectManager.ActiveEffects.TryGetValue(FishEffectType.Stun, out Effect effect)
                    || !effect.Enabled) {//poison and stun use same lens distortion//so check if stun not active

                deltaSmooth = 0f;
                while (deltaSmooth < EFFECTS_FADE_DURATION) {
                    deltaSmooth += DJTime.UnscaledDeltaTime;
                    normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

                    _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_MAX_INTENS, LENS_DISORTION_DEFAULT_INTENS, normDelta);
                    _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_MAX_SCALE, LENS_DISORTION_DEFAULT_SCALE, normDelta);
                    await Task.Yield();
                }
            }
        }
        finally {
            _lensDistortion.intensity.value = LENS_DISORTION_DEFAULT_INTENS;
            _lensDistortion.scale.value = LENS_DISORTION_DEFAULT_SCALE;
        }
    }

    /// <summary>
    /// for any additional behaviour (in UI) when an effect is added or removed from the player
    /// </summary>
    /// <param name="type"></param>
    /// <param name="added"></param>
    public void EffectsVisualizationsUpdate(FishEffectType type, EffectManager effectManager, bool isAdded) {
        if (type == FishEffectType.StunInitiator || !isAdded)
            return;

        if (isAdded) {        //if (currentOverlay != null) {
            if (type == FishEffectType.Stun && !IsTaskRunning(_stunTask))
                _stunTask = ActivateStunVisualization(effectManager);
            else if (type == FishEffectType.Poison && !IsTaskRunning(_poisonTask))
                _poisonTask = ActivatePoisonVisualization(effectManager);
            else if (type == FishEffectType.FreezeTime && !IsTaskRunning(_freezeTimeTask))
                _freezeTimeTask = ActivateFreezeTimeVisualization(effectManager);
            else if (type == FishEffectType.Blindness && !IsTaskRunning(_blindnessTask))
                _blindnessTask = ActivateBlindnessVisualization(effectManager);
        }
        else
            return;
    }

    public void ResetAllEffectsVisualization() {
        if (_vignette != null) {
            _vignette.intensity.value = VIGNETTE_INTENS_DEFAULT;
            _vignette.color.value = VIGNETTE_DEFAULT_COLOR;
        }
        if (_lensDistortion != null) {
            _lensDistortion.intensity.value = LENS_DISORTION_DEFAULT_INTENS;
            _lensDistortion.center.value = Vector2.one * LENS_DISORTION_OFFSET_CENTER;
            _lensDistortion.scale.value = LENS_DISORTION_DEFAULT_SCALE;
        }
        if (_chromaticAberration != null) {
            _chromaticAberration.intensity.value = CHROMATICABERRATION_DEFAULT_INTENSITY;
        }
        if (_colorAdjustments != null) {
            _colorAdjustments.hueShift.value = COLORADJUSTMENTS_DEFAULT_HUE;
            _colorAdjustments.saturation.value = COLORADJUSTMENTS_DEFAULT_SATURATION;
        }
        if (_depthOfField != null) {
            _depthOfField.gaussianStart.value = DEPTHOFFIELD_DEFAULT_START;
        }
    }

}
