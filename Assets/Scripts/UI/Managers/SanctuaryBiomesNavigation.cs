using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public interface ISanctuaryBiomesNavigation {
    BiomeType CurrentBiomeType { get; }
    Action<BiomeType> OnBiomeChanged { get; set; }
}

public class SanctuaryBiomesNavigation : MonoBehaviour, ISanctuaryBiomesNavigation {
    [SerializeField] Button nextBiomeButton;
    [SerializeField] Button previousBiomeButton;
    [SerializeField] Button exitSactuaryButton;
    [SerializeField] Button hideUIButton;
    [SerializeField] Button viewUIButton;
    [SerializeField] GameObject biomeUi;
    [SerializeField] GameObject hideUi;

    [Inject] private readonly IAudioPlayService _audioPlayService;
    [Inject] private readonly ISanctuariesManager _sanctuariesManager;
    [Inject] private readonly ISceneLoader _sceneLoader;

    public BiomeType CurrentBiomeType { get; private set; } = BiomeType.CoralReef;
    public Action<BiomeType> OnBiomeChanged { get; set; }


    private void Start() {
        InitButtonsListeners();
        UpdateSelectedBiomeUI(CurrentBiomeType);
    }

    void InitButtonsListeners() {
        nextBiomeButton.onClick.RemoveAllListeners();
        previousBiomeButton.onClick.RemoveAllListeners();
        exitSactuaryButton.onClick.RemoveAllListeners();
        hideUIButton.onClick.RemoveAllListeners();
        viewUIButton.onClick.RemoveAllListeners();

        nextBiomeButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        previousBiomeButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        exitSactuaryButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        hideUIButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        viewUIButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));

        nextBiomeButton.onClick.AddListener(OnNextBiome);
        previousBiomeButton.onClick.AddListener(OnPreviousBiome);
        exitSactuaryButton.onClick.AddListener(OnExitSanctuary);
        hideUIButton.onClick.AddListener(OnHideUI);
        viewUIButton.onClick.AddListener(OnViewUI);
    }

    void OnNextBiome() => UpdateSelectedBiomeUI(CurrentBiomeType + 1);
    void OnPreviousBiome() => UpdateSelectedBiomeUI(CurrentBiomeType - 1);
    void OnExitSanctuary() => _sceneLoader.OnLoad_MainMenuScene();
    void OnHideUI() {
        biomeUi.SetActive(false);
        hideUi.SetActive(true);
    }
    void OnViewUI() {
        biomeUi.SetActive(true);
        hideUi.SetActive(false);
    }

    void UpdateSelectedBiomeUI(BiomeType currentBiomeType) {
        if (_sanctuariesManager != null) {
            if (currentBiomeType < BiomeType.CoralReef)//infinity scroll
                currentBiomeType = BiomeType.OceanicAbyss;
            if (currentBiomeType > BiomeType.OceanicAbyss)
                currentBiomeType = BiomeType.CoralReef;

            CurrentBiomeType = currentBiomeType;

            OnBiomeChanged?.Invoke(currentBiomeType);
        }
    }
}
