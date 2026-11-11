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

public interface IGameTip {
    void OnDelayedVictimTip();
    void OnEnemyTip();
    void OnEffectTip();
    void OnStageTip();
    void OnClearCoralTip();
    void OnClearSeaweedTip();
    void OnAlgaesBarrierTip();
    void OnDeepCoralsBarrierTip();
    void OnCloseTips();

}

public class GameTipsService : MonoBehaviour, IGameTip {
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
    
    private WorldData _worldData;


    void Awake () {
        _worldData = ProgressSaver.IsWorldDataExist ?
            ProgressSaver.LoadWorldData() : new WorldData();

        if (_worldData.IsAllTipsShowed() || GameModeWnd.CurrentMode != GameMode.Easy)
            gameObject.SetActive(false);
        else
            if (!_worldData.isVictimTipShowed)
            Invoke(nameof(OnDelayedVictimTip), VICTIM_TIP_DELAY);
    }

    public void OnDelayedVictimTip() {
        _worldData.isVictimTipShowed = true;
        ProgressSaver.SaveData(_worldData);

        GameManager.ChangeGameTime(TIP_TIME_SCALE);
        victimTip.SetActive(true);
    }

    public void OnEnemyTip() => ActivateStage(TipType.Enemy, _worldData.isEnemyTipShowed);
    public void OnEffectTip() => ActivateStage(TipType.Effects, _worldData.isEffectTipShowed);
    public void OnStageTip() => ActivateStage(TipType.StageUp, _worldData.isStageTipShowed);
    public void OnClearCoralTip() => ActivateStage(TipType.ClearCorals, _worldData.isClearCoralsTipShowed, false);
    public void OnClearSeaweedTip() => ActivateStage(TipType.ClearSeaweed, _worldData.isClearSeaweedShowed, false);
    public void OnAlgaesBarrierTip() => ActivateStage(TipType.AlgaesBarrier, _worldData.isAlgaesBarrierTipShowed, false);
    public void OnDeepCoralsBarrierTip() => ActivateStage(TipType.DeepCoralBarrier, _worldData.isDeepCoralBarrierTipShowed, false);

    void ActivateStage(TipType type, /*bool isSaveDepend = true,*/ bool isShowed, bool isGamemodeDepend = true) {
        bool condition = /*!isSaveDepend || */(gameObject.activeInHierarchy && !isShowed);
        condition = condition && (!isGamemodeDepend || GameModeWnd.CurrentMode == GameMode.Easy);

        if (condition /*worldData.isStageTipShowed*/) {
            _worldData = ProgressSaver.IsWorldDataExist ?//load other prop in case changing by others scripts
                ProgressSaver.LoadWorldData() : new WorldData();

            //worldData set
            switch (type) {
                case TipType.Victim:
                _worldData.isVictimTipShowed = true;
                break;
                case TipType.Enemy:
                _worldData.isEnemyTipShowed = true;
                break;
                case TipType.Effects:
                _worldData.isEffectTipShowed = true;
                break;
                case TipType.StageUp:
                _worldData.isStageTipShowed = true;
                break;
                case TipType.ClearCorals:
                _worldData.isClearCoralsTipShowed = true;
                break;
                case TipType.ClearSeaweed:
                _worldData.isClearSeaweedShowed = true;
                break;
                case TipType.AlgaesBarrier:
                _worldData.isAlgaesBarrierTipShowed = true;
                break;
                case TipType.DeepCoralBarrier:
                _worldData.isDeepCoralBarrierTipShowed = true;
                break;
            }

            ProgressSaver.SaveData(_worldData);

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
