using System;
using UnityEngine;

public class SanctuariesManager : MonoBehaviour {
    public static SanctuariesManager Instance { get; private set; } = null;
    public event Action<int> OnWheatAmountChanged;
    public event Action<BiomeWrapper> OnBiomeResearched;
    public int ShellsCnt { get; private set; }

    [SerializeField] BiomeInfo coralReefInfo;
    [SerializeField] BiomeInfo cliffsSeaweedInfo;
    [SerializeField] BiomeInfo pelagialInfo;
    [SerializeField] BiomeInfo oceanicAbyssInfo;

    private float timer;
    private BiomeWrapper coralReef;
    private BiomeWrapper cliffsSeaweed;
    private BiomeWrapper pelagial;
    private BiomeWrapper oceanicAbyss;

    private const float PRODUCTION_INTERVAL = 1f;//every sec
    //private const float RESEARCH_INTERVAL_TRIGGER = 1f;//every sec

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        ProgressSaver.LoadSanctuaryData();

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        SanctuariesBehaviour();
    }

    void SanctuariesBehaviour() {
        timer += Time.unscaledDeltaTime;
        if (timer >= PRODUCTION_INTERVAL) {
            timer = 0f;
            //Production
            ProduceResourceProgress(coralReef);
            ProduceResourceProgress(cliffsSeaweed);
            ProduceResourceProgress(pelagial);
            ProduceResourceProgress(oceanicAbyss);

            OnWheatAmountChanged?.Invoke(ShellsCnt);
            //upgrading
            ResearchBiomeProgress(coralReef);
            ResearchBiomeProgress(cliffsSeaweed);
            ResearchBiomeProgress(pelagial);
            ResearchBiomeProgress(oceanicAbyss);
        }
    }

    public void OnAddShells(int shellsCnt) =>
        ShellsCnt += shellsCnt;

    void ResearchBiomeProgress(BiomeWrapper biome) {
        if (biome.IsBiomeUpgrading) {
            biome.TimeToUpgrade--;

            if (biome.TimeToUpgrade == 0) {
                biome.Level++;
                biome.TimeToUpgrade = SanctuaryData.NON_UPGRADE_TIME;
                OnBiomeResearched?.Invoke(biome);
            }
        }
    }

    private void ProduceResourceProgress(BiomeWrapper biome) {
        if (biome.Level > 0)
            ShellsCnt += biome.ProductionCnt;
    }

    private void OnEnable() {
        //load data
        SanctuaryData sancturariesData = ProgressSaver.IsSanctuaryDataExist ?
            ProgressSaver.LoadSanctuaryData() : new SanctuaryData();

        ShellsCnt = sancturariesData.shellsCnt;

        //init Biomes
        coralReef = new(coralReefInfo, sancturariesData.coralReef_Lvl, sancturariesData.coralReef_SecondsToUpgrade);
        cliffsSeaweed = new(cliffsSeaweedInfo, sancturariesData.cliffsSeaweed_Lvl, sancturariesData.cliffsSeaweed_SecondsToUpgrade);
        pelagial = new(pelagialInfo, sancturariesData.pelagial_Lvl, sancturariesData.pelagial_SecondsToUpgrade);
        oceanicAbyss = new(oceanicAbyssInfo, sancturariesData.oceanicAbyss_Lvl, sancturariesData.oceanicAbyss_SecondsToUpgrade);
    }

    private void OnDisable() {
        //save data
        SanctuaryData sancturariesData = ProgressSaver.IsSanctuaryDataExist ?
            ProgressSaver.LoadSanctuaryData() : new SanctuaryData();
        sancturariesData.shellsCnt = ShellsCnt;
        sancturariesData.coralReef_Lvl = coralReef.Level;
        sancturariesData.coralReef_SecondsToUpgrade = coralReef.TimeToUpgrade;

        sancturariesData.cliffsSeaweed_Lvl = cliffsSeaweed.Level;
        sancturariesData.cliffsSeaweed_SecondsToUpgrade = cliffsSeaweed.TimeToUpgrade;

        sancturariesData.pelagial_Lvl = pelagial.Level;
        sancturariesData.pelagial_SecondsToUpgrade = pelagial.TimeToUpgrade;

        sancturariesData.oceanicAbyss_Lvl = oceanicAbyss.Level;
        sancturariesData.oceanicAbyss_SecondsToUpgrade = oceanicAbyss.TimeToUpgrade;

        ProgressSaver.SaveData(sancturariesData);
    }
}

public class BiomeWrapper {
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
    public bool IsBiomeUpgrading => TimeToUpgrade != SanctuaryData.NON_UPGRADE_TIME;
    public int ProductionCnt => Level * (int)Info.Type;
    public float ResearchTimer { get; set; }

    public BiomeWrapper(BiomeInfo info, int level, int timeToUpgrade) {
        Info = info;
        Level = level;
        TimeToUpgrade = timeToUpgrade;
    }
}
