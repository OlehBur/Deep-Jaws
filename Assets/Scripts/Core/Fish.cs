
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
    [SerializeField]/*[Range(0, GameStaticInfo.MAX_FISH_LEVEL)]*/ FishLevel level;
    [SerializeField] int health;
    [SerializeField] float speed;
    [SerializeField] int responseRadius;
    //const int RIQIDBODY_VELOCITY_MULTIPLIER = 80;
    const float GROWTH_MULTIPLIER = 0.4f; // multiplier for fish scale growth
    private float SpeedMultiplier { get; set; } = 1;
    private float SpeedMultiplierSecond { get; set; } = 1;
    private float SpeedMultiplierThird { get; set; } = 1;
    private float SpeedMultiplierFourth { get; set; } = 1;
    public FishLevel Level { get { return level; } /*protected set { level = value; }*/ }
    public int Health { get { return health; } protected set { health = value; } }
    public int MaxHealth { get; private set; }
    public float Speed { get { return speed * /*RIQIDBODY_VELOCITY_MULTIPLIER **/ SpeedMultiplier * SpeedMultiplierSecond * SpeedMultiplierThird * SpeedMultiplierFourth; } protected set { speed = value; } }
    public int ResponseRadius { get { return responseRadius /** RIQIDBODY_VELOCITY_MULTIPLIER*/; }/* protected set { responseRadius = value; }*/ }
    private const float ROTATION_SPEED_MULTIPLIER = .02f; // multiplier for rotation speed
    public float RotationSpeed => /*Speed*/speed * ROTATION_SPEED_MULTIPLIER;
    /// <summary>
    /// Random value in enemies during sparing
    /// </summary>
    public float Progress { get; protected set; }
    private const float LEVEL_EAT_DEPEND_MULTIPLIER = .45f;
    private const float MIN_PROGRESS_GAIN_MULTIPLIER = 11f;
    // <summary>
    // For enemy fish - that crossed their collider for the first time, 
    // for the player - that he crossed his collider and he goes to this victim.
    // </summary>
    //public Fish CurrentTarget { get; protected set; }
    //public bool IsPlayer { get; private set; }

    /// <summary>
    /// чи активний об'єкт риби (залежить від відстанні до гравця), для відповідного обчислення абож очікування в скриптах
    /// whether the fish object is active (depending on the distance to the player), for the corresponding calculation or wait in the scripts
    /// </summary>
    public bool IsActive { get; set; } = false;
    public bool IsVisible { get; set; } = false;
    public bool IsPlayer { get; set; } = false;
    /// <summary>
    /// for game pool on delegates (async method)
    /// </summary>
    public bool IsAlreadySelected { get; set; } = false;

    [SerializeField] Transform prefabVisualTransform; //for grow animation
    //public Vector3 CurrentScale { get; private set; }
    [HideInInspector] public Vector3 ScaleModifier => new Vector3(1f, 1f, .66f + Mathf.Clamp01(Progress) * .34f); // scale modifier for fish, in start - fish looks like baby
    [SerializeField] float jawForwardOffset;
    public float JawForwardOffset => jawForwardOffset;


    private const float ATTACK_INTERVAL_COEFFICIENT = 3f;
    private const float MAX_ATTACK_INTERVAL = 1.1f; // maximum attack interval while fish stay collision with the victim
    private const float KING_ABYSS_PROGRESS = 1.5f; // progress for king abyss achievement
    public float DeffaultAttackTime => Mathf.Min(MAX_ATTACK_INTERVAL, (1f / (float)Level) * ATTACK_INTERVAL_COEFFICIENT); // default attack time for fish

    public event Action<GameObject> OnFishLvlUp;
    public event Action<GameObject, FishLevel> OnFishDie;
    /// <summary>
    /// for save new score and aute play bite animation
    /// </summary>
    public event Action<int> OnFishEat;
    public event Action OnFishDamage;
    public event Action OnInfoStatsChanged;


    private void /*Fish*/Awake(/*FishLevel level, int health, float speed, int responseRadius, bool isPlayer = false*/) {
        MaxHealth = health; // store the initial health as max health
        //this.level = level;
        //this.health = health;
        //this.speed = speed;
        //this.responseRadius = responseRadius;
        //IsPlayer = isPlayer;
        //OnInfoStatsChanged?.Invoke();
    }

    //private void Start() {
    //Health = Health / 4;
    //Progress = .9f;
    //UpdateGrowSize();
    //Debug.LogWarning("max progress");

    //OnInfoStatsChanged?.Invoke();
    //}

    private void OnEnable() {
        SpeedMultiplier = 1f;
        SpeedMultiplierSecond = 1f;
        SpeedMultiplierThird = 1f;
        SpeedMultiplierFourth = 1f;

        OnInfoStatsChanged?.Invoke();
        UpdateGrowSize();
    }

    //public object Clone() {
    //    return MemberwiseClone();
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
    /// only for player fish, sets level and resets progress
    /// </summary>
    /// <param name="lvl"></param>
    //public void SetLvl(int lvl) {
    //    Level = (FishLevel)Math.Min(lvl, (int)GameStaticInfo.MAX_FISH_LEVEL);
    //    Progress = 0f;//only the player resets progress each time he reaches a level, enemies remain at the initial spawn level

    //    OnFishLvlUp?.Invoke(gameObject);
    //    OnInfoStatsChanged?.Invoke();
    //}

    /// <summary>
    /// return true if fish is alive, false if fish is dead
    /// </summary>
    /// <param name="victim"></param>
    public bool Eat(Fish victim) {
        float entityCeiling = (float)GameStaticInfo.MAX_FISH_LEVEL - Mathf.Abs((float)victim.Level);
        float levelSpeedProgressing = (float)level * LEVEL_EAT_DEPEND_MULTIPLIER;
        float progressDivider = MIN_PROGRESS_GAIN_MULTIPLIER * levelSpeedProgressing + entityCeiling;//abs for unbeatable fishes
        float enemyProgressMultiplier = IsPlayer ? 1f : 3f;//progress tripl longer in enemies
        enemyProgressMultiplier = GameStaticInfo.Gamemode != GameMode.Easy ? 1f : enemyProgressMultiplier; // in easy mode - slow progress for enemies
        progressDivider *= enemyProgressMultiplier;

        Progress += 1f / progressDivider;

        if (!IsPlayer && Progress > 1f) //player can grow over 1f progress
            Progress = 1f;

        //if (CompareTag("Player"))
        //    Debug.LogWarning("Activated fast progress!!!");
        //Debug.LogWarning(progressDivider);

        int playerDamageMultiplier = victim.IsPlayer && GameStaticInfo.Gamemode == GameMode.Easy ? 1 : 3; // player gets 1/3 damage
        int damage = (int)level * playerDamageMultiplier;
        OnFishEat?.Invoke(damage);
        //Debug.Log($"{gameObject.name} ({Progress}) eats {victim.name} ({victim.Progress}).");

        UpdateGrowSize();
        OnInfoStatsChanged?.Invoke();

        return victim.TakeDamage(damage);
    }

    public void FeedCaviar(/*float foodNutrition*/) {
        Progress += 1 / (MIN_PROGRESS_GAIN_MULTIPLIER + (float)level * MIN_PROGRESS_GAIN_MULTIPLIER);
        OnFishEat?.Invoke(0);
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
        Health = Mathf.Min(Health + amount, MaxHealth);
        OnInfoStatsChanged?.Invoke();
    }

    public void Die() {
        //Debug.Log($"{this.GetType().Name} has died.");
        Health = MaxHealth;
        Progress = 0f;
        gameObject.SetActive(false);
        IsAlreadySelected = false;

        OnFishDie?.Invoke(gameObject, Level);
    }

    public void UpdateGrowSize() {
        if (IsPlayer && Progress >= 1f && Level != GameStaticInfo.MAX_FISH_LEVEL) {//player with max level will not grow
            //Progress = 0f;
            //level up only if event has subscribers (onl in player (implemented in grow of the player interface method))
            //Level++;
            OnFishLvlUp?.Invoke(gameObject);
        }
        else if (IsPlayer && Level == GameStaticInfo.MAX_FISH_LEVEL && GameStaticInfo.Gamemode == GameMode.Hard && Progress >= KING_ABYSS_PROGRESS)
            GPG_Manager.OnTriggerAchievement(AchievementKey.KingAbyss);

        prefabVisualTransform.localScale
        /*CurrentScale*/ = ScaleModifier * (1 + Mathf.Clamp01(Progress) * GROWTH_MULTIPLIER);
        //if (CompareTag("Player"))
        //    Debug.Log("E: " + Vector3.one * (1 + Mathf.Clamp01(Progress) * GROWTH_MULTIPLIER));
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
        bool allowSmLvlPlyrAtck = GameStaticInfo.Gamemode == GameMode.Hard;
        bool isShark = Level >= GameStaticInfo.SHARK_INDEX_BARRIER;

        if (IsPlayer)
            return (smallerLvl || sameLevelLegimate);//other is weaker or equal to player
        else {
            const float SAME_LVL_ATACK_CHANCE = .03f; // chance to attack same level fish

            if (other.Level <= GameStaticInfo.UNBEATABLE_FISH_LVL)//immortal fish that can't get harm (only by player) 
                return false;

            if (withChance)
                return smallerLvl ||
                    (sameLevelLegimate && (UnityEngine.Random.value <= SAME_LVL_ATACK_CHANCE && //but not player and ran chance, or player or others if enemy fish is shark or hard mode
                            (!other.IsPlayer || isShark || allowSmLvlPlyrAtck)));
            else
                return smallerLvl ||
                    (sameLevelLegimate && (!other.IsPlayer || isShark || allowSmLvlPlyrAtck));
        }
    }
}


//public interface IPlayerFish {
//    void Movement();
//    //void Turn();
//    //void UpdateModel(); //after grow -> spawn manager - get and change model
//    void Grow() { }//base.Grow();//UpdateModel();
//}


public enum EnemyState {
    Walking,
    Escape,
    Attack
}

//public class EnemyFish : Fish {
public interface IFish {
    /// <summary>
    /// Time spent in idle state
    /// </summary>
    float IdleTime { get; }
    /// <summary>
    /// Time after which fish can stop
    /// </summary>
    float TimeToIdle { get; }
    EnemyState State { get; }
    //риба відносно якої відбувається реакція - the fish relative to which the reaction occurs
    Fish/*FishMultiPrefabController*/ CurrentTarget { get; set; }

    void FishProximity(Fish player);
    /// <summary>
    /// returns non normalize direction vector for movement
    /// </summary>
    /// <returns></returns>
    Vector3 Walking();
    Vector3 Escape();
    /// <summary>
    /// навідміну від walking та escape - не зберігає цільової позиції, оскільки жертва точно тікатиме 
    /// unlike walking and escape, does not store the target position, since the victim will definitely run away
    /// </summary>
    /// <returns></returns>
    Vector3 Attack();
}
