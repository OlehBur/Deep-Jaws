//#define UNITY_ANDROID

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


[System.Serializable]
public class BiomeUIContainer {
    public ResidentButtonsView residentButtonsView;
    public TextMeshProUGUI biomeTittle;
    public GameObject biomeUnlockInfo;
    public BiomeLvlBarView biomeLvlBar;
    public TextMeshProUGUI productionPerMinuteTxt;
    public Button reseachButton;
    public TextMeshProUGUI researchButtonTxt;
    public Button boostResearchButton;
    public ProgressBar researchProgressBar;
    public TextMeshProUGUI requirementVictimValue;
    public GameObject requirementContainer;
}


public class SanctuaryVisualizeWnd : MonoBehaviour {
    [SerializeField] GameObject biomesUI;
    [SerializeField] GameObject almanacUI;
    [SerializeField] ShellsBarView shellsBar;
    [SerializeField] BiomeUIContainer biomeUIContainer;

    [Inject] private readonly SanctuaryUIInfo _sanctuaryUIInfo;
    [Inject] private readonly FishLocalizationInfo _fishLocalized;
    [Inject] private readonly ISanctuaryCameraChanger _cameraChanger;
    [Inject] private readonly ISanctuaryBiomesNavigation _sanctuaryBiomesNavigation;
    [Inject] private readonly IFishAlmanacVisualizeUI _fishAlmanac;
    [Inject] private readonly ISanctuariesManager _sanctuariesManager;
    [Inject] private readonly IAdsManager _adsManager;

    /// <summary>
    /// only for check requirements once per load scene
    /// </summary>
    private SanctuaryData _sanctuaryData;
    private BiomeWrapper _selectedBiome;


    private void Start() {
        _sanctuaryBiomesNavigation.OnBiomeChanged += UpdateBiomeUI;
        UpdateBiomeUI(_sanctuaryBiomesNavigation.CurrentBiomeType);

        biomeUIContainer.reseachButton.onClick.AddListener(OnResearchCurrentBiome);
        biomeUIContainer.residentButtonsView.InitListeners(this);
        biomesUI.SetActive(true);
        almanacUI.SetActive(false);

        //if (_sanctuariesManager != null) {
        UpdateShellsCnt(_sanctuariesManager.ShellsCnt);
        //}

#if !UNITY_STANDALONE_WIN
        //if (_adsManager != null)
        biomeUIContainer.boostResearchButton.onClick.AddListener(() => _adsManager.OnBoostClicked(_sanctuaryBiomesNavigation.CurrentBiomeType));
#else 
        biomeUIContainer.boostResearchButton.gameObject.SetActive(false);
#endif
    }

    void UpdateBiomeUI(BiomeType currentBiomeType) {
        //if (_sanctuaryBiomesNavigation.CurrentBiomeType != currentBiomeType)
        _cameraChanger.OnChangeBiomeCamera(currentBiomeType);

        _selectedBiome = _sanctuariesManager.GetBiomeWrapper(currentBiomeType);

        biomeUIContainer.biomeTittle.text = _selectedBiome.Info.Tittle.GetLocalizedString();
        biomeUIContainer.researchButtonTxt.text = _selectedBiome.GetPriceToUpgrade().ToString();
        biomeUIContainer.reseachButton.interactable = _selectedBiome.GetPriceToUpgrade() <= _sanctuariesManager.ShellsCnt;
        if (_selectedBiome.NotReserched) {
            biomeUIContainer.biomeUnlockInfo.SetActive(false);

            UpdateResearchProgress(currentBiomeType, _selectedBiome.TimeToUpgrade);
            biomeUIContainer.residentButtonsView.DisableAllSlots();
        }
        else {
            biomeUIContainer.biomeUnlockInfo.SetActive(true);

            UpdateBiomeNonStatInfo();
            biomeUIContainer.residentButtonsView.UpdateResidentImagesPreview(_selectedBiome);

            if (_selectedBiome.Level >= _selectedBiome.Info.MaxLevel) {
                biomeUIContainer.reseachButton.gameObject.SetActive(false);
                biomeUIContainer.boostResearchButton.gameObject.SetActive(false);
                biomeUIContainer.researchProgressBar.gameObject.SetActive(false);
                biomeUIContainer.requirementContainer.SetActive(false);
            }
        }
    }

    private void OnEnable() {
        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnShellsAmountChanged += UpdateShellsCnt;
            _sanctuariesManager.OnResearchUpdated += UpdateResearchProgress;
            _sanctuariesManager.OnBiomeResearched += UpdateUIafterResearch;
        }
#if UNITY_ANDROID
        //if (GameAdsManager.Instance != null)
        _adsManager.OnAdButtonsInteractable += BoostInteractable;
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
        //if (GameAdsManager.Instance != null)
        _adsManager.OnAdButtonsInteractable -= BoostInteractable;
#endif

        biomeUIContainer.reseachButton.onClick.RemoveAllListeners();
        biomeUIContainer.boostResearchButton.onClick.RemoveAllListeners();
    }

#if !UNITY_STANDALONE_WIN
    void BoostInteractable(bool interactable) =>
        biomeUIContainer.boostResearchButton.interactable = interactable;
#endif

    public void OnOpenAlmanacUI(int index) {
        FishLevel fishLevel = biomeUIContainer.residentButtonsView.GetFishResidentByIndex(index, _selectedBiome);
        _fishAlmanac.InitWnd(fishLevel);
        _cameraChanger.EnableAlmanacCamera();

        biomesUI.SetActive(false);
        almanacUI.SetActive(true);
    }


    public void OnCloseAlmanacUI() {
        _cameraChanger.DisableAlmanacCamera();
        biomesUI.SetActive(true);
        almanacUI.SetActive(false);
    }


    void SetRequirement() {
        if (_selectedBiome.Level >= _selectedBiome.Info.MaxLevel) {
            biomeUIContainer.requirementContainer.SetActive(false);
            return;
        }
        FishLevel requiredFishType = _selectedBiome.Info.Requirements[_selectedBiome.Level];

        biomeUIContainer.requirementVictimValue.text = _sanctuaryUIInfo.ResearchNeutralize.GetLocalizedString() + " "
            + _fishLocalized.GetFishLocalizeByType(requiredFishType);
    }

    void OnResearchCurrentBiome() {
        _sanctuariesManager.OnAddShells(-_selectedBiome.GetPriceToUpgrade());
        _selectedBiome.OnStartUpgrade();

        //_selectedBiome.BoostResearch();
        //if (_selectedBiome.Level == _selectedBiome.Info.MaxLevel)
        //    biomeUIContainer.reseachButton.gameObject.SetActive(false);
        //Debug.LogWarning("ACtivate one click research");

        UpdateResearchProgress(_selectedBiome.Info.Type, _selectedBiome.TimeToUpgrade);
    }

    void UpdateBiomeNonStatInfo() {
        biomeUIContainer.biomeLvlBar.UpdateStarsVisualize(_selectedBiome.Level, _selectedBiome.Info.MaxLevel);
        biomeUIContainer.productionPerMinuteTxt.text = _selectedBiome.GetProductionPerMinute().ToString() + "/"
            + _sanctuaryUIInfo.MinuteShort.GetLocalizedString();
        biomeUIContainer.researchButtonTxt.text = _selectedBiome.GetPriceToUpgrade().ToString();
        biomeUIContainer.reseachButton.interactable = _selectedBiome.GetPriceToUpgrade() <= _sanctuariesManager.ShellsCnt;

        UpdateResearchProgress(_selectedBiome.Info.Type, _selectedBiome.TimeToUpgrade);
    }



    void UpdateUIafterResearch(BiomeType type, int lvl) {
        if (_sanctuaryBiomesNavigation.CurrentBiomeType == type) {
            biomeUIContainer.biomeUnlockInfo.SetActive(true);

            UpdateBiomeNonStatInfo();
            biomeUIContainer.residentButtonsView.UpdateResidentImagesPreview(_selectedBiome);
        }
    }

    void UpdateResearchProgress(BiomeType biomeType, int timeToUpgrade) {
        if (_selectedBiome == null)
            _selectedBiome = _sanctuariesManager.GetBiomeWrapper(_sanctuaryBiomesNavigation.CurrentBiomeType);

        if (biomeType == _sanctuaryBiomesNavigation.CurrentBiomeType) {
            if (_selectedBiome.IsUpgrading) {
                biomeUIContainer.reseachButton.gameObject.SetActive(false);
                biomeUIContainer.researchProgressBar.gameObject.SetActive(true);
#if !UNITY_STANDALONE_WIN
                biomeUIContainer.boostResearchButton.gameObject.SetActive(true);
#endif
                biomeUIContainer.requirementContainer.SetActive(false); //disable requirements
            }
            else {
                //requirement check
                if (_selectedBiome.IsRequirementMet(_sanctuaryData)) {
                    biomeUIContainer.requirementContainer.SetActive(false);
                    biomeUIContainer.reseachButton.interactable = _selectedBiome.GetPriceToUpgrade() <= _sanctuariesManager.ShellsCnt;
                }
                else {
                    SetRequirement();
                    biomeUIContainer.requirementContainer.SetActive(true);
                    biomeUIContainer.reseachButton.interactable = false;
                }

                //biomeUIContainer.reseachButton.interactable = true;
                //Debug.LogWarning("Research always free");

                //progress bar and boost button hide
                biomeUIContainer.researchProgressBar.gameObject.SetActive(false);
                biomeUIContainer.boostResearchButton.gameObject.SetActive(false);

                if (_selectedBiome.Level < _selectedBiome.Info.MaxLevel)
                    biomeUIContainer.reseachButton.gameObject.SetActive(true);

                return;
            }

            float maxTimeToUpgradeSec = _selectedBiome.Info.GetUpgradeTimeByLevel_Hours(_selectedBiome.Level) * GameStaticInfo.SECONDS_IN_HOUR;
            biomeUIContainer.researchProgressBar.SetFillAmount((float)(maxTimeToUpgradeSec - timeToUpgrade) / maxTimeToUpgradeSec);
            biomeUIContainer.researchProgressBar.SetText(_sanctuaryUIInfo.ResearchProg.GetLocalizedString() + " " + GetTimeString(timeToUpgrade));
        }
    }

    string GetTimeString(int seconds) {
        int hours = seconds / GameStaticInfo.SECONDS_IN_HOUR;
        int minutes = (seconds % GameStaticInfo.SECONDS_IN_HOUR) / GameStaticInfo.SECONDS_IN_MINUTE;
        if (hours > 0)
            return hours + _sanctuaryUIInfo.HourShort.GetLocalizedString() + " " + minutes + _sanctuaryUIInfo.MinuteShort.GetLocalizedString() + "...";
        else
            return minutes + _sanctuaryUIInfo.MinuteShort.GetLocalizedString() + "...";
    }

    private void UpdateShellsCnt(int currentWheat) =>
        shellsBar.UpdateShellsCnt(currentWheat);

}