using System;
using UnityEngine;

public class SanctuariesManager : MonoBehaviour {
    public static SanctuariesManager Instance { get; private set; } = null;
    public event Action<int> OnShellsAmountChanged;
    /// <summary>
    /// int - time to upgrade
    /// </summary>
    public event Action<BiomeType, int> OnResearchUpdated;
    /// <summary>
    /// int - lvl
    /// </summary>
    public event Action<BiomeType, int> OnBiomeResearched;
    public int ShellsCnt { get; private set; }

    [SerializeField] BiomeInfo coralReefInfo;
    [SerializeField] BiomeInfo cliffsSeaweedInfo;
    [SerializeField] BiomeInfo pelagialInfo;
    [SerializeField] BiomeInfo oceanicAbyssInfo;

    //private float secondTimer;
    private float minuteTimer;
    public BiomeWrapper coralReef { get; private set; }
    public BiomeWrapper cliffsSeaweed { get; private set; }
    public BiomeWrapper pelagial { get; private set; }
    public BiomeWrapper oceanicAbyss { get; private set; }
    public SanctuaryData SanctuaryData { get; private set; }


    private const float PRODUCTION_UPDATE_INTERVAL = 60f;//every min
    //private const float RESEARCH_INTERVAL_TRIGGER = 1f;//every sec

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetStatics() {
        Instance = null;
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        SanctuaryData = ProgressSaver.IsSanctuaryDataExist ?
            ProgressSaver.LoadSanctuaryData() : new SanctuaryData();

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        //init update
        OnShellsAmountChanged?.Invoke(ShellsCnt);
        OnResearchUpdated?.Invoke(BiomeType.CoralReef, coralReef.TimeToUpgrade);
        //OnAddShells(33333);
        //Debug.LogWarning("Added cheat extra shells");
    }

    private void Update() {
        SanctuariesUpdateBehaviour();
    }

    public void BoostResearch(BiomeType type) {
        BiomeWrapper biome = GetBiomeWrapper(type);
        biome.BoostResearch();
    }

    public BiomeWrapper GetBiomeWrapper(BiomeType type) =>
        type switch {
            BiomeType.CoralReef => coralReef,
            BiomeType.CliffsSeaweed => cliffsSeaweed,
            BiomeType.Pelagial => pelagial,
            BiomeType.OceanicAbyss => oceanicAbyss,
            _ => null
        };

    void SanctuariesUpdateBehaviour() {
        minuteTimer += Time.unscaledDeltaTime;
        //Debug.Log(minuteTimer.ToString());
        if (minuteTimer >= PRODUCTION_UPDATE_INTERVAL) {
            minuteTimer = 0f;
            //Production
            ProduceResourceProgress(coralReef);
            ProduceResourceProgress(cliffsSeaweed);
            ProduceResourceProgress(pelagial);
            ProduceResourceProgress(oceanicAbyss);
            OnShellsAmountChanged?.Invoke(ShellsCnt);

            //upgrading
            ResearchBiomeProgress(coralReef);
            ResearchBiomeProgress(cliffsSeaweed);
            ResearchBiomeProgress(pelagial);
            ResearchBiomeProgress(oceanicAbyss);
        }
    }

    public void OnAddShells(int shellsCnt) {
        ShellsCnt += shellsCnt;
        OnShellsAmountChanged?.Invoke(ShellsCnt);
    }

    void ResearchBiomeProgress(BiomeWrapper biome) {
        if (biome.IsUpgrading) {
            biome.TimeToUpgrade-=GameStaticInfo.SECONDS_IN_MINUTE;

            UpgradeBiomeLevel(biome);
        }
    }

    void UpgradeBiomeLevel(BiomeWrapper biome) {
        if (biome.TimeToUpgrade <= 0) {
            biome.Level++;
            biome.TimeToUpgrade = SanctuaryData.NON_UPGRADE_TIME;

            //research immed prize
            ShellsCnt += biome.GetProductionPerMinute();
            OnShellsAmountChanged?.Invoke(ShellsCnt);

            OnBiomeResearched?.Invoke(biome.Info.Type, biome.Level);
        }

        OnResearchUpdated?.Invoke(biome.Info.Type, biome.TimeToUpgrade);//           if biome is upgrd call for update UI
    }

    private void ProduceResourceProgress(BiomeWrapper biome) {
        if (biome.Level > 0)
            ShellsCnt += biome.ProductionCnt;
    }

    private void OnEnable() {
        //load data
        SanctuaryData sancturariesData = ProgressSaver.IsSanctuaryDataExist ?
            ProgressSaver.LoadSanctuaryData() : new SanctuaryData();
        AccountData accountData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        ShellsCnt = accountData.shellsCnt;

        //init Biomes
        coralReef = new(coralReefInfo, sancturariesData.coralReef_Lvl, sancturariesData.coralReef_SecondsToUpgrade);
        cliffsSeaweed = new(cliffsSeaweedInfo, sancturariesData.cliffsSeaweed_Lvl, sancturariesData.cliffsSeaweed_SecondsToUpgrade);
        pelagial = new(pelagialInfo, sancturariesData.pelagial_Lvl, sancturariesData.pelagial_SecondsToUpgrade);
        oceanicAbyss = new(oceanicAbyssInfo, sancturariesData.oceanicAbyss_Lvl, sancturariesData.oceanicAbyss_SecondsToUpgrade);

        coralReef.OnBoostedProgress += UpgradeBiomeLevel;
        cliffsSeaweed.OnBoostedProgress += UpgradeBiomeLevel;
        pelagial.OnBoostedProgress += UpgradeBiomeLevel;
        oceanicAbyss.OnBoostedProgress += UpgradeBiomeLevel;
    }

    private void OnDisable() {
        if (Instance == this) {
            //save data
            SanctuaryData sancturariesData = ProgressSaver.IsSanctuaryDataExist ?
                ProgressSaver.LoadSanctuaryData() : new SanctuaryData();
            AccountData accountData = ProgressSaver.IsAccountDataExist ?
                ProgressSaver.LoadAccountData() : new AccountData();
            accountData.shellsCnt = ShellsCnt;
            sancturariesData.coralReef_Lvl = coralReef.Level;
            sancturariesData.coralReef_SecondsToUpgrade = coralReef.TimeToUpgrade;

            sancturariesData.cliffsSeaweed_Lvl = cliffsSeaweed.Level;
            sancturariesData.cliffsSeaweed_SecondsToUpgrade = cliffsSeaweed.TimeToUpgrade;

            sancturariesData.pelagial_Lvl = pelagial.Level;
            sancturariesData.pelagial_SecondsToUpgrade = pelagial.TimeToUpgrade;

            sancturariesData.oceanicAbyss_Lvl = oceanicAbyss.Level;
            sancturariesData.oceanicAbyss_SecondsToUpgrade = oceanicAbyss.TimeToUpgrade;
            ProgressSaver.SaveData(sancturariesData);
            ProgressSaver.SaveData(accountData);
            Instance = null;

            coralReef.OnBoostedProgress -= UpgradeBiomeLevel;
            cliffsSeaweed.OnBoostedProgress -= UpgradeBiomeLevel;
            pelagial.OnBoostedProgress -= UpgradeBiomeLevel;
            oceanicAbyss.OnBoostedProgress -= UpgradeBiomeLevel;
        }
    }
}

public class BiomeWrapper {
    public event Action<BiomeWrapper> OnBoostedProgress;
    public BiomeInfo Info { get; private set; }
    private int level = 0;
    public int Level {
        get {
            return level;
        }
        set {
            level = value;
            if (level >= Info.MaxLevel)
                level = Info.MaxLevel;
        }
    }
    public int TimeToUpgrade { get; set; } = SanctuaryData.NON_UPGRADE_TIME;
    public bool IsUpgrading => TimeToUpgrade != SanctuaryData.NON_UPGRADE_TIME;
    public bool NotReserched => Level == 0;
    public int ProductionCnt => Level * (int)Info.Type;
    public int GetProductionPerMinute() => ProductionCnt;
    public int GetPriceToUpgrade() {
        if (level >= Info.MaxLevel)
            return 0;

        return Info.LevelPrices[level];
    }
    public float ResearchTimer { get; set; }

    private const float SECONDS_IN_HOUR = 3600f;


    public BiomeWrapper(BiomeInfo info, int level, int timeToUpgrade) {
        Info = info;
        Level = level;
        TimeToUpgrade = timeToUpgrade;
    }

    public bool IsRequirementMet(SanctuaryData data) {
        if (level >= Info.MaxLevel)
            return false;

        FishLevel currentReqFish = Info.Requirements[level];
        return data[currentReqFish];
    }

    public bool IsEnoughShellsToUpgrade(int currentShellsCnt) {
        if (level >= Info.MaxLevel)
            return false;

        int price = Info.LevelPrices[level];
        return currentShellsCnt >= price;
    }

    public void OnStartUpgrade() {
        if (level >= Info.MaxLevel)
            return;

        TimeToUpgrade = (int)(Info.UpgradeTimeByLevel_Hours[level] * SECONDS_IN_HOUR);
        //TimeToUpgrade = (int)(TimeToUpgrade * .01f);
        //Debug.LogWarning("Activate fast research");
        //Level++;
    }

    public void BoostResearch() {
        if (TimeToUpgrade == SanctuaryData.NON_UPGRADE_TIME)
            return;

        TimeToUpgrade -= GameStaticInfo.BIOME_RESEARCH_BOOST_TIME;
        if (TimeToUpgrade < 0)
            TimeToUpgrade = 0;

        //Debug.LogWarning("Activate one click research");
        //TimeToUpgrade = 0;

        OnBoostedProgress?.Invoke(this);
    }
}
