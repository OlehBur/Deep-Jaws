using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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

    [SerializeField] AudioSource speedEffectSource;
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

    private void Start() {
        //isPlayerFish = CompareTag("Player");
        //CacheExistingComponents(); //cache existing components on start
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

    public int GetActiveEffectCnt() {
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
                cnt++;

        return cnt;
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
        if (activeEffects.TryGetValue(type, out Effect existing) && existing != null) {
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
        if (activeEffects.TryGetValue(type, out Effect effect) && effect != null) {
            effect.OnEffeñtExpire -= RemoveEffect;
        }

        activeEffects.Remove(type);
        UpdateIconVisualization();
        UpdateSpeedEffectsFlag(); // upd flag
        OnEffectChanges?.Invoke(type, false);
    }

    //optimization for FixedUpdate
    private void UpdateSpeedEffectsFlag() {
        hasSpeedEffects = activeEffects.ContainsKey(FishEffectType.SprintSpeed) ||
                         activeEffects.ContainsKey(FishEffectType.CurrentSpeed);
    }

    private void CheckAchievements() {
        if (activeEffects.ContainsKey(FishEffectType.SprintSpeed) && activeEffects.ContainsKey(FishEffectType.CurrentSpeed))
            GPG_Manager.OnTriggerAchievement(AchievementKey.FlashSpeed);

        if (activeEffects.Count >= 4)
            GPG_Manager.OnTriggerAchievement(AchievementKey.Impossible);
    }

    private void UpdateIconVisualization() {
        int visibleEffectsCnt = GetVisibleEffectsCount();
        float[] XOffsets = GetIconOffsets(visibleEffectsCnt);

        // deactivate all icons 
        SetAllIconsActive(false);

        //only needed icons activation and positioning
        int index = 0;
        foreach (var kvp in activeEffects) {
            if (kvp.Value != null && ShouldShowIcon(kvp.Key)) {
                SetIconActive(kvp.Key, true, XOffsets[index]);
                index++;
            }
        }
    }

    private int GetVisibleEffectsCount() {
        int count = 0;
        foreach (var kvp in activeEffects)
            if (kvp.Value != null && ShouldShowIcon(kvp.Key))
                count++;

        return count;
    }

    /// <summary>
    /// icons that shouldnt shown in world - return false
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool ShouldShowIcon(FishEffectType type) {
        return type != FishEffectType.SprintSpeed &&
               type != FishEffectType.StunInitiator &&
               type != FishEffectType.FreezeTime &&
               type != FishEffectType.Blindness;
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
    }
}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;


///// <summary>
/////component that adds and removes effects to the fish and stores all active effects
///// </summary>
//public class EffectManager : MonoBehaviour {
//    public Dictionary<FishEffectType, Effect> activeEffects { get; private set; } = new();
//    /// <summary>
//    /// called when an effect is added or removed, first parameter - effect type, second - true if added, false if removed
//    /// </summary>
//    public event Action<FishEffectType, bool> OnEffectChanges;
//    [Header("Icons for effects (World)")]
//    [SerializeField] GameObject shieldIcon; //icon for the shield effect, set in inspector
//    [SerializeField] GameObject stunIcon;
//    [SerializeField] GameObject currentIcon;
//    [SerializeField] GameObject poisonIcon;
//    //timeIcon, sprintIcon and stunInitiator - doesnt need to have icons above the fish

//    [SerializeField] GameObject shieldObj = null; //all small fish can have a shield, others are null;
//    [SerializeField] Rigidbody rb; //rigidbody for the player fish, set in inspector
//    private Vector3 prevPos; //previous position for the player fish, used to calculate velocity for volume adjustment of the speed effect audio source
//    [SerializeField] SkinnedMeshRenderer fishRenderer; //for change timescale in fishWave shader

//    //effects visualization
//    [SerializeField] AudioSource speedEffectSource;
//    [SerializeField] GameObject currentBubblesWhirl; //bubbles whirl for the current speed effect, set in inspector
//    [SerializeField] GameObject waterTrail; //water trail for the player fish, set in inspector
//    [SerializeField] Fish fish;
//    [SerializeField] FishAnimManager fishAnim; //for speedy tail waving while fish sprinting
//    private const float Y_OFFSET = 2f;//offset for icons above fish
//    private const float X_OFFSET_DOUBLE = .6f; //offset for icons to the right of each other
//    private const float X_OFFSET_TRIPLE = 1.2f; // offset for the outer icons in a triple icon arrangement
//    private const float X_OFFSET_FOURTH = 1.8f; // offset for the outer icons in a triple icon arrangement

//    private bool isPlayerFish;

//    private void Start() {
//        //speedEffectSource.enabled = CompareTag("Player"); // enable audio source only for player fish
//        isPlayerFish = CompareTag("Player");
//    }

//    /// <summary>
//    /// get count of active effects, removing null values from the dictionary
//    /// </summary>
//    /// <returns></returns>
//    public int GetActiveEffectCnt() {
//        foreach (FishEffectType type in Enum.GetValues(typeof(FishEffectType))) {
//            if (activeEffects.TryGetValue(type, out Effect effect) && effect == null)
//                activeEffects.Remove(type);
//        }

//        return activeEffects.Count;
//    }

//    //public int GetActiveVisiblyEffectCnt() {
//    //    int cnt = activeEffects.Count;

//    //    if (activeEffects.ContainsKey(FishEffectType.StunInitiator))//stunInitiator isnt visible in icons
//    //        --cnt;
//    //    //    Shield,
//    //    //CurrentSpeed,
//    //    //Stun,
//    //    //StunInitiator,
//    //    //SprintSpeed,
//    //    //FreezeTime

//    //    return cnt;
//    //}

//    /// <summary>
//    /// count of effects that may be visualized in ui
//    /// </summary>
//    /// <returns></returns>
//    //public int ActiveEffectsCnt() {
//    //    int cnt = 0;
//    //    foreach (Effect effect in activeEffects.Values)//because keys is stay in dic even the val were removed
//    //        cnt = (effect != null) ? ++cnt : cnt;

//    //    //if (activeEffects.ContainsKey(FishEffectType.StunInitiator)/* && activeEffects.ContainsValue(activeEffects[FishEffectType.StunInitiator])*/)
//    //    //    --cnt;

//    //    return cnt;
//    //}

//    /*Type*/
//    bool IsEffectComponentExistByType(FishEffectType type, out Effect effect) {
//        //Type outType;
//        switch (type) {
//            case FishEffectType.Shield:
//                return TryGetComponent(out /*ShieldEffect s*/effect);//typeof(ShieldEffect);
//            case FishEffectType.CurrentSpeed:
//                return TryGetComponent(out /*CurrentEffect c*/effect);//typeof(CurrentEffect);
//            case FishEffectType.FreezeTime:
//                return TryGetComponent(out /*StopTimeEffect t*/effect);//typeof(StopTimeEffect);
//            case FishEffectType.StunInitiator:
//                return TryGetComponent(out /*StunInitiatorEffet i*/effect);//typeof(StunInitiatorEffet);
//            case FishEffectType.Poison:
//                return TryGetComponent(out /*PoisonEffect p*/effect);//typeof(PoisonEffect);
//            case FishEffectType.Stun:
//                return TryGetComponent(out /*StunEffect st*/effect); //typeof(StunEffect);
//            case FishEffectType.SprintSpeed:
//                return TryGetComponent(out /*SprintEffect sp*/effect); ;//typeof(SprintEffect);
//            default:
//                effect = null;
//                return false;
//                //    outType = type switch {
//                //FishEffectType.Shield => typeof(ShieldEffect),
//                //FishEffectType.CurrentSpeed => typeof(CurrentEffect),
//                //FishEffectType.FreezeTime => typeof(StopTimeEffect),
//                //FishEffectType.StunInitiator => typeof(StunInitiatorEffet),
//                //FishEffectType.Poison => typeof(PoisonEffect),
//                //FishEffectType.Stun => typeof(StunEffect),
//                //FishEffectType.SprintSpeed => typeof(SprintEffect),
//                //_ => null
//                //};
//        }
//    }

//    public void ApplyEffect(FishEffectType type, float duration) {
//        if (activeEffects.TryGetValue(type, out Effect existing) && existing != null/* && existing.isActiveAndEnabled*/) {//if effect already exists and active, refresh it
//            existing.Refresh();
//            //Debug.Log("refreched " + type); 
//            //if (CompareTag("Player") && type == FishEffectType.Poison)
//            //Debug.LogWarning("R");//Poison effect Refreshed
//        }
//        else {//than add effect
//            //Fish fish = GetComponent<Fish>();
//            if (fish == null)
//                return;

//            //if (CompareTag("Player") && type == FishEffectType.Poison)
//            //    Debug.LogWarning("A");//Poison effect Added

//            Effect newEffect = (IsEffectComponentExistByType(type, out newEffect)) ? newEffect : AddEffectComponent(type);
//            if (newEffect != null) {
//                newEffect.Init(fish, duration);

//                if (type != FishEffectType.StunInitiator)// because this effect does not require an icon as it does not affect its owner
//                    activeEffects.TryAdd(type, newEffect);

//                //Debug.Log(/*activeEffects.Count*/ActiveEffectsCnt());

//                UpdateIconVisialization();

//                //event OnEffeñtExpire will always be triggered while the effect exists, so during this event,
//                //to correctly update the changed icons considering the not yet destroyed one, we will manually remove it from the list
//                newEffect.OnEffeñtExpire += RemoveEffect;

//                const int IMPOSSIBLE_EFFECTS_CNT = 4;
//                if (activeEffects.ContainsKey(FishEffectType.SprintSpeed) && activeEffects.ContainsKey(FishEffectType.CurrentSpeed))
//                    GPG_Manager.OnTriggerAchievement(AchievementKey.FlashSpeed);

//                if (activeEffects.Count >= IMPOSSIBLE_EFFECTS_CNT)
//                    GPG_Manager.OnTriggerAchievement(AchievementKey.Impossible);

//                OnEffectChanges?.Invoke(type, true);
//            }
//        }
//    }

//    public void RemoveEffect(FishEffectType type) {
//        if (activeEffects.ContainsKey(type))
//            activeEffects[type].OnEffeñtExpire -= RemoveEffect;

//        /*bool removed =*/
//        activeEffects.Remove(type);
//        //Debug.Log($"Removed: {removed}, ContainsKey: {activeEffects.ContainsKey(type)}, Count: {activeEffects.Count}");

//        //Debug.LogWarning("Eff removing...");
//        UpdateIconVisialization();
//        OnEffectChanges?.Invoke(type, false);
//    }

//    /// <summary>
//    /// update icons visualization and positions above the enemy fish (sprites in the world space)
//    /// </summary>
//    /// <exception cref="NotImplementedException"></exception>
//    void UpdateIconVisialization() {
//        int effectsCnt = activeEffects.Count;
//        if (activeEffects.ContainsKey(FishEffectType.SprintSpeed))
//            --effectsCnt;
//        if (activeEffects.ContainsKey(FishEffectType.StunInitiator))
//            --effectsCnt;
//        if (activeEffects.ContainsKey(FishEffectType.FreezeTime))
//            --effectsCnt;
//        //Debug.Log("Need to change cnt sys");

//        float[] XOffsets =
//        //icon position indexes depending on the number of active effects
//        effectsCnt switch {
//            1 => new float[1] { 0f },
//            2 => new float[2] { -X_OFFSET_DOUBLE, X_OFFSET_DOUBLE },
//            3 => new float[3] { -X_OFFSET_TRIPLE, 0, X_OFFSET_TRIPLE },
//            4 => new float[4] { -X_OFFSET_FOURTH, -X_OFFSET_DOUBLE, X_OFFSET_DOUBLE, X_OFFSET_FOURTH },
//            _ => new float[1] { 0f }
//        };

//        //icon activation and positioning 
//        shieldIcon.SetActive(false);
//        stunIcon.SetActive(false);
//        currentIcon.SetActive(false);
//        poisonIcon.SetActive(false);

//        int index = 0;
//        foreach (FishEffectType t in activeEffects.Keys)
//            if (activeEffects.TryGetValue(t, out Effect v) && v != null && t != FishEffectType.SprintSpeed
//                && t != FishEffectType.StunInitiator && t != FishEffectType.FreezeTime) {//sprint speed only for player fish, no icon
//                switch (t) {
//                    case FishEffectType.Shield:
//                        shieldIcon.SetActive(true);
//                        shieldIcon.transform.localPosition = new Vector3(XOffsets[index], Y_OFFSET, 0f);
//                        break;
//                    case FishEffectType.Stun:
//                        stunIcon.SetActive(true);
//                        stunIcon.transform.localPosition = new Vector3(XOffsets[index], Y_OFFSET, 0f);
//                        break;
//                    case FishEffectType.CurrentSpeed:
//                        currentIcon.SetActive(true);
//                        currentIcon.transform.localPosition = new Vector3(XOffsets[index], Y_OFFSET, 0f);
//                        break;
//                    case FishEffectType.Poison:
//                        poisonIcon.SetActive(true);
//                        poisonIcon.transform.localPosition = new Vector3(XOffsets[index], Y_OFFSET, 0f);
//                        break;
//                    default:
//                        return;// throw new NotImplementedException($"Effect type {t} is not implemented.");
//                }

//                index++;
//            }
//    }

//    private Effect AddEffectComponent(FishEffectType type) {
//        //OnEffectChanges?.Invoke(type, true);

//        switch (type) {
//            case FishEffectType.Shield:
//                if (shieldObj == null) //for sharks and other big fish
//                    return null;

//                ShieldEffect shieldEffect = gameObject.AddComponent<ShieldEffect>();
//                shieldEffect.Init(shieldObj);
//                return shieldEffect;

//            case FishEffectType.CurrentSpeed:
//                CurrentEffect speedEffect = gameObject.AddComponent<CurrentEffect>();
//                speedEffect.Init(currentBubblesWhirl);
//                if (speedEffectSource != null)
//                    speedEffectSource.enabled = isPlayerFish;//for optimization 
//                return speedEffect;

//            case FishEffectType.Stun:
//                return gameObject.AddComponent<StunEffect>();

//            case FishEffectType.StunInitiator:
//                return gameObject.AddComponent<StunInitiatorEffet>();

//            case FishEffectType.SprintSpeed:
//                SprintEffect sprintEffect = gameObject.AddComponent<SprintEffect>();
//                sprintEffect.Init(waterTrail, fishAnim);
//                if (speedEffectSource != null)
//                    speedEffectSource.enabled = isPlayerFish;
//                return sprintEffect;

//            case FishEffectType.FreezeTime:
//                //Debug.LogWarning("Stop Time Effect applying");
//                if (!isPlayerFish)
//                    return null;//only player can have this effect

//                StopTimeEffect stopTimeEffect = gameObject.AddComponent<StopTimeEffect>();
//                stopTimeEffect.Init(fishRenderer.material, fishAnim.FishAnimator);
//                return stopTimeEffect;

//            case FishEffectType.Poison:
//                if (fish.Level == FishLevel.Cuttlefish)//cuttlefish has antidote
//                    return null;

//                return gameObject.AddComponent<PoisonEffect>();

//            default:
//                throw null/*new NotImplementedException($"Effect type {type} is not implemented.")*/;
//        }
//        //return type switch {
//        //    FishEffectType.Shield => gameObject.AddComponent<ShieldEffect>(),
//        //    FishEffectType.SpeedBoost => gameObject.AddComponent<SpeedEffect>(),
//        //    _ => throw new NotImplementedException($"Effect type {type} is not implemented.")
//        //};
//    }

//    /// <summary>
//    /// for current or speed audiosurce volume dynamic changing
//    /// </summary>
//    private void FixedUpdate() {
//        const float MIN_VOLUME = 0.01f; // minimum volume for the speed effect audio source
//        const float VOLUME_FADE_SPEED = 4f; // speed of volume fade

//        if (!fish.IsActive)
//            return;

//        //speedEffectSource enabled onky on player fish
//        if (speedEffectSource != null && speedEffectSource.enabled) {
//            float velocity = (prevPos - rb.position).magnitude / Time.fixedDeltaTime;

//            if (activeEffects.ContainsKey(FishEffectType.SprintSpeed) || activeEffects.ContainsKey(FishEffectType.CurrentSpeed)) {

//                prevPos = rb.position; // update previous position for next frame

//                if (velocity != 0f)//acceleration - inc vol
//                    speedEffectSource.volume = Mathf.Lerp(speedEffectSource.volume, 1f, Time.fixedDeltaTime * VOLUME_FADE_SPEED);//Mathf.Clamp01(velocity / VELOCITY_DIVIDER); //adjust volume based on speed, max volume at 10 m/s
//                else//braking - dec vol
//                    speedEffectSource.volume = Mathf.Lerp(speedEffectSource.volume, 0, Time.fixedDeltaTime * VOLUME_FADE_SPEED);
//                //Debug.Log($"Speed effect volume: {velocity}");
//            }

//            else {
//                speedEffectSource.volume = Mathf.Lerp(speedEffectSource.volume, 0, Time.fixedDeltaTime);

//                if (speedEffectSource.volume < MIN_VOLUME &&  //if volume is too low, disable the audio source
//                    (!activeEffects.ContainsKey(FishEffectType.SprintSpeed) && !activeEffects.ContainsKey(FishEffectType.CurrentSpeed)))
//                    speedEffectSource.enabled = false;
//            }
//        }
//    }

//    private void OnDestroy() {
//        foreach (FishEffectType type in activeEffects.Keys)
//            activeEffects[type].OnEffeñtExpire -= RemoveEffect;
//    }

//    private void OnDisable() {
//        foreach (FishEffectType type in Enum.GetValues(typeof(FishEffectType))) {
//            if (activeEffects.TryGetValue(type, out Effect e))
//                e.OnEffeñtExpire -= RemoveEffect;

//            if (activeEffects.TryGetValue(type, out Effect effect)) {
//                activeEffects.Remove(type);
//                Destroy(effect);
//            }
//        }
//    }
//}
