using UnityEngine;

public class GameTips : MonoBehaviour {
    [SerializeField] GameObject victimTip;
    [SerializeField] GameObject enemyTip;
    [SerializeField] GameObject effectsTip;
    [SerializeField] GameObject stageTip;

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

    public void OnEnemyTip() {
        if (gameObject.activeInHierarchy && GameStaticInfo.Gamemode == GameMode.Easy && !worldData.isEnemyTipShowed) {
            worldData = ProgressSaver.IsWorldDataExist ?//load other prop in case changing by others scripts
                ProgressSaver.LoadWorldData() : new WorldData();

            worldData.isEnemyTipShowed = true;
            ProgressSaver.SaveData(worldData);

            GameManager.ChangeGameTime(TIP_TIME_SCALE);
            enemyTip.SetActive(true);
        }
    }

    public void OnEffectTip() {
        if (gameObject.activeInHierarchy && GameStaticInfo.Gamemode == GameMode.Easy && !worldData.isEffectTipShowed) {
            worldData = ProgressSaver.IsWorldDataExist ?//load other prop in case changing by others scripts
                ProgressSaver.LoadWorldData() : new WorldData();

            worldData.isEffectTipShowed = true;
            ProgressSaver.SaveData(worldData);

            GameManager.ChangeGameTime(TIP_TIME_SCALE);
            effectsTip.SetActive(true);
        }
    }

    public void OnStageTip() {
        if (gameObject.activeInHierarchy && GameStaticInfo.Gamemode == GameMode.Easy && !worldData.isStageTipShowed) {
            worldData = ProgressSaver.IsWorldDataExist ?//load other prop in case changing by others scripts
                ProgressSaver.LoadWorldData() : new WorldData();

            worldData.isStageTipShowed = true;
            ProgressSaver.SaveData(worldData);

            GameManager.ChangeGameTime(TIP_TIME_SCALE);
            stageTip.SetActive(true);
        }
    }

    public void OnCloseTips() {
        victimTip.SetActive(false);
        enemyTip.SetActive(false);
        effectsTip.SetActive(false);
        stageTip.SetActive(false);

        GameManager.ChangeGameTime(GameManager.DEFAULT_GAME_TIME);
    }
}
