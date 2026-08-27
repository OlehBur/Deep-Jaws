using System;
using System.Collections.Generic;
using UnityEngine;


public enum TypeEffects {
    All,
    VisibleUI,
    VisibleWorld,
    Negative,
    Positive
}

public class EffectManager : MonoBehaviour {
    [Header("Icons for effects (World)")]
    [SerializeField] GameObject shieldIcon;
    [SerializeField] GameObject stunIcon;
    [SerializeField] GameObject currentIcon;
    [SerializeField] GameObject poisonIcon;
    [SerializeField] Rigidbody rb;
    [SerializeField] Renderer fishSkinnedRenderer;
    //[SerializeField] Transform editableTransform;
    [SerializeField] Transform rotationTransform;
    //[SerializeField] public bool IsHaveShield = true;
    //[SerializeField] Vector3 shieldLocalPos;
    //[SerializeField] Vector3 shieldLocalScale;
    //[SerializeField] Vector3 whirlLocalPos;
    [SerializeField] Fish fish;
    [SerializeField] FishAnim fishAnim;

    public Dictionary<FishEffectType, Effect> activeEffects { get; private set; } = new();
    public event Action<FishEffectType, bool> OnEffectChanges;
    public GameObject ShieldObj { get; set; } = null;
    // cache all effect types for iteration
    public static readonly FishEffectType[] AllEffectTypes = (FishEffectType[])Enum.GetValues(typeof(FishEffectType));

    private bool hasSpeedEffects = false;    //fixed update optimization
    private AudioSource speedEffectSource;
    private AudioManager audioManager;
    private FishAdditionalCompManager fishAdditionalComponentManager;
    private Vector3 prevPos;

    private const float X_OFFSET_DOUBLE = .6f;
    private const float X_OFFSET_TRIPLE = 1.2f;
    private const float X_OFFSET_FOURTH = 1.8f;
    private const float MIN_VOLUME = 0.01f;
    private const float VOLUME_FADE_SPEED = 4f;

    /// <summary>
    /// set player current effect audiosource to control volume by effect
    /// </summary>
    /// <param name="speedEffectSource">only for player</param>
    public void Init(AudioManager audioManager/*, AudioSource speedEffectSource*/, FishAdditionalCompManager fishAdditionalComponentManager) {
        this.fishAdditionalComponentManager = fishAdditionalComponentManager;

        if (audioManager == null)
            return;

        this.audioManager = audioManager;
        speedEffectSource = audioManager.playerRevolumeAudioSource;
    }
    private void OnEnable() {
        if (fishSkinnedRenderer != null)//if respawn afret freezetime effect activation
            fishSkinnedRenderer.sharedMaterial.SetFloat(/*"_UnscaledTime"*/GameStaticInfo.FISH_WAVING_UNSCAL_TIME_PROP, 0f);

        activeEffects.Clear();
    }

    // effects update timers
    private void Update() {
        foreach (FishEffectType type in AllEffectTypes)
            if (IsEffectActive(type))
                activeEffects[type].UpdateTimer();
    }

    /// <summary>
    /// is effect exist and enabled
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsEffectActive(FishEffectType type) {
        return activeEffects.ContainsKey(type) && activeEffects[type] != null && activeEffects[type].Enabled;
    }

    public bool IsNoNegativeEffects() {
        return !(IsEffectActive(FishEffectType.Stun)
            || IsEffectActive(FishEffectType.Blindness)
            || IsEffectActive(FishEffectType.Poison));
    }


    public int GetActiveEffectCnt(TypeEffects key) {
        int cnt = 0;

        foreach (KeyValuePair<FishEffectType, Effect> kvp in activeEffects)
            if (kvp.Value != null && kvp.Value.Enabled)
                switch (key) {
                    case TypeEffects.All:
                    cnt++;
                    break;

                    case TypeEffects.VisibleUI:
                    if (IsVisibleEffect(kvp.Key, true))
                        cnt++;
                    break;

                    case TypeEffects.VisibleWorld:
                    if (IsVisibleEffect(kvp.Key, false))
                        cnt++;
                    break;

                    case TypeEffects.Negative:
                    if (IsNegativeEffect(kvp.Key))
                        cnt++;
                    break;

                    case TypeEffects.Positive:
                    if (!IsNegativeEffect(kvp.Key))
                        cnt++;
                    break;
                }


        return cnt;
    }

    /// <summary>
    /// Return eff to visibl
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isUi">in ui oterwise in world</param>
    /// <returns></returns>
    public static bool IsVisibleEffect(FishEffectType type, bool isUi) {
        if (isUi)
            return type != FishEffectType.StunInitiator;
        else
            return type != FishEffectType.Blindness && type != FishEffectType.SprintSpeed
                && type != FishEffectType.FreezeTime && type != FishEffectType.StunInitiator;
    }

    public static bool IsNegativeEffect(FishEffectType type) =>
        type == FishEffectType.Stun || type == FishEffectType.Poison || type == FishEffectType.Blindness;


    public void ApplyEffect(FishEffectType type, float duration) {
        if (activeEffects.TryGetValue(type, out Effect existing) && existing != null && existing.Enabled) {
            existing.Refresh(duration);
            return;
        }

        if (fish == null)
            return;

        Effect newEffect = InitNewEffect(type);

        if (newEffect != null) {
            activeEffects[type] = newEffect;
            newEffect.Init(fish, duration);

            if (type == FishEffectType.Shield) {//import after Init (shield obj added in apply meth in init)
                ShieldEffect shieldEffect = newEffect as ShieldEffect;
                if (shieldEffect.ShieldWrap != null)
                    ShieldObj = shieldEffect.ShieldWrap.obj;
            }

            //if (type == FishEffectType.StunInitiator)
            //    Debug.Log("Stun Initiator effect applied");
            if (activeEffects.TryGetValue(type, out Effect effect))
                effect.OnEffeñtExpire -= RemoveEffect;
            activeEffects[type] = newEffect; // use [] instead TryAdd 

            UpdateIconVisualization();

            newEffect.OnEffeñtExpire += RemoveEffect;

            // upd flag for FixedUpdate
            UpdateSpeedEffectsFlag();
            CheckAchievements();

            OnEffectChanges?.Invoke(type, true);
        }
    }

    public void RemoveEffect(FishEffectType type) {
        UpdateIconVisualization();
        UpdateSpeedEffectsFlag(); // upd flag
        OnEffectChanges?.Invoke(type, false);
    }

    //optimization for FixedUpdate
    private void UpdateSpeedEffectsFlag() {
        hasSpeedEffects = IsEffectActive(FishEffectType.SprintSpeed) ||
                         IsEffectActive(FishEffectType.CurrentSpeed);
    }

    private void CheckAchievements() {
        if (IsEffectActive(FishEffectType.SprintSpeed) && IsEffectActive(FishEffectType.CurrentSpeed)/*activeEffects.ContainsKey(FishEffectType.SprintSpeed) && activeEffects.ContainsKey(FishEffectType.CurrentSpeed)*/)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_youre_fast_as_flash);

        if (GetActiveEffectCnt(TypeEffects.VisibleUI) >= 4)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_thats_impossible_seriously);
    }


    //in world
    private void UpdateIconVisualization() {
        int visibleEffectsCnt = GetActiveEffectCnt(TypeEffects.VisibleWorld);
        float[] XOffsets = GetIconOffsets(visibleEffectsCnt);

        // deactivate all icons 
        SetAllIconsActive(false);

        //only needed icons activation and positioning
        int index = 0;
        foreach (var kvp in activeEffects) {
            if (kvp.Value != null && kvp.Value.Enabled && IsVisibleEffect(kvp.Key, false)) {
                SetIconActive(kvp.Key, true, XOffsets[index]);
                index++;
            }
        }
    }

    private float[] GetIconOffsets(int count) {
        return count switch {
            1 => new float[1] { 0f },
            2 => new float[2] { -X_OFFSET_DOUBLE, X_OFFSET_DOUBLE },
            3 => new float[3] { -X_OFFSET_TRIPLE, 0, X_OFFSET_TRIPLE },
            4 => new float[4] { -X_OFFSET_FOURTH, -X_OFFSET_DOUBLE, X_OFFSET_DOUBLE, X_OFFSET_FOURTH },
            _ => new float[1] { 0f }
        };
    }

    private void SetAllIconsActive(bool active) {
        shieldIcon.SetActive(active);
        stunIcon.SetActive(active);
        currentIcon.SetActive(active);
        poisonIcon.SetActive(active);
    }

    private void SetIconActive(FishEffectType type, bool active, float xOffset) {
        GameObject icon = type switch {
            FishEffectType.Shield => shieldIcon,
            FishEffectType.Stun => stunIcon,
            FishEffectType.CurrentSpeed => currentIcon,
            FishEffectType.Poison => poisonIcon,
            _ => null
        };

        if (icon != null) {
            icon.SetActive(active);
            if (active)
                icon.transform.localPosition = new Vector3(xOffset, /*Y_OFFSET*/0f, 0f);
        }
    }
    Effect InitNewEffect(FishEffectType type) {
        return type switch {
            FishEffectType.Shield => CreateShieldEffect(),
            FishEffectType.CurrentSpeed => CreateCurrentEffect(),
            FishEffectType.Stun => new StunEffect(rb),
            FishEffectType.StunInitiator => new StunInitiatorEffet(),
            FishEffectType.SprintSpeed => CreateSprintEffect(),
            FishEffectType.FreezeTime => CreateFreezeTimeEffect(),
            FishEffectType.Poison => CreatePoisonEffect(),
            FishEffectType.Blindness => new BlindnessEffect(),
            _ => null
        };
    }

    private Effect CreateShieldEffect() {
        if (!/*IsHaveShield*/fish.DefaultInfo.IsHaveShield)
            return null;

        ShieldEffect shieldEffect = new(audioManager,
            fishAdditionalComponentManager,
            /*editableTransform*/rotationTransform,
            /*shieldLocalPos*/fish.DefaultInfo.ShieldLocPos,
            /*shieldLocalScale*/fish.DefaultInfo.ShieldVisualScale);

        return shieldEffect;
    }

    private Effect CreateCurrentEffect() {
        CurrentEffect speedEffect = new(fishAdditionalComponentManager, /*transform*/rotationTransform, /*whirlLocalPos*/fish.DefaultInfo.WhirlLocPos);

        if (speedEffectSource != null)//not null only for player
            speedEffectSource.enabled = true;

        return speedEffect;
    }

    private Effect CreateSprintEffect() {
        SprintEffect sprintEffect = new(fishAdditionalComponentManager, fishAnim, rotationTransform, fish.DefaultInfo.WhirlLocPos);

        if (speedEffectSource != null)
            speedEffectSource.enabled = true;

        return sprintEffect;
    }

    private Effect CreateFreezeTimeEffect() {
        if (!fish.IsPlayer)
            return null;

        StopTimeEffect stopTimeEffect = new(fishSkinnedRenderer, fishAnim.FishAnimator);
        return stopTimeEffect;
    }

    private Effect CreatePoisonEffect() {
        if (fish.Level == FishLevel.Cuttlefish || fish.Level == FishLevel.Pufferfish)
            return null;

        PoisonEffect poisonEffect = new();
        return poisonEffect;
    }

    private void FixedUpdate() {
        if (!fish.IsPlayer || speedEffectSource == null
            || (!hasSpeedEffects && speedEffectSource.volume == 0f))
            return;

        float velocity = (prevPos - rb.position).magnitude / Time.fixedDeltaTime;
        prevPos = rb.position;

        if (velocity > 0f && hasSpeedEffects)//if fish is moving and has speed effects - inc vol
            speedEffectSource.volume = Mathf.Lerp(speedEffectSource.volume, 1f, Time.fixedDeltaTime * VOLUME_FADE_SPEED);
        else
            speedEffectSource.volume = Mathf.Lerp(speedEffectSource.volume, 0, Time.fixedDeltaTime * VOLUME_FADE_SPEED);

        // dis audiosource if it dont need
        if (speedEffectSource.volume < MIN_VOLUME && !hasSpeedEffects)
            speedEffectSource.enabled = false;
    }

    // safe cleanup
    private void OnDestroy() {
        foreach (KeyValuePair<FishEffectType, Effect> kvp in activeEffects) {
            if (kvp.Value != null) {
                kvp.Value.OnEffeñtExpire -= RemoveEffect;
            }
        }
    }

    private void OnDisable() {
        foreach (FishEffectType type in AllEffectTypes)
            if (IsEffectActive(type)) {
                activeEffects[type].OnEffeñtExpire -= RemoveEffect;
                activeEffects[type].DeactivateEffect();
                activeEffects[type].Enabled = false;
            }

        activeEffects.Clear();
        SetAllIconsActive(false);
    }
}
