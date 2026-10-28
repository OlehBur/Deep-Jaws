using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using VContainer;



public class SanctuaryVisualizeUI : MonoBehaviour {
    [SerializeField] SanctuaryCameraChanger cameraChanger;
    [SerializeField] FishAlmanacVisualizeUI fishAlmanac;
    [SerializeField] FishSpriteInfo fishSpriteInfo;

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

    [Inject] private readonly ISanctuariesManager _sanctuariesManager;

    /// <summary>
    /// only for check requirements once per load scene
    /// </summary>
    private SanctuaryData _sanctuaryData;
    private BiomeWrapper _selectedBiome;
    private BiomeType _currentBiomeType = BiomeType.CoralReef;

    private readonly float[][] starsXposMultiplier = new float[3][] {
         new float[] { -1f, 0f, 1f },//3 star
         new float[] { -1.5f, - 0.5f, 0.5f, 1.5f },//4 stars
         new float[] { -2f, -1f, 0f, 1f, 2f }//5 stars
    };
    private const int MIN_STARS = 3;


    private void Start() {
        reseachButton.onClick.AddListener(OnResearchCurrentBiome);

        //init residents buttons
        for (int i = 0; i < residentsButtons.Length; i++) {
            int redisDentInd = i;
            residentsButtons[i].onClick.RemoveAllListeners();
            residentsButtons[i].onClick.AddListener(() => OnOpenAlmanacUI(redisDentInd));
        }

        biomesUI.SetActive(true);
        almanacUI.SetActive(false);

        if (_sanctuariesManager != null) {
            UpdateShellsCnt(_sanctuariesManager.ShellsCnt);
            UpdateSelectedBiomeUI(_currentBiomeType);
        }

        if (GameAdsManager.Instance != null)
            boostResearchButton.onClick.AddListener(() => GameAdsManager.Instance.OnBoostClicked(_currentBiomeType));
    }

    private void OnEnable() {
        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnShellsAmountChanged += UpdateShellsCnt;
            _sanctuariesManager.OnResearchUpdated += UpdateResearchProgress;
            _sanctuariesManager.OnBiomeResearched += UpdateUIafterResearch;
        }
#if UNITY_ANDROID
        if (GameAdsManager.Instance != null)
            GameAdsManager.Instance.OnAdButtonsInteractable += BoostInteractable;
#endif

        _sanctuaryData = ProgressSaver.IsSanctuaryDataExist ?
            ProgressSaver.LoadSanctuaryData() : new SanctuaryData();
    }

    private void OnDisable() {
        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnShellsAmountChanged -= UpdateShellsCnt;
            _sanctuariesManager.OnResearchUpdated -= UpdateResearchProgress;
            _sanctuariesManager.OnBiomeResearched -= UpdateUIafterResearch;
        }
#if UNITY_ANDROID
        if (GameAdsManager.Instance != null)
            GameAdsManager.Instance.OnAdButtonsInteractable -= BoostInteractable;
#endif

        reseachButton.onClick.RemoveAllListeners();
        boostResearchButton.onClick.RemoveAllListeners();
    }

    void BoostInteractable(bool interactable) =>
        boostResearchButton.interactable = interactable;

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
        residentsImages[lastSpriteIndex].sprite = fishSpriteInfo.GetFishSprite(_selectedBiome.Info.Requirements[residentInd]);
        residentsButtons[lastSpriteIndex].gameObject.SetActive(true);
        for (residentInd = 1; residentInd < _selectedBiome.Level/*Info.Requirements.Length*/; residentInd++) {
            FishLevel newFish = _selectedBiome.Info.Requirements[residentInd];
            FishLevel previousFish = _selectedBiome.Info.Requirements[residentInd - 1];

            if (newFish != previousFish)
                ++lastSpriteIndex;

            residentsImages[lastSpriteIndex].sprite = fishSpriteInfo.GetFishSprite(newFish);
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
        FishLevel actualResident = _selectedBiome.Info.Requirements[0];
        if (index == 0)
            return actualResident;

        for (int r = 1, actual = 0; r < _selectedBiome.Info.Requirements.Length; r++) {
            if (actualResident != _selectedBiome.Info.Requirements[r]) {
                actualResident = _selectedBiome.Info.Requirements[r];
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
        if (_sanctuariesManager != null) {
            if (currentBiomeType < BiomeType.CoralReef)//infinity scroll
                currentBiomeType = BiomeType.OceanicAbyss;
            if (currentBiomeType > BiomeType.OceanicAbyss)
                currentBiomeType = BiomeType.CoralReef;

            if (_currentBiomeType != currentBiomeType)
                cameraChanger.OnChangeBiomeCamera(currentBiomeType);

            _currentBiomeType = currentBiomeType;
            _selectedBiome = _sanctuariesManager.GetBiomeWrapper(currentBiomeType);

            biomeTittle.text = _selectedBiome.Info.Tittle.GetLocalizedString();
            researchButtonTxt.text = _selectedBiome.GetPriceToUpgrade().ToString();
            reseachButton.interactable = _selectedBiome.GetPriceToUpgrade() <= _sanctuariesManager.ShellsCnt;
            if (_selectedBiome.NotReserched) {
                foreach (GameObject obj in biomeHideLockObjects)
                    obj.SetActive(false);

                UpdateResearchProgress(currentBiomeType, _selectedBiome.TimeToUpgrade);
                foreach (Button button in residentsButtons)//disable residents buttons
                    button.gameObject.SetActive(false);
            }
            else {
                foreach (GameObject obj in biomeHideLockObjects)
                    obj.SetActive(true);

                UpdateBiomeNonStatInfo();
                UpdateResidentImagesPreview();

                if (_selectedBiome.Level >= _selectedBiome.Info.MaxLevel) {
                    reseachButton.gameObject.SetActive(false);
                    boostResearchButton.gameObject.SetActive(false);
                    researchProgressBar.gameObject.SetActive(false);
                    requirement.SetActive(false);
                }
            }
        }
    }

    void SetRequirement() {
        if (_selectedBiome.Level >= _selectedBiome.Info.MaxLevel) {
            requirement.SetActive(false);
            return;
        }
        FishLevel requiredFishType = _selectedBiome.Info.Requirements[_selectedBiome.Level];

        requirementVictim.text = researchNeutralize.GetLocalizedString() + " " + fishLocalized.GetFishLocalizeByType(requiredFishType);
    }

    public void OnNextBiome() => UpdateSelectedBiomeUI(_currentBiomeType + 1);
    public void OnPreviousBiome() => UpdateSelectedBiomeUI(_currentBiomeType - 1);

    void OnResearchCurrentBiome() {
        _sanctuariesManager.OnAddShells(-_selectedBiome.GetPriceToUpgrade());
        _selectedBiome.OnStartUpgrade();

        //selectedBiome.BoostResearch();
        //if (selectedBiome.Level == selectedBiome.Info.MaxLevel)
        //    reseachButton.gameObject.SetActive(false);
        //Debug.LogWarning("ACtivate one click research");

        UpdateResearchProgress(_selectedBiome.Info.Type, _selectedBiome.TimeToUpgrade);
    }

    void UpdateBiomeNonStatInfo() {
        UpdateStars(_selectedBiome.Level, _selectedBiome.Info.MaxLevel);
        productionPerMinuteTxt.text = _selectedBiome.GetProductionPerMinute().ToString() + "/" + minuteShort.GetLocalizedString();
        researchButtonTxt.text = _selectedBiome.GetPriceToUpgrade().ToString();
        reseachButton.interactable = _selectedBiome.GetPriceToUpgrade() <= _sanctuariesManager.ShellsCnt;

        UpdateResearchProgress(_selectedBiome.Info.Type, _selectedBiome.TimeToUpgrade);
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
        if (_currentBiomeType == type) {
            foreach (GameObject obj in biomeHideLockObjects)//todo: call by research event
                obj.SetActive(true);

            UpdateBiomeNonStatInfo();
            UpdateResidentImagesPreview();
        }
    }

    void UpdateResearchProgress(BiomeType biomeType, int timeToUpgrade) {
        if (_selectedBiome == null)
            _selectedBiome = _sanctuariesManager.GetBiomeWrapper(_currentBiomeType);

        if (biomeType == _currentBiomeType) {
            if (_selectedBiome.IsUpgrading) {
                reseachButton.gameObject.SetActive(false);
                researchProgressBar.gameObject.SetActive(true);
                boostResearchButton.gameObject.SetActive(true);
                requirement.SetActive(false); //disable requirements
            }
            else {
                //requirement check
                if (_selectedBiome.IsRequirementMet(_sanctuaryData)) {
                    requirement.SetActive(false);
                    reseachButton.interactable = _selectedBiome.GetPriceToUpgrade() <= _sanctuariesManager.ShellsCnt;
                }
                else {
                    SetRequirement();
                    requirement.SetActive(true);
                    reseachButton.interactable = false;
                }

                //reseachButton.interactable = true;
                //Debug.LogWarning("Research always free");

                //progress bar and boost button hide
                researchProgressBar.gameObject.SetActive(false);
                boostResearchButton.gameObject.SetActive(false);

                if (_selectedBiome.Level < _selectedBiome.Info.MaxLevel)
                    reseachButton.gameObject.SetActive(true);

                return;
            }

            float maxTimeToUpgradeSec = _selectedBiome.Info.UpgradeTimeByLevel_Hours[_selectedBiome.Level] * GameStaticInfo.SECONDS_IN_HOUR;
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