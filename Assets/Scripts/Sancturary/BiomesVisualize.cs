using System.Collections;
using UnityEngine;

public class BiomesVisualize : MonoBehaviour {
    [SerializeField] Transform coralReefModels;
    [SerializeField] Transform cliffsSeaweedModels;
    [SerializeField] Transform pelagialModels;
    [SerializeField] Transform oceanicAbyssModels;
    [SerializeField] GameObject coral_researchParticle;
    [SerializeField] GameObject cliffs_researchParticle;
    [SerializeField] GameObject pelagial_researchParticle;
    [SerializeField] GameObject abyss_researchParticle;

    private WaitForSeconds WAIT_PARTICLE_BURST = new WaitForSeconds(1f);
    private bool isBiomesInited = false;

    private const float SCALING_MODELS_SPEED = 2f;


    private void Start() {
        if (SanctuariesManager.Instance != null)
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

        UpdateBiomeModel(BiomeType.CoralReef, SanctuariesManager.Instance.coralReef.Level);
        UpdateBiomeModel(BiomeType.CliffsSeaweed, SanctuariesManager.Instance.cliffsSeaweed.Level);
        UpdateBiomeModel(BiomeType.Pelagial, SanctuariesManager.Instance.pelagial.Level);
        UpdateBiomeModel(BiomeType.OceanicAbyss, SanctuariesManager.Instance.oceanicAbyss.Level);

        isBiomesInited = true;
    }

    private void OnEnable() {
        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnBiomeResearched += UpdateBiomeModel;

        }
    }

    private void OnDisable() {
        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnBiomeResearched -= UpdateBiomeModel;

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

        if (isBiomesInited) {//research call
            GameObject researchParticle = type switch {
                BiomeType.CoralReef => coral_researchParticle,
                BiomeType.CliffsSeaweed => cliffs_researchParticle,
                BiomeType.Pelagial => pelagial_researchParticle,
                BiomeType.OceanicAbyss => abyss_researchParticle,
                _ => coral_researchParticle
            };

            StartCoroutine(ResearchAnim(lvlModelsParent.GetChild(Level - 1), researchParticle));
        }
        else {//init biome
            for (int i = 0; i < Level; i++)
                lvlModelsParent.GetChild(i).gameObject.SetActive(true);
        }
    }

    IEnumerator ResearchAnim(Transform modelsTrans, GameObject buildParticle) {
        modelsTrans.localScale = Vector3.zero;
        buildParticle.SetActive(true);

        yield return WAIT_PARTICLE_BURST;

        modelsTrans.gameObject.SetActive(true);
        while (modelsTrans.localScale.x < 1f) {
            modelsTrans.localScale += Vector3.one * Time.deltaTime * SCALING_MODELS_SPEED;

            yield return null;
        }
        modelsTrans.localScale = Vector3.one;
    }

}
