using UnityEngine;

public class BiomesVisualize : MonoBehaviour {
    [SerializeField] Transform coralReefModels;
    [SerializeField] Transform cliffsSeaweedModels;
    [SerializeField] Transform pelagialModels;
    [SerializeField] Transform oceanicAbyssModels;


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

        for (int i = 0; i < Level; i++)
            lvlModelsParent.GetChild(i).gameObject.SetActive(true);
    }

}
