using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]
public class ParticleTimescaleController : MonoBehaviour {
    [SerializeField] ParticleSystem particleSys;
    private ParticleSystem.MainModule mainModule;

    private void Awake() {
        mainModule = particleSys.main;
    }

    private void OnEnable() {
        DJTime.OnTimeScaleChanged += ApplyCustomTimeScale;
        ApplyCustomTimeScale(DJTime.TimeScale);
    }

    private void OnDisable() {
        DJTime.OnTimeScaleChanged -= ApplyCustomTimeScale;
    }

    private void ApplyCustomTimeScale(float newTimeScale) =>
        mainModule.simulationSpeed = newTimeScale;
}