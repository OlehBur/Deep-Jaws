using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class EffectsBarView : MonoBehaviour {
    [SerializeField] RectTransform iconsParent;
    [SerializeField] RectTransform shieldIcon;
    [SerializeField] RectTransform stunIcon;
    [SerializeField] RectTransform sprintIcon;
    [SerializeField] RectTransform currentIcon;
    [SerializeField] RectTransform timeIcon;
    [SerializeField] RectTransform poisonIcon;
    [SerializeField] RectTransform blindnessIcon;
    [SerializeField] Image shieldIconImg;
    [SerializeField] Image stunIconImg;
    [SerializeField] Image sprintIconImg;
    [SerializeField] Image currentIconImg;
    [SerializeField] Image timeIconImg;
    [SerializeField] Image poisonIconImg;
    [SerializeField] Image blindnessIconImg;


    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;

    public static readonly Vector2[] iconsPositionsMultipliers1 = new Vector2[1] { new Vector2(.5f, .7f) };
    public static readonly Vector2[] iconsPositionsMultipliers2 = new Vector2[2]{
            new Vector2(.35f,.7f),
            new Vector2(.65f,.7f)
    };
    public static readonly Vector2[] iconsPositionsMultipliers3 = new Vector2[3] {
            new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f)
    };
    public static readonly Vector2[] iconsPositionsMultipliers4 = new Vector2[4] {
            new Vector2(.35f, .2f),
            new Vector2(.65f, .2f),
            new Vector2(.35f, .7f),
            new Vector2(.65f, .7f),
    };
    public static readonly Vector2[] iconsPositionsMultipliers5 = new Vector2[5] {
            new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f),
            new Vector2(.35f, .2f),
            new Vector2(.65f, .2f),
    };
    public static readonly Vector2[] iconsPositionsMultipliers6 = new Vector2[6] {
        new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f),
            new Vector2(.17f, .2f),
            new Vector2(.5f, .2f),
            new Vector2(.83f, .2f),
    };
    public static readonly Vector2[] iconsPositionsMultipliers7 = new Vector2[7] {
        new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f),
            new Vector2(.17f, .2f),
            new Vector2(.5f, .2f),
            new Vector2(.83f, .2f),
            new Vector2(.5f, -.3f),
    };
    public static readonly Vector2[][] iconsPositionsMultipliers = new Vector2[7][] {
        iconsPositionsMultipliers1,
        iconsPositionsMultipliers2,
        iconsPositionsMultipliers3,
        iconsPositionsMultipliers4,
        iconsPositionsMultipliers5,
        iconsPositionsMultipliers6,
        iconsPositionsMultipliers7
    };

    private readonly Dictionary<FishEffectType, RectTransform> _effectsSlotsMatch = new Dictionary<FishEffectType, RectTransform> { };

    private EffectManager _playerEffectManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _effectsSlotsMatch[FishEffectType.Shield] = shieldIcon;
        _effectsSlotsMatch[FishEffectType.Stun] = stunIcon;
        _effectsSlotsMatch[FishEffectType.SprintSpeed] = sprintIcon;
        _effectsSlotsMatch[FishEffectType.CurrentSpeed] = currentIcon;
        _effectsSlotsMatch[FishEffectType.FreezeTime] = timeIcon;
        _effectsSlotsMatch[FishEffectType.Poison] = poisonIcon;
        _effectsSlotsMatch[FishEffectType.Blindness] = blindnessIcon;

        _playerFishPoolService.OnPlayerRespawn += PlayerReConnect;
    }

    private void Update() {
        //activate and draw progress for all effects icons that have player
        EffectsProgressReset(); // reset all icons progress

        if (_playerEffectManager != null && _playerEffectManager.ActiveEffects.Count > 0)
            foreach (KeyValuePair<FishEffectType, Effect> eff in _playerEffectManager.ActiveEffects)
                if (eff.Value == null || !eff.Value.Enabled)
                    IconsUpdBehaviour(eff.Key, false);
                else  //!null & enabled
                    SetEffectProgress(eff.Key, eff.Value);
    }

    void PlayerReConnect(Fish fish, bool isDie) {
        if (_playerEffectManager != null)
            _playerEffectManager.OnEffectChanges -= IconsUpdBehaviour;

        _playerEffectManager = fish.GetComponent<EffectManager>();
        _playerEffectManager.OnEffectChanges += IconsUpdBehaviour;

        DisactivateDefaultIcons();
        EffectsProgressReset();

        //upd icons pos of spawn effects
        foreach (FishEffectType effectType in Enum.GetValues(typeof(FishEffectType)))
            if (_playerEffectManager.ActiveEffects.TryGetValue(effectType, out Effect value) && value != null)
                IconsUpdBehaviour(effectType, true);
    }

    /// <summary>
    /// deactivates icons visualized above the fish , since the player has a more detailed visualization in the interface
    /// </summary>
    void DisactivateDefaultIcons() {
        //Debug.LogWarning("Fish Prefab: Init icons parent in Effect Manager");
        _playerEffectManager.IconsParentObj.SetActive(false);

        if (shieldIcon == null)//or eny other (only for editor debug)
            return;

        shieldIcon.gameObject.SetActive(false);
        stunIcon.gameObject.SetActive(false);
        sprintIcon.gameObject.SetActive(false);
        currentIcon.gameObject.SetActive(false);
        timeIcon.gameObject.SetActive(false);
        poisonIcon.gameObject.SetActive(false);
        blindnessIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// sets the progress of the effect icon that affects the player, 
    /// </summary>
    /// <param name="effectType"></param>
    /// <param name="effect"></param>
    void SetEffectProgress(FishEffectType effectType, Effect effect) {
        float progress = (effect != null) ? effect.LeftTime / effect.Duration : 0f;

        switch (effectType) {
            case FishEffectType.Stun:
            stunIconImg.fillAmount = progress;
            stunIcon.transform.parent.gameObject.SetActive(stunIconImg.fillAmount != 0);
            break;

            case FishEffectType.Shield:
            shieldIconImg.fillAmount = progress;
            shieldIcon.transform.parent.gameObject.SetActive(shieldIconImg.fillAmount != 0);
            break;

            case FishEffectType.CurrentSpeed:
            currentIconImg.fillAmount = progress;
            currentIcon.transform.parent.gameObject.SetActive(currentIconImg.fillAmount != 0);
            break;

            case FishEffectType.SprintSpeed:
            sprintIconImg.fillAmount = progress;
            sprintIcon.transform.parent.gameObject.SetActive(sprintIconImg.fillAmount != 0);
            break;

            case FishEffectType.FreezeTime:
            timeIconImg.fillAmount = progress;
            timeIcon.transform.parent.gameObject.SetActive(timeIconImg.fillAmount != 0);
            break;

            case FishEffectType.Poison:
            poisonIconImg.fillAmount = progress;
            poisonIcon.transform.parent.gameObject.SetActive(poisonIconImg.fillAmount != 0);
            break;

            case FishEffectType.Blindness:
            blindnessIconImg.fillAmount = progress;
            blindnessIcon.transform.parent.gameObject.SetActive(blindnessIconImg.fillAmount != 0);
            break;

        }
    }

    void EffectsProgressReset() {
        shieldIcon.transform.parent.gameObject.SetActive(false);
        stunIcon.transform.parent.gameObject.SetActive(false);
        currentIcon.transform.parent.gameObject.SetActive(false);
        sprintIcon.transform.parent.gameObject.SetActive(false);
        timeIcon.transform.parent.gameObject.SetActive(false);
        poisonIcon.transform.parent.gameObject.SetActive(false);
        blindnessIcon.transform.parent.gameObject.SetActive(false);

        stunIconImg.fillAmount = 0;
        shieldIconImg.fillAmount = 0;
        currentIconImg.fillAmount = 0;
        sprintIconImg.fillAmount = 0;
        timeIconImg.fillAmount = 0;
        poisonIconImg.fillAmount = 0;
        blindnessIconImg.fillAmount = 0;
    }

    /// <summary>
    /// for any additional behaviour (in UI) when an effect is added or removed from the player
    /// </summary>
    /// <param name="type"></param>
    /// <param name="added"></param>
    void IconsUpdBehaviour(FishEffectType type, bool isAdded) {
        if (type == FishEffectType.StunInitiator)
            return;
        //repos
        int effectsCnt = _playerEffectManager.GetActiveEffectCnt(TypeEffects.VisibleUI)/*activeEffects.Count*/;

        if (!isAdded)//if effect expires - hide icon
            _effectsSlotsMatch[type].gameObject.SetActive(false);

        if (effectsCnt > 0) {//reposing
            int effectIconInd = 0;
            foreach (FishEffectType currType in _playerEffectManager.ActiveEffects.Keys) {
                if (!EffectManager.IsVisibleEffect(currType, true)
                    || _playerEffectManager.ActiveEffects[currType] == null || !_playerEffectManager.ActiveEffects[currType].Enabled)
                    continue;

                //Debug.Log(effectsCnt - 1 + " " + effectIconInd);
                Vector2 newPos = new Vector2(
                    iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].x * iconsParent.rect.width,
                    iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].y * iconsParent.rect.height);
                //Debug.LogWarning(iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].x + " "
                //+ iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].y);

                _effectsSlotsMatch[currType].anchoredPosition = newPos;

                effectIconInd = (effectIconInd + 1 < GameStaticInfo.MAX_EFFECTS_CNT) ?
                    ++effectIconInd : effectIconInd;
            }
        }
    }
}
