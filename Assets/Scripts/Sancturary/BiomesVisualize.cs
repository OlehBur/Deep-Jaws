using System.Collections;
using UnityEngine;
using VContainer;

public class BiomesVisualize : MonoBehaviour {
    [SerializeField] Transform coralReefModels;
    [SerializeField] Transform cliffsSeaweedModels;
    [SerializeField] Transform pelagialModels;
    [SerializeField] Transform oceanicAbyssModels;
    [SerializeField] GameObject coral_researchParticle;
    [SerializeField] GameObject cliffs_researchParticle;
    [SerializeField] GameObject pelagial_researchParticle;
    [SerializeField] GameObject abyss_researchParticle;

    [Inject] private readonly ISanctuariesManager _sanctuariesManager;
    [Inject] private readonly IAudioPlayService _audioPlayService;

    private WaitForSeconds _waitParticleBurst = new WaitForSeconds(1f);
    private bool _isBiomesInited = false;

    private const float SCALING_MODELS_SPEED = 2f;


    private void Start() {
        //if (_sanctuariesManager != null)
        InitModelsByLevels();
    }

    void InitModelsByLevels() {
        foreach (Transform t in coralReefModels)
            t.gameObject.SetActive(false);
        foreach (Transform t in cliffsSeaweedModels)
            t.gameObject.SetActive(false);
        foreach (Transform t in pelagialModels)
            t.gameObject.SetActive(false);
        foreach (Transform t in oceanicAbyssModels)
            t.gameObject.SetActive(false);

        UpdateBiomeModel(BiomeType.CoralReef, _sanctuariesManager.CoralReef.Level);
        UpdateBiomeModel(BiomeType.CliffsSeaweed, _sanctuariesManager.CliffsSeaweed.Level);
        UpdateBiomeModel(BiomeType.Pelagial, _sanctuariesManager.Pelagial.Level);
        UpdateBiomeModel(BiomeType.OceanicAbyss, _sanctuariesManager.OceanicAbyss.Level);

        _isBiomesInited = true;
    }

    private void OnEnable() {
        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnBiomeResearched += UpdateBiomeModel;

        }
    }

    private void OnDisable() {
        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnBiomeResearched -= UpdateBiomeModel;

        }
    }

    void UpdateBiomeModel(BiomeType type, int Level) {
        Transform lvlModelsParent = type switch {
            BiomeType.CoralReef => coralReefModels,
            BiomeType.CliffsSeaweed => cliffsSeaweedModels,
            BiomeType.Pelagial => pelagialModels,
            BiomeType.OceanicAbyss => oceanicAbyssModels,
            _ => coralReefModels
        };

        if (_isBiomesInited) {//research call
            GameObject researchParticle = type switch {
                BiomeType.CoralReef => coral_researchParticle,
                BiomeType.CliffsSeaweed => cliffs_researchParticle,
                BiomeType.Pelagial => pelagial_researchParticle,
                BiomeType.OceanicAbyss => abyss_researchParticle,
                _ => coral_researchParticle
            };

            StartCoroutine(ResearchAnim(lvlModelsParent.GetChild(Level - 1), researchParticle));
            _audioPlayService.PlayVFX(VFX_clip.SanctuaryResearch);
        }
        else {//init biome
            for (int i = 0; i < Level; i++)
                lvlModelsParent.GetChild(i).gameObject.SetActive(true);
        }
    }

    IEnumerator ResearchAnim(Transform modelsTrans, GameObject buildParticle) {
        modelsTrans.localScale = Vector3.zero;
        buildParticle.SetActive(false);
        buildParticle.SetActive(true);

        yield return _waitParticleBurst;

        modelsTrans.gameObject.SetActive(true);
        while (modelsTrans.localScale.x < 1f) {
            modelsTrans.localScale += Vector3.one * DJTime.DeltaTime * SCALING_MODELS_SPEED;

            yield return null;
        }
        modelsTrans.localScale = Vector3.one;
    }

}
