using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;



public class SanctuaryVisualizeUI : MonoBehaviour {
    [SerializeField] SanctuaryCameraChanger cameraChanger;
    [SerializeField] FishAlmanacVisualizeUI fishAlmanac;
    [SerializeField] FishInfoManager fishInfoManager;

    [SerializeField] GameObject biomesUI;
    [SerializeField] GameObject almanacUI;
    [SerializeField] TextMeshProUGUI shellsCnt;
    [SerializeField] TextMeshProUGUI biomeTittle;
    [SerializeField] GameObject[] biomeHideLockObjects;
    [SerializeField] RectTransform[] stars;
    [SerializeField] TextMeshProUGUI productionPerMinuteTxt;
    [SerializeField] Button reseachButton;
    [SerializeField] TextMeshProUGUI researchButtonTxt;
    [SerializeField] Button boostResearchButton;
    //[SerializeField] TextMeshProUGUI boostResearchButtonTxt;
    [SerializeField] ProgressBar researchProgressBar;
    [SerializeField] TextMeshProUGUI requirementVictim;
    [SerializeField] GameObject requirement;
    [SerializeField] Button[] residentsButtons;
    [SerializeField] Image[] residentsImages;

    [SerializeField] LocalizedString researchNeutralize;
    [SerializeField] LocalizedString researchProg;
    [SerializeField] LocalizedString hourShort;
    [SerializeField] LocalizedString minuteShort;
    [SerializeField] FishLocalized fishLocalized;

    private BiomeWrapper selectedBiome;
    private BiomeType currentBiomeType = BiomeType.CoralReef;
    private SanctuaryData sanctuaryData;

    private readonly float[][] starsXposMultiplier = new float[3][] {
         new float[] { -1f, 0f, 1f },//3 star
         new float[] { -1.5f, - 0.5f, 0.5f, 1.5f },//4 stars
         new float[] { -2f, -1f, 0f, 1f, 2f }//5 stars
    };
    private const int MIN_STARS = 3;


    private void Start() {
        reseachButton.onClick.AddListener(OnResearchCurrentBiome);
        boostResearchButton.onClick.AddListener(OnBoostResearch);

        //init residents buttons
        for (int i = 0; i < residentsButtons.Length; i++) {
            int redisDentInd = i;
            residentsButtons[i].onClick.RemoveAllListeners();
            residentsButtons[i].onClick.AddListener(() => OnOpenAlmanacUI(redisDentInd));
        }

        biomesUI.SetActive(true);
        almanacUI.SetActive(false);

        if (SanctuariesManager.Instance != null) {
            UpdateShellsCnt(SanctuariesManager.Instance.ShellsCnt);
            UpdateSelectedBiomeUI(currentBiomeType);
        }
    }

    private void OnEnable() {
        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnShellsAmountChanged += UpdateShellsCnt;
            SanctuariesManager.Instance.OnResearchUpdated += UpdateResearchProgress;
            SanctuariesManager.Instance.OnBiomeResearched += UpdateUIafterResearch;
        }

        sanctuaryData = ProgressSaver.IsSanctuaryDataExist ?
            ProgressSaver.LoadSanctuaryData() : new SanctuaryData();
        sanctuaryData.isDartfishNeutralized = true;
        sanctuaryData.isCrabNeutralized = true;
        sanctuaryData.isSeabassNeutralized = true;
        sanctuaryData.isMackarelNeutralized = true;
        sanctuaryData.isHammerheadNeutralized = true;
        Debug.LogWarning("Activate sone neutralization for testing");
    }
    private void OnDisable() {
        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnShellsAmountChanged -= UpdateShellsCnt;
            SanctuariesManager.Instance.OnResearchUpdated -= UpdateResearchProgress;
            SanctuariesManager.Instance.OnBiomeResearched -= UpdateUIafterResearch;
        }
    }

    private void OnOpenAlmanacUI(int index) {
        FishLevel fishLevel = GetFishResidentByIndex(index);
        fishAlmanac.InitWnd(fishLevel);
        cameraChanger.EnableAlmanacCamera();

        biomesUI.SetActive(false);
        almanacUI.SetActive(true);
    }

    void UpdateResidentImagesPreview() {// only diff residents shows
        int lastSpriteIndex = 0;
        int residentInd = 0;
        residentsImages[lastSpriteIndex].sprite = fishInfoManager.GetFishSprite(selectedBiome.Info.Requirements[residentInd]);
        residentsButtons[lastSpriteIndex].gameObject.SetActive(true);
        for (residentInd = 1; residentInd < selectedBiome.Level/*Info.Requirements.Length*/; residentInd++) {
            FishLevel newFish = selectedBiome.Info.Requirements[residentInd];
            FishLevel previousFish = selectedBiome.Info.Requirements[residentInd - 1];

            if (newFish != previousFish)
                ++lastSpriteIndex;

            residentsImages[lastSpriteIndex].sprite = fishInfoManager.GetFishSprite(newFish);
            residentsButtons[lastSpriteIndex].gameObject.SetActive(true);
        }

        for (int i = lastSpriteIndex + 1; i < residentsImages.Length; i++) {
            residentsImages[i].sprite = null;
            residentsButtons[i].gameObject.SetActive(false);
        }
    }

    FishLevel GetFishResidentByIndex(int index) {
        //if (index < 0 || index >= selectedBiome.Info.Requirements.Length)
        //    return FishLevel.WhiteShark;
        FishLevel actualResident = selectedBiome.Info.Requirements[0];
        if (index == 0)
            return actualResident;

        for (int r = 1, actual = 0; r < selectedBiome.Info.Requirements.Length; r++) {
            if (actualResident != selectedBiome.Info.Requirements[r]) {
                actualResident = selectedBiome.Info.Requirements[r];
                ++actual;

                if (actual == index)
                    break;
            }
        }

        return actualResident;
    }

    public void OnCloseAlmanacUI() {
        cameraChanger.DisableAlmanacCamera();
        biomesUI.SetActive(true);
        almanacUI.SetActive(false);
    }

    private void UpdateSelectedBiomeUI(BiomeType currentBiomeType) {
        //if (SanctuariesManager.Instance != null) {
        if (currentBiomeType < BiomeType.CoralReef)//infinity scroll
            currentBiomeType = BiomeType.OceanicAbyss;
        if (currentBiomeType > BiomeType.OceanicAbyss)
            currentBiomeType = BiomeType.CoralReef;

        if (this.currentBiomeType != currentBiomeType)
            cameraChanger.OnChangeBiomeCamera(currentBiomeType);

        this.currentBiomeType = currentBiomeType;
        selectedBiome = SanctuariesManager.Instance.GetBiomeWrapper(currentBiomeType);

        biomeTittle.text = selectedBiome.Info.Tittle.GetLocalizedString();
        researchButtonTxt.text = selectedBiome.GetPriceToUpgrade().ToString();

        if (selectedBiome.NotReserched) {
            foreach (GameObject obj in biomeHideLockObjects)
                obj.SetActive(false);

            UpdateResearchProgress(currentBiomeType, selectedBiome.TimeToUpgrade);
            foreach (Button button in residentsButtons)//disable residents buttons
                button.gameObject.SetActive(false);
        }
        else {
            foreach (GameObject obj in biomeHideLockObjects)
                obj.SetActive(true);

            UpdateBiomeNonStatInfo();
            UpdateResidentImagesPreview();

            if (selectedBiome.Level == selectedBiome.Info.MaxLevel) {
                reseachButton.gameObject.SetActive(false);
                boostResearchButton.gameObject.SetActive(false);
                researchProgressBar.gameObject.SetActive(false);
                requirement.SetActive(false);
            }
        }
        //}
    }

    void SetRequirement() {
        if (selectedBiome.Level >= selectedBiome.Info.MaxLevel) {
            requirement.SetActive(false);
            return;
        }
        FishLevel requiredFishType = selectedBiome.Info.Requirements[selectedBiome.Level];

        requirementVictim.text = researchNeutralize.GetLocalizedString() + " " + fishLocalized.GetFishLocalizeByType(requiredFishType);
    }

    public void OnNextBiome() => UpdateSelectedBiomeUI(currentBiomeType + 1);
    public void OnPreviousBiome() => UpdateSelectedBiomeUI(currentBiomeType - 1);

    void OnResearchCurrentBiome() {
        selectedBiome.OnStartUpgrade();
        UpdateResearchProgress(selectedBiome.Info.Type, selectedBiome.TimeToUpgrade);
    }
    void OnBoostResearch() {
        selectedBiome.BoostResearch();
        UpdateResearchProgress(selectedBiome.Info.Type, selectedBiome.TimeToUpgrade);
    }

    void UpdateBiomeNonStatInfo() {
        UpdateStars(selectedBiome.Level, selectedBiome.Info.MaxLevel);
        productionPerMinuteTxt.text = selectedBiome.GetProductionPerMinute().ToString() + "/" + minuteShort.GetLocalizedString();
        researchButtonTxt.text = selectedBiome.GetPriceToUpgrade().ToString();

        UpdateResearchProgress(selectedBiome.Info.Type, selectedBiome.TimeToUpgrade);
    }

    void UpdateStars(int currentLevel, int maxLevel) {
        for (int i = 0; i < stars.Length; i++) {
            if (i < maxLevel) {
                stars[i].gameObject.SetActive(true);
                stars[i].anchoredPosition = new Vector2(starsXposMultiplier[maxLevel - MIN_STARS][i] * stars[i].rect.width, stars[i].anchoredPosition.y);
                stars[i].GetChild(0).gameObject.SetActive(i < currentLevel);//fill star if current level >= index
            }
            else
                stars[i].gameObject.SetActive(false);
        }
    }



    void UpdateUIafterResearch(BiomeType type, int lvl) {
        if (currentBiomeType == type) {
            foreach (GameObject obj in biomeHideLockObjects)//todo: call by research event
                obj.SetActive(true);

            UpdateBiomeNonStatInfo();
            UpdateResidentImagesPreview();
        }
    }

    void UpdateResearchProgress(BiomeType biomeType, int timeToUpgrade) {
        if (selectedBiome == null)
            selectedBiome = SanctuariesManager.Instance.GetBiomeWrapper(currentBiomeType);

        if (biomeType == currentBiomeType) {
            if (selectedBiome.IsUpgrading) {
                reseachButton.gameObject.SetActive(false);
                researchProgressBar.gameObject.SetActive(true);
                boostResearchButton.gameObject.SetActive(true);
                requirement.SetActive(false); //disable requirements
            }
            else {
                //requirement check
                if (selectedBiome.IsRequirementMet(sanctuaryData)) {
                    requirement.SetActive(false);
                    reseachButton.interactable = true;
                }
                else {
                    SetRequirement();
                    requirement.SetActive(true);
                    reseachButton.interactable = false;
                }

                //progress bar and boost button hide
                researchProgressBar.gameObject.SetActive(false);
                boostResearchButton.gameObject.SetActive(false);

                if (selectedBiome.Level < selectedBiome.Info.MaxLevel)
                    reseachButton.gameObject.SetActive(true);

                return;
            }

            float maxTimeToUpgradeSec = selectedBiome.Info.UpgradeTimeByLevel_Hours[selectedBiome.Level] * GameStaticInfo.SECONDS_IN_HOUR;
            researchProgressBar.SetFillAmount((float)(maxTimeToUpgradeSec - timeToUpgrade) / maxTimeToUpgradeSec);
            researchProgressBar.SetText(researchProg.GetLocalizedString() + " " + GetTimeString(timeToUpgrade));
        }
    }

    string GetTimeString(int seconds) {
        int hours = seconds / GameStaticInfo.SECONDS_IN_HOUR;
        int minutes = (seconds % GameStaticInfo.SECONDS_IN_HOUR) / GameStaticInfo.SECONDS_IN_MINUTE;
        if (hours > 0)
            return hours + hourShort.GetLocalizedString() + " " + minutes + minuteShort.GetLocalizedString() + "...";
        else
            return minutes + minuteShort.GetLocalizedString() + "...";
    }

    private void UpdateShellsCnt(int currentWheat) =>
        shellsCnt.text = currentWheat.ToString();

}