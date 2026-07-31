using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "FishAlmanac", menuName = "Scriptable Objects/FishAlmanac")]
public class FishInfo : ScriptableObject {
    [Header("Almanac")]
    [field: SerializeField] public FishLevel Level { get; private set; }
    //[SerializeField] public FishLevel level;
    [field: SerializeField] public LocalizedString Tittle { get; private set; }
    //[SerializeField] public LocalizedString tittle;
    [field: SerializeField] public LocalizedString Description { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; }
    //[SerializeField] public int maxHealth;
    [field: SerializeField] public float Speed { get; private set; }
    //[SerializeField] public float speed;
    [field: SerializeField] public float ResponseRadius { get; private set; }
    //[SerializeField] public float responseRadius;
    [field: SerializeField] public float BiteDamage { get; private set; }
    //[SerializeField] public float biteDamage;

    [Header("Collider")]
    [field: SerializeField] public float JawFwdOffset { get; private set; }
    //[SerializeField] public float jawFwdOffset;
    [field: SerializeField] public float ColliderDefaultHeight { get; private set; }
    //[SerializeField] public float colliderDefaultHeight;
    [field: SerializeField] public float ColliderDefaultRadius { get; private set; }
    //[SerializeField] public float colliderDefaultRadius;

    [Header("Animations")]
    [field: SerializeField] public bool IsUseRotShader { get; private set; }
    //[SerializeField] public bool isUseRotShader;
    [field: SerializeField] public float SmoothRotSpeed { get; private set; }
    //[SerializeField] public float smoothRotSpeed;
    [field: SerializeField] public float AngRotDetect { get; private set; }
    //[SerializeField] public float angRotDetect;
    [field: SerializeField] public float MaxShaderFrequencyVal { get; private set; }
    [field: SerializeField] public float MaxShaderRotationVal { get; private set; }
    //[SerializeField] public float maxShaderRotationVal;


    [Header("Effects")]
    [field: SerializeField] public bool IsHaveShield { get; private set; }
    //[SerializeField] public bool isHaveShield;
    [field: SerializeField] public float BubbleXoffset { get; private set; }
    //[SerializeField] public float bubbleXoffset;
    [field: SerializeField] public Vector3 BiteParticleLocPos { get; private set; }
    //[SerializeField] public Vector3 biteParticleLocPos;
    [field: SerializeField] public Vector3 ShieldLocPos { get; private set; }
    //[SerializeField] public Vector3 shieldLocPos;
    [field: SerializeField] public Vector3 ShieldVisualScale { get; private set; }
    //[SerializeField] public Vector3 shieldVisualScale;
    [field: SerializeField] public Vector3 WhirlLocPos { get; private set; }
    //[SerializeField] public Vector3 whirlLocPos;

    //private void Awake() {//quick fix
    //    JawFwdOffset = jawFwdOffset;
    //    Level = level;
    //    Tittle = tittle;
    //    MaxHealth = maxHealth;
    //    Speed = speed;
    //    ResponseRadius = responseRadius;
    //    BiteDamage = biteDamage;
    //    ColliderDefaultHeight = colliderDefaultHeight;
    //    ColliderDefaultRadius = colliderDefaultRadius;
    //    IsUseRotShader = isUseRotShader;
    //    SmoothRotSpeed = smoothRotSpeed;
    //    AngRotDetect = angRotDetect;
    //    MaxShaderRotationVal = maxShaderRotationVal;
    //    IsHaveShield = isHaveShield;
    //    BubbleXoffset = bubbleXoffset;
    //    BiteParticleLocPos = biteParticleLocPos;
    //    ShieldLocPos = shieldLocPos;
    //    ShieldVisualScale = shieldVisualScale;
    //    WhirlLocPos = whirlLocPos;
    //}
}
