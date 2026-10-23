using UnityEngine;

[CreateAssetMenu(fileName = "VFX_PrefabsInfo", menuName = "Scriptable Objects/VFX_PrefabsInfo")]
public class VFX_PrefabsInfo : ScriptableObject {
    [field: SerializeField] public GameObject ShieldBubblePrefab { get; private set; }
    [field: SerializeField] public GameObject SpintLinePrefab { get; private set; }
    [field: SerializeField] public GameObject WhirlLinePrefab { get; private set; }
    [field: SerializeField] public GameObject DeadBubblesParticlPrefab { get; private set; }
    [field: SerializeField] public GameObject BiteParticlePrefab { get; private set; }
    [field: SerializeField] public GameObject SkeletParticlePrefab { get; private set; }
    [field: SerializeField] public GameObject HealParticlePrefab { get; private set; }
    [field: SerializeField] public GameObject InkParticlesPrefab { get; private set; }
    [field: SerializeField] public GameObject SpawnParticles { get; private set; }
    [field: SerializeField] public GameObject SkeletonParticle { get; private set; }
    [field: SerializeField] public GameObject DieParticles { get; private set; }
}
