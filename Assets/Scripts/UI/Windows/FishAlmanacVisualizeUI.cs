using TMPro;
using UnityEngine;
using VContainer;


public interface IFishAlmanacVisualizeUI {
    void InitWnd(FishLevel fishLevel);
}

public class FishAlmanacVisualizeUI : MonoBehaviour, IFishAlmanacVisualizeUI {
    [SerializeField] TextMeshProUGUI fishName;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TextMeshProUGUI health;
    [SerializeField] TextMeshProUGUI speed;
    [SerializeField] TextMeshProUGUI responseRadius;
    [SerializeField] TextMeshProUGUI biteDamage;
    [SerializeField] TextMeshProUGUI isHasShield;
    [SerializeField] TextMeshProUGUI isHasPoisonImmun;
    [SerializeField] TextMeshProUGUI isAggressive;
    [SerializeField] ProgressBar healthBar;
    [SerializeField] ProgressBar speedBar;
    [SerializeField] ProgressBar responseBar;
    [SerializeField] ProgressBar biteBar;

    [SerializeField] Transform fishesParent;
    [SerializeField] GameObject dartfish;
    [SerializeField] GameObject crab;
    [SerializeField] GameObject seabass;
    [SerializeField] GameObject mackerel;
    [SerializeField] GameObject moray;
    [SerializeField] GameObject lionfish;
    [SerializeField] GameObject barracuda;
    [SerializeField] GameObject tuna;
    [SerializeField] GameObject hammer;
    [SerializeField] GameObject tiger;
    [SerializeField] GameObject white;
    [SerializeField] GameObject cuttle;
    [SerializeField] GameObject puffer;

    [Inject] private readonly SanctuaryUIInfo _sanctuaryUIInfo;
    [Inject] private readonly FishAllInfo _fishInfoManager;

    private const string TEXT_RED_TAG = "<color=red>";
    private const string TEXT_GREEN_TAG = "<color=green>";

    private float FISH_ROTATE_SPEED = 20f;
    private float maxHealth = 0f;
    private float maxSpeed = 0f;
    private float maxResponse = 0f;
    private float maxBite = 0f;


    private void Start() {
        //init max stats
        foreach (FishLevel fish in GameStaticInfo.AllFishLevels) {
            FishInfo currentFish = _fishInfoManager.GetFishInfo(fish);

            if (maxHealth < currentFish.MaxHealth)
                maxHealth = currentFish.MaxHealth;
            if (maxSpeed < currentFish.Speed)
                maxSpeed = currentFish.Speed;
            if (maxResponse < currentFish.ResponseRadius)
                maxResponse = currentFish.ResponseRadius;
            if (maxBite < currentFish.BiteDamage)
                maxBite = currentFish.BiteDamage;
        }
    }

    public void InitWnd(FishLevel fishLevel) {
        FishInfo currentFishInfo = _fishInfoManager.GetFishInfo(fishLevel);
        DisableFishObjects();
        GetObjByFish(fishLevel).SetActive(true);

        fishName.text = currentFishInfo.Tittle.GetLocalizedString();
        description.text = currentFishInfo.Description.GetLocalizedString();

        health.text = currentFishInfo.MaxHealth.ToString() + _sanctuaryUIInfo.Units.GetLocalizedString();
        healthBar.SetFillAmount(currentFishInfo.MaxHealth / maxHealth);

        speed.text = currentFishInfo.Speed.ToString("F1") + _sanctuaryUIInfo.MetersPerSec.GetLocalizedString();
        speedBar.SetFillAmount(currentFishInfo.Speed / maxSpeed);

        responseRadius.text = currentFishInfo.ResponseRadius.ToString("F1") + _sanctuaryUIInfo.Meters.GetLocalizedString();
        responseBar.SetFillAmount(currentFishInfo.ResponseRadius / maxResponse);

        biteDamage.text = currentFishInfo.BiteDamage.ToString("F1") + _sanctuaryUIInfo.Units.GetLocalizedString();
        biteBar.SetFillAmount(currentFishInfo.BiteDamage / maxBite);

        isHasShield.text = GetYesNoLabel(currentFishInfo.IsHaveShield);
        isHasPoisonImmun.text = GetYesNoLabel(currentFishInfo.Level <= GameStaticInfo.POISON_IMMUNE_FISH_TRIGGER);
        isAggressive.text = GetYesNoLabel(currentFishInfo.Level > GameStaticInfo.PEACEFUL_FISH_LVL);
    }

    string GetYesNoLabel(bool isYes) {
        return isYes ?
            TEXT_GREEN_TAG + _sanctuaryUIInfo.YesStr.GetLocalizedString() :
            TEXT_RED_TAG + _sanctuaryUIInfo.NoStr.GetLocalizedString();
    }

    GameObject GetObjByFish(FishLevel fishLevel) {
        return fishLevel switch {
            FishLevel.Dartfish => dartfish,
            FishLevel.Crab => crab,
            FishLevel.SeaBass => seabass,
            FishLevel.Mackerel => mackerel,
            FishLevel.Moray => moray,
            FishLevel.Lionfish => lionfish,
            FishLevel.Barracuda => barracuda,
            FishLevel.Tuna => tuna,
            FishLevel.HammerShark => hammer,
            FishLevel.TigerShark => tiger,
            FishLevel.WhiteShark => white,
            FishLevel.Cuttlefish => cuttle,
            FishLevel.Pufferfish => puffer,
            _ => null
        };
    }

    void DisableFishObjects() {
        dartfish.SetActive(false);
        crab.SetActive(false);
        seabass.SetActive(false);
        mackerel.SetActive(false);
        moray.SetActive(false);
        lionfish.SetActive(false);
        barracuda.SetActive(false);
        tuna.SetActive(false);
        hammer.SetActive(false);
        tiger.SetActive(false);
        white.SetActive(false);
        cuttle.SetActive(false);
        puffer.SetActive(false);
    }

    private void Update() {
        fishesParent.Rotate(Vector3.up, FISH_ROTATE_SPEED * DJTime.UnscaledDeltaTime);
    }
}
