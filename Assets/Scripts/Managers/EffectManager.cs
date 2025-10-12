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
    public Dictionary<FishEffectType, Effect> activeEffects { get; private set; } = new();
    public event Action<FishEffectType, bool> OnEffectChanges;

    [Header("Icons for effects (World)")]
    [SerializeField] GameObject shieldIcon;
    [SerializeField] GameObject stunIcon;
    [SerializeField] GameObject currentIcon;
    [SerializeField] GameObject poisonIcon;

    [SerializeField] GameObject shieldObj = null;
    [SerializeField] Rigidbody rb;
    private Vector3 prevPos;
    [SerializeField] SkinnedMeshRenderer fishSkinnedRenderer;

    /*[SerializeField]*/
    private AudioSource speedEffectSource;
    [SerializeField] GameObject currentBubblesWhirl;
    [SerializeField] GameObject waterTrail;
    [SerializeField] Fish fish;
    [SerializeField] FishAnimManager fishAnim;

    // cache all effect types for iteration
    private static readonly FishEffectType[] AllEffectTypes = (FishEffectType[])Enum.GetValues(typeof(FishEffectType));
    //private readonly Dictionary<FishEffectType, Component> existingComponents = new();

    //fixed update optimization
    private bool hasSpeedEffects = false;
    //private bool isPlayerFish;

    private const float Y_OFFSET = 2f;
    private const float X_OFFSET_DOUBLE = .6f;
    private const float X_OFFSET_TRIPLE = 1.2f;
    private const float X_OFFSET_FOURTH = 1.8f;
    private const float MIN_VOLUME = 0.01f;
    private const float VOLUME_FADE_SPEED = 4f;

    /// <summary>
    /// set player current effect audiosource to control volume by effect
    /// </summary>
    /// <param name="speedEffectSource">only for player</param>
    public void Init(AudioSource speedEffectSource) {
        this.speedEffectSource = speedEffectSource;
        //isPlayerFish = CompareTag("Player");
        //CacheExistingComponents(); //cache existing components on start
    }
    private void OnEnable() {
        if (fishSkinnedRenderer != null)//if respawn afret freezetime effect activation
            fishSkinnedRenderer.sharedMaterial.SetFloat("_UnscaledTime", 0f);
    }

    // effects update timers
    private void Update() {
        foreach (FishEffectType type in AllEffectTypes)
            if (IsEffectActive(type))
                activeEffects[type].UpdateTimer();
    }

    bool IsEffectActive(FishEffectType type) {
        return activeEffects.ContainsKey(type) && activeEffects[type] != null && activeEffects[type].Enabled;
    }

    public bool IsNoNegativeEffects() {
        return !(IsEffectActive(FishEffectType.Stun)
            || IsEffectActive(FishEffectType.Blindness)
            || IsEffectActive(FishEffectType.Poison));
    }


    // cache existing components to avoid multiple GetComponent calls
    //private void CacheExistingComponents() {
    //    foreach (FishEffectType type in AllEffectTypes) {
    //        if (TryGetEffectComponent(type, out Component component)) {
    //            existingComponents[type] = component;
    //        }
    //    }
    //}

    public int GetActiveEffectCnt(TypeEffects key) {
        // clear without allocations
        //List<FishEffectType> keysToRemove = new List<FishEffectType>();
        //foreach (KeyValuePair<FishEffectType, Effect> kvp in activeEffects) {
        //    if (kvp.Value == null) {
        //        keysToRemove.Add(kvp.Key);
        //    }
        //}

        //foreach (FishEffectType key in keysToRemove) {
        //    activeEffects.Remove(key);
        //}

        //return activeEffects.Count;

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

    public static bool IsNegativeEffect(FishEffectType type) {
        return type == FishEffectType.Stun || type == FishEffectType.Poison || type == FishEffectType.Blindness;
    }

    //private bool TryGetEffectComponent(FishEffectType type, out Component component) {
    //    // check cache
    //    if (existingComponents.TryGetValue(type, out component) && component != null) {
    //        return true;
    //    }

    //    //if cache miss - try get component
    //    switch (type) {
    //        case FishEffectType.Shield:
    //            return TryGetComponent(out component);
    //        case FishEffectType.CurrentSpeed:
    //            return TryGetComponent(out CurrentEffect currentEffect) && (component = currentEffect) != null;
    //        case FishEffectType.FreezeTime:
    //            return TryGetComponent(out StopTimeEffect timeEffect) && (component = timeEffect) != null;
    //        case FishEffectType.StunInitiator:
    //            return TryGetComponent(out StunInitiatorEffet stunInit) && (component = stunInit) != null;
    //        case FishEffectType.Poison:
    //            return TryGetComponent(out PoisonEffect poison) && (component = poison) != null;
    //        case FishEffectType.Stun:
    //            return TryGetComponent(out StunEffect stun) && (component = stun) != null;
    //        case FishEffectType.SprintSpeed:
    //            return TryGetComponent(out SprintEffect sprint) && (component = sprint) != null;
    //        case FishEffectType.Blindness:
    //            return TryGetComponent(out BlindnessEffect blindness) && (component = blindness) != null;
    //        default:
    //            component = null;
    //            return false;
    //    }
    //}

    public void ApplyEffect(FishEffectType type, float duration) {
        if (activeEffects.TryGetValue(type, out Effect existing) && existing != null && existing.Enabled) {
            existing.Refresh(duration);
            return;
        }

        if (fish == null)
            return;

        // first check exist compon
        //if (TryGetEffectComponent(type, out Component existingComponent) && existingComponent is Effect existingEffect) {
        //    newEffect = existingEffect;
        //}
        //else

        Effect newEffect = InitNewEffect(type);
        activeEffects[type] = newEffect; //        newEffect = AddEffectComponent(type);

        if (newEffect != null) {
            newEffect.Init(fish, duration);

            if (type != FishEffectType.StunInitiator)
                activeEffects[type] = newEffect; // use [] instead TryAdd

            UpdateIconVisualization();

            newEffect.OnEffeñtExpire -= RemoveEffect;
            newEffect.OnEffeñtExpire += RemoveEffect;

            // upd flag for FixedUpdate
            UpdateSpeedEffectsFlag();
            CheckAchievements();

            OnEffectChanges?.Invoke(type, true);
        }
    }

    public void RemoveEffect(FishEffectType type) {
        //if (activeEffects.TryGetValue(type, out Effect effect) && effect != null) {
        //    effect.OnEffeñtExpire -= RemoveEffect;
        //}

        //activeEffects.Remove(type);
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
        if (activeEffects.ContainsKey(FishEffectType.SprintSpeed) && activeEffects.ContainsKey(FishEffectType.CurrentSpeed))
            GPG_Manager.OnTriggerAchievement(AchievementKey.FlashSpeed);

        if (activeEffects.Count >= 4)
            GPG_Manager.OnTriggerAchievement(AchievementKey.Impossible);
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

    //private int GetVisibleEffectsCount() {
    //    int count = 0;
    //    foreach (var kvp in activeEffects)
    //        if (kvp.Value != null && ShouldShowIcon(kvp.Key))
    //            count++;

    //    return count;
    //}

    /// <summary>
    /// icons that shouldnt shown in world - return false
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    //private bool ShouldShowIcon(FishEffectType type) {
    //    return type != FishEffectType.SprintSpeed &&
    //           type != FishEffectType.StunInitiator &&
    //           type != FishEffectType.FreezeTime &&
    //           type != FishEffectType.Blindness;
    //}

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
                icon.transform.localPosition = new Vector3(xOffset, Y_OFFSET, 0f);
        }
    }

    //private Effect AddEffectComponent(FishEffectType type) {//if exist before adding - return existing
    //    if (TryGetEffectComponent(type, out Component existing) && existing is Effect existingEffect)
    //        return existingEffect;

    //    return type switch {
    //        FishEffectType.Shield => CreateShieldEffect(),
    //        FishEffectType.CurrentSpeed => CreateCurrentEffect(),
    //        FishEffectType.Stun => gameObject.AddComponent<StunEffect>(),
    //        FishEffectType.StunInitiator => gameObject.AddComponent<StunInitiatorEffet>(),
    //        FishEffectType.SprintSpeed => CreateSprintEffect(),
    //        FishEffectType.FreezeTime => CreateFreezeTimeEffect(),
    //        FishEffectType.Poison => CreatePoisonEffect(),
    //        FishEffectType.Blindness => gameObject.AddComponent<BlindnessEffect>(),
    //        _ => null
    //    };
    //}

    Effect InitNewEffect(FishEffectType type) {
        return type switch {
            FishEffectType.Shield => CreateShieldEffect(),
            FishEffectType.CurrentSpeed => CreateCurrentEffect(),
            FishEffectType.Stun => new StunEffect(GetComponent<Rigidbody>()),
            FishEffectType.StunInitiator => new StunInitiatorEffet(),
            FishEffectType.SprintSpeed => CreateSprintEffect(),
            FishEffectType.FreezeTime => CreateFreezeTimeEffect(),
            FishEffectType.Poison => CreatePoisonEffect(),
            FishEffectType.Blindness => new BlindnessEffect(),
            _ => null
        };
    }

    private Effect CreateShieldEffect() {
        if (shieldObj == null) return null;

        //ShieldEffect shieldEffect = gameObject.AddComponent<ShieldEffect>();
        //shieldEffect.Init(shieldObj);
        //existingComponents[FishEffectType.Shield] = shieldEffect; // upd cache

        ShieldEffect shieldEffect = new(shieldObj);

        return shieldEffect;
    }

    private Effect CreateCurrentEffect() {
        //CurrentEffect speedEffect = gameObject.AddComponent<CurrentEffect>();
        //speedEffect.Init(currentBubblesWhirl);

        //if (speedEffectSource != null)
        //    speedEffectSource.enabled = isPlayerFish;
        //existingComponents[FishEffectType.CurrentSpeed] = speedEffect; //upd cache

        CurrentEffect speedEffect = new(currentBubblesWhirl);

        if (speedEffectSource != null)
            speedEffectSource.enabled = fish.IsPlayer;

        return speedEffect;
    }

    private Effect CreateSprintEffect() {
        //SprintEffect sprintEffect = gameObject.AddComponent<SprintEffect>();
        //sprintEffect.Init(waterTrail, fishAnim);

        //if (speedEffectSource != null)
        //    speedEffectSource.enabled = isPlayerFish;
        //existingComponents[FishEffectType.SprintSpeed] = sprintEffect; // upd cache

        SprintEffect sprintEffect = new(waterTrail, fishAnim);

        if (speedEffectSource != null)
            speedEffectSource.enabled = fish.IsPlayer;

        return sprintEffect;
    }

    private Effect CreateFreezeTimeEffect() {
        if (!fish.IsPlayer)
            return null;

        //StopTimeEffect stopTimeEffect = gameObject.AddComponent<StopTimeEffect>();
        //stopTimeEffect.Init(fishSkinnedRenderer, fishAnim.FishAnimator);
        //existingComponents[FishEffectType.FreezeTime] = stopTimeEffect; // upd cache

        StopTimeEffect stopTimeEffect = new(fishSkinnedRenderer, fishAnim.FishAnimator);

        return stopTimeEffect;
    }

    private Effect CreatePoisonEffect() {
        if (fish.Level == FishLevel.Cuttlefish || fish.Level == FishLevel.Pufferfish)
            return null;

        //PoisonEffect poisonEffect = gameObject.AddComponent<PoisonEffect>();
        //existingComponents[FishEffectType.Poison] = poisonEffect; // upd cache

        PoisonEffect poisonEffect = new();

        return poisonEffect;
    }

    private void FixedUpdate() {
        if (!fish.IsActive || speedEffectSource == null
            || (!hasSpeedEffects && speedEffectSource.volume == 0f)
            || !speedEffectSource.enabled)
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
                activeEffects[type].Enabled = false;
            }

        //    foreach (KeyValuePair<FishEffectType, Effect> kvp in activeEffects.ToArray()) { //safe iteration
        //if (kvp.Value != null) {
        //    kvp.Value.OnEffeñtExpire -= RemoveEffect;
        //    kvp.Value.Enabled = false;
        //    //Destroy(kvp.Value);
        //}
        //}
        activeEffects.Clear();

        SetAllIconsActive(false);
    }
}
