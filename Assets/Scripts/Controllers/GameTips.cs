using UnityEngine;


public enum TipType {
    Victim,
    Enemy,
    Effects,
    StageUp,
    ClearCorals,
    ClearSeaweed,
    AlgaesBarrier,
    DeepCoralBarrier
}

public class GameTips : MonoBehaviour {
    [SerializeField] GameObject victimTip;
    [SerializeField] GameObject enemyTip;
    [SerializeField] GameObject effectsTip;
    [SerializeField] GameObject stageTip;
    [SerializeField] GameObject clearCoralsTip;
    [SerializeField] GameObject clearSeaweedTip;
    [SerializeField] GameObject algaesBarrierTip;
    [SerializeField] GameObject deeepCoralBarrierTip;

    private const float TIP_TIME_SCALE = .2f;
    private const float VICTIM_TIP_DELAY = 1f;


    private WorldData worldData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        worldData = ProgressSaver.IsWorldDataExist ?
            ProgressSaver.LoadWorldData() : new WorldData();

        if (worldData.IsAllTipsShowed() || GameStaticInfo.Gamemode != GameMode.Easy)
            gameObject.SetActive(false);
        else
            if (!worldData.isVictimTipShowed)
            Invoke(nameof(OnDelayedVictimTip), VICTIM_TIP_DELAY);
    }

    public void OnDelayedVictimTip() {
        worldData.isVictimTipShowed = true;
        ProgressSaver.SaveData(worldData);

        GameManager.ChangeGameTime(TIP_TIME_SCALE);
        victimTip.SetActive(true);
    }

    public void OnEnemyTip() => ActivateStage(TipType.Enemy, worldData.isEnemyTipShowed);
    public void OnEffectTip() => ActivateStage(TipType.Effects, worldData.isEffectTipShowed);
    public void OnStageTip() => ActivateStage(TipType.StageUp, worldData.isStageTipShowed);
    public void OnClearCoralTip() => ActivateStage(TipType.ClearCorals, worldData.isClearCoralsTipShowed, false);
    public void OnClearSeaweedTip() => ActivateStage(TipType.ClearSeaweed, worldData.isClearSeaweedShowed, false);
    public void OnAlgaesBarrierTip() => ActivateStage(TipType.AlgaesBarrier, worldData.isAlgaesBarrierTipShowed, false);
    public void OnDeepCoralsBarrierTip() => ActivateStage(TipType.DeepCoralBarrier, worldData.isDeepCoralBarrierTipShowed, false);

    void ActivateStage(TipType type, /*bool isSaveDepend = true,*/ bool isShowed, bool isGamemodeDepend = true) {
        bool condition = /*!isSaveDepend || */(gameObject.activeInHierarchy && !isShowed);
        condition = condition && (!isGamemodeDepend || GameStaticInfo.Gamemode == GameMode.Easy);

        if (condition /*worldData.isStageTipShowed*/) {
            worldData = ProgressSaver.IsWorldDataExist ?//load other prop in case changing by others scripts
                ProgressSaver.LoadWorldData() : new WorldData();

            //worldData set
            switch (type) {
                case TipType.Victim:
                worldData.isVictimTipShowed = true;
                break;
                case TipType.Enemy:
                worldData.isEnemyTipShowed = true;
                break;
                case TipType.Effects:
                worldData.isEffectTipShowed = true;
                break;
                case TipType.StageUp:
                worldData.isStageTipShowed = true;
                break;
                case TipType.ClearCorals:
                worldData.isClearCoralsTipShowed = true;
                break;
                case TipType.ClearSeaweed:
                worldData.isClearSeaweedShowed = true;
                break;
                case TipType.AlgaesBarrier:
                worldData.isAlgaesBarrierTipShowed = true;
                break;
                case TipType.DeepCoralBarrier:
                worldData.isDeepCoralBarrierTipShowed = true;
                break;
            }

            ProgressSaver.SaveData(worldData);

            GameManager.ChangeGameTime(TIP_TIME_SCALE);
            GetTip(type).SetActive(true);
        }
    }


    GameObject GetTip(TipType type) =>
        type switch {
            TipType.Victim => victimTip,
            TipType.Enemy => enemyTip,
            TipType.Effects => effectsTip,
            TipType.StageUp => stageTip,
            TipType.ClearCorals => clearCoralsTip,
            TipType.ClearSeaweed => clearSeaweedTip,
            TipType.AlgaesBarrier => algaesBarrierTip,
            TipType.DeepCoralBarrier => deeepCoralBarrierTip,
            _ => null
        };

    public void OnCloseTips() {
        victimTip.SetActive(false);
        enemyTip.SetActive(false);
        effectsTip.SetActive(false);
        stageTip.SetActive(false);
        clearCoralsTip.SetActive(false);
        clearSeaweedTip.SetActive(false);
        algaesBarrierTip.SetActive(false);
        deeepCoralBarrierTip.SetActive(false);

        GameManager.ChangeGameTime(GameManager.DEFAULT_GAME_TIME);
    }
}
