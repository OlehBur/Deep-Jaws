using UnityEngine;


public interface ISanctuaryCameraChanger {
    void OnChangeBiomeCamera(BiomeType biomeType);
    void EnableAlmanacCamera();
    void DisableAlmanacCamera();
}

public class SanctuaryCameraChangerService : MonoBehaviour, ISanctuaryCameraChanger {
    [SerializeField] Camera almanac;
    [SerializeField] Camera coralReef;
    [SerializeField] Camera cliffsSeaweed;
    [SerializeField] Camera pelagial;
    [SerializeField] Camera oceanicAbyss;

    private Camera currentCamera;
    private Camera previosCamera;

    private void Awake() {
        almanac.enabled = false;
        cliffsSeaweed.enabled = false;
        pelagial.enabled = false;
        oceanicAbyss.enabled = false;

        currentCamera = coralReef;
        currentCamera.enabled = true;
    }

    public void OnChangeBiomeCamera(BiomeType biomeType) {
        currentCamera.enabled = false;
        //previosCamera = currentCamera;

        currentCamera = biomeType switch {
            BiomeType.CoralReef => coralReef,
            BiomeType.CliffsSeaweed => cliffsSeaweed,
            BiomeType.Pelagial => pelagial,
            BiomeType.OceanicAbyss => oceanicAbyss,
            _ => coralReef
        };
        currentCamera.enabled = true;
    }

    public void EnableAlmanacCamera() {
        previosCamera = currentCamera;
        currentCamera.enabled = false;
        currentCamera = almanac;
        currentCamera.enabled = true;
    }

    public void DisableAlmanacCamera() {
        currentCamera.enabled = false;
        currentCamera = previosCamera;
        currentCamera.enabled = true;
    }
}
