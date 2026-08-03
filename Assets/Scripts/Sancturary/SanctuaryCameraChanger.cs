using UnityEngine;

public class SanctuaryCameraChanger : MonoBehaviour {
    [SerializeField] Camera almanac;
    [SerializeField] Camera coralReef;
    [SerializeField] Camera cliffsSeaweed;
    [SerializeField] Camera pelagial;
    [SerializeField] Camera oceanicAbyss;

    private Camera currentCamera;
    private Camera previosCamera;

    private void Awake() {
        currentCamera = coralReef;
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
