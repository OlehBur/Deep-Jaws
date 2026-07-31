
using System;
using UnityEngine;


public enum FishSpeedModifierType {
    Current,
    Sprint,
    StopTime,
    Poison,
}

/// <summary>
/// Class for the player and enemy fish (base component for fish prefab)
/// </summary>
public class Fish : MonoBehaviour {
    [SerializeField]
    FishInfo fishInfo;
    //[SerializeField] FishLevel level;
    /*[SerializeField]*/
    private int health;
    //[SerializeField] float speed;
    //[SerializeField] int responseRadius;
    [SerializeField] Transform prefabVisualTransform; //for grow animation
    //[SerializeField] float jawForwardOffset;
    [SerializeField] Rigidbody rb;

    public FishInfo DefaultInfo => fishInfo;
    public Vector3 VisualTransformScale => prefabVisualTransform.localScale;
    public Rigidbody Rb => rb;
    public FishLevel Level => /*level*/fishInfo.Level;
    public int Health { get { return health; } protected set { health = value; } }
    //public int MaxHealth { get; private set; }
    public float Speed { get { return /*speed*/fishInfo.Speed * SpeedMultiplier * SpeedMultiplierSecond * SpeedMultiplierThird * SpeedMultiplierFourth; }/* protected set { speed = value; }*/ }
    //public int ResponseRadius => responseRadius;
    public float RotationSpeed => /*speed*/fishInfo.Speed * ROTATION_SPEED_MULTIPLIER;
    /// <summary>
    /// Random value in enemies during sparing
    /// </summary>
    public float Progress { get; protected set; }
    /// <summary>
    /// чи активний об'Їкт риби (залежить в≥д в≥дстанн≥ до гравц€), дл€ в≥дпов≥дного обчисленн€ абож оч≥куванн€ в скриптах
    /// whether the fish object is active (depending on the distance to the player), for the corresponding calculation or wait in the scripts
    /// </summary>
    public bool IsActive { get; set; } = false;
    public bool IsVisible { get; set; } = false;
    public bool IsPlayer { get; set; } = false;
    /// <summary>
    /// for game pool on delegates (async method)
    /// </summary>
    public bool IsAlreadySelected { get; set; } = false;

    /*[HideInInspector]*/ public Vector3 ScaleModifier => new(1f, 1f, .66f + Mathf.Clamp01(Progress) * .34f); // scale modifier for fish, in start - fish looks like baby
    //public float JawForwardOffset => jawForwardOffset;
    public float DeffaultAttackTime => Mathf.Min(MAX_ATTACK_INTERVAL, (1f / (float)Level) * ATTACK_INTERVAL_COEFFICIENT); // default attack time for fish
    public event Action<Vector3> OnFishUpdateSize;
    public event Action<GameObject> OnFishLvlUp;
    public event Action<GameObject, FishLevel, bool> OnFishDie;
    /// <summary>
    /// for save new score and aute play bite animation
    /// </summary>
    public event Action<int> OnFishEat;
    public event Action OnFishDamage;
    public event Action OnInfoStatsChanged;

    private float SpeedMultiplier { get; set; } = 1;
    private float SpeedMultiplierSecond { get; set; } = 1;
    private float SpeedMultiplierThird { get; set; } = 1;
    private float SpeedMultiplierFourth { get; set; } = 1;

    private const float ROTATION_SPEED_MULTIPLIER = 40f; // multiplier for rotation speed
    private const float LEVEL_EAT_DEPEND_MULTIPLIER = .45f;
    private const float MIN_PROGRESS_GAIN_MULTIPLIER = 15f;
    public const float GROWTH_MULTIPLIER = 0.4f; // multiplier for fish scale growth
    private const float ATTACK_INTERVAL_COEFFICIENT = 3f;
    private const float MAX_ATTACK_INTERVAL = 1.1f; // maximum attack interval while fish stay collision with the victim
    private const float KING_ABYSS_PROGRESS = 1.5f; // progress for king abyss achievement


    private void Awake() {
        //MaxHealth = health; // store the initial health as max health
        health = fishInfo.MaxHealth;
    }

    private void OnEnable() {
        SpeedMultiplier = 1f;
        SpeedMultiplierSecond = 1f;
        SpeedMultiplierThird = 1f;
        SpeedMultiplierFourth = 1f;

        OnInfoStatsChanged?.Invoke();
        UpdateGrowSize();
    }

    //[SerializeField] FishControl fc;
    //[SerializeField][Range(0f, 1f)] float growProgress = 0;
    //private void OnValidate() {
    //    if (fc == null)
    //        return;
    //    prefabVisualTransform.localScale = new Vector3(1f, 1f, .66f + Mathf.Clamp01(growProgress) * .34f) * (1 + Mathf.Clamp01(growProgress) * GROWTH_MULTIPLIER);
    //    fc.OnUpdateCollider(prefabVisualTransform.localScale);
    //}

    public void CopyData(int health, float progress) {
        Health = health;
        Progress = progress;
        OnInfoStatsChanged?.Invoke();
        UpdateGrowSize();//update in case of load data from save
    }

    public void ChangeSpeedMultiplier(float value, FishSpeedModifierType index) {
        switch (index) {
            case FishSpeedModifierType.Sprint:
            SpeedMultiplier = value;
            break;
            case FishSpeedModifierType.Current:
            SpeedMultiplierSecond = value;
            break;
            case FishSpeedModifierType.StopTime:
            SpeedMultiplierThird = value;
            break;
            case FishSpeedModifierType.Poison:
            SpeedMultiplierFourth = value;
            break;
            default:
            break;
        }
    }

    public void SetRandomProgress() {
        Progress = UnityEngine.Random.value;
        UpdateGrowSize(); // update scale after setting progress
    }

    /// <summary>
    /// return true if fish is alive, false if fish is dead
    /// </summary>
    /// <param name="victim"></param>
    public void Eat(Fish victim, out bool isStillAlive) {
        float entityCeiling = (float)GameStaticInfo.MAX_FISH_LEVEL - Mathf.Abs((float)victim.Level);
        float levelSpeedProgressing = (float)/*level*/fishInfo.Level * LEVEL_EAT_DEPEND_MULTIPLIER;
        float progressDivider = MIN_PROGRESS_GAIN_MULTIPLIER * levelSpeedProgressing + entityCeiling;//abs for unbeatable fishes
        float enemyProgressMultiplier = IsPlayer ? 1f : 3f;//progress tripl longer in enemies
        enemyProgressMultiplier = GameStaticInfo.Gamemode != GameMode.Easy ? 1f : enemyProgressMultiplier; // in easy mode - slow progress for enemies
        progressDivider *= enemyProgressMultiplier;

        //if (IsPlayer) {
        //    progressDivider = 4f;
        //    Debug.LogWarning("Activated fast progress!!!");
        //}

        Progress += 1f / progressDivider;
        if (!IsPlayer && Progress > 1f) //player can grow over 1f progress
            Progress = 1f;

        int playerDamageMultiplier = victim.IsPlayer && GameStaticInfo.Gamemode == GameMode.Easy ? 1 : 3; // player gets 1/3 damage
        int damage = (int)/*level*/fishInfo.Level * playerDamageMultiplier;
        OnFishEat?.Invoke(damage);
        //Debug.Log($"{gameObject.name} ({Progress}) eats {victim.name} ({victim.Progress}).");

        UpdateGrowSize();
        OnInfoStatsChanged?.Invoke();

        /*return*/
        isStillAlive = victim.TakeDamage(damage);
    }

    public void FeedCaviar(/*float foodNutrition*/) {
        Progress += 1 / (MIN_PROGRESS_GAIN_MULTIPLIER + (float)fishInfo.Level * MIN_PROGRESS_GAIN_MULTIPLIER);
        OnFishEat?.Invoke(1);
        UpdateGrowSize();
        OnInfoStatsChanged?.Invoke();
    }

    /// <summary>
    /// return true when fish is alive after damage, false when fish is dead
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public bool TakeDamage(int damage) {
        Health -= damage;
        OnFishDamage?.Invoke();
        OnInfoStatsChanged?.Invoke();

        if (Health <= 0) {
            Die();
            return false; // fish is dead
        }

        return true; // fish is alive
    }

    public void Heal(int amount) {
        Health = Mathf.Min(Health + amount, /*MaxHealth*/fishInfo.MaxHealth);
        OnInfoStatsChanged?.Invoke();
    }

    public void Die() {
        //Debug.Log($"{this.GetType().Name} has died.");
        Health = /*MaxHealth*/fishInfo.MaxHealth;
        gameObject.SetActive(false);
        IsAlreadySelected = false;

        OnFishDie?.Invoke(gameObject, Level, IsPlayer);
    }

    public void UpdateGrowSize() {
        if (IsPlayer && Progress >= 1f && Level != GameStaticInfo.MAX_FISH_LEVEL) //player with max level will not grow
            OnFishLvlUp?.Invoke(gameObject);
        else if (IsPlayer && Level == GameStaticInfo.MAX_FISH_LEVEL && GameStaticInfo.Gamemode == GameMode.Hard && Progress >= KING_ABYSS_PROGRESS)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_king_of_the_abyss);

        prefabVisualTransform.localScale = ScaleModifier * (1 + Mathf.Clamp01(Progress) * GROWTH_MULTIPLIER);
        OnFishUpdateSize?.Invoke(prefabVisualTransform.localScale);
    }

    /// <summary>
    /// whether the other fish is a legitimate target for attack
    /// [[
    /// - Hostile(enemy) fish are legitimate targets if they are weaker in level, or equal but weaker in progress.
    /// - In turn, hostile fish of the same level do not attack the player, except in cases when the fish 
    /// is a shark class. 
    /// - The player can attack all fish that have a lower level or lower progress if they are of the same level]].
    /// - Crab is a peaceful type of fish and does not attack anyone.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsLegitimateTarget(Fish other, bool withChance = false) {
        bool smallerLvl = other.Level < Level;//other - weaker 
        bool sameLevelLegimate = other.Level == Level && other.Progress <= Progress; // same level and <= progress
        bool allowSameLvlPlayerAtack = GameStaticInfo.Gamemode == GameMode.Hard;
        bool isShark = Level >= GameStaticInfo.SHARK_INDEX_BARRIER;

        if (IsPlayer)
            return (smallerLvl || sameLevelLegimate);//other is weaker or equal to player
        else {
            const float SAME_LVL_ATACK_CHANCE = .03f; // chance to attack same level fish

            if (other.Level <= GameStaticInfo.UNBEATABLE_FISH_LVL)//immortal fish that can't get harm (only by player) 
                return false;

            if (withChance && !other.IsPlayer)// chance only for enemies, not for player (enemy always attack player if same level and progress)
                return smallerLvl ||
                    (sameLevelLegimate && (UnityEngine.Random.value <= SAME_LVL_ATACK_CHANCE && //but not player and ran chance, or player or others if enemy fish is shark or hard mode
                            (!other.IsPlayer || isShark || allowSameLvlPlayerAtack)));
            else
                return smallerLvl ||
                    (sameLevelLegimate && (!other.IsPlayer || isShark || allowSameLvlPlayerAtack));
        }
    }
}


public enum EnemyState {
    Walking,
    Escape,
    Attack
}

public interface IFish {
    /// <summary>
    /// Time spent in idle state
    /// </summary>
    //float IdleTime { get; }
    /// <summary>
    /// Time after which fish can stop
    /// </summary>
    //float TimeToIdle { get; }
    EnemyState State { get; }
    //the fish relative to which the reaction occurs
    Fish CurrentTarget { get; set; }

    void FishProximity(Fish otherFish);
    /// <summary>
    /// returns non normalize direction vector for movement
    /// </summary>
    /// <returns></returns>
    Vector3 Walking();
    Vector3 Escape();
    /// <summary>
    /// unlike walking and escape, does not store the target position, since the victim will definitely run away
    /// </summary>
    /// <returns></returns>
    Vector3 Attack();
}
