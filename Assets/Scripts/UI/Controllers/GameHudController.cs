using UnityEngine;
using UnityEngine.UI;
using VContainer;


public interface IGameHudController {
    void ActivatePauseWnd(bool isActivate);
    void OnOpenDemoFin();
    void ResetAndReloadGame_Hard();
    void DeactivateRespawnWnd();
}

public class GameHudController : MonoBehaviour, IGameHudController {
    [SerializeField] GameObject demoFinMsg;
    [SerializeField] GameResultWnd gameResult;
    [SerializeField] GameManager gameManager;
    //[SerializeField] GameObject pauseButton;
    [SerializeField] GameObject pauseWnd;
    //[SerializeField] SpawnManager spawnManager;
    [SerializeField] RespawnWnd respawnWnd;
    [SerializeField] Button pauseButton;

    [Inject] private readonly IAudioPlayService _audioPlayService;
    [Inject] private readonly IPlayerFishPoolService _playerSpawnService;
    [Inject] private readonly ISceneLoader _sceneLoader;
    [Inject] private readonly IGameManager _gameManager;
    [Inject] private readonly IAdsManager _adsManager;


    private void Start() {
        PauseButtonInit();
    }

    private void OnEnable() {
        if (_playerSpawnService != null) {
            _playerSpawnService.OnFishDie += ActivateRespawnWnd;
            _playerSpawnService.OnPlayerLVLup += PlayerLvlUpHandler;
            _playerSpawnService.OnPlayerRespawn += PlayerRespawnHandler;
        }
    }

    private void OnDisable() {
        if (_playerSpawnService != null) {
            _playerSpawnService.OnFishDie -= ActivateRespawnWnd;
            _playerSpawnService.OnPlayerLVLup -= PlayerLvlUpHandler;
            _playerSpawnService.OnPlayerRespawn -= PlayerRespawnHandler;
        }
    }

    void PauseButtonInit() {
        pauseButton.gameObject.SetActive(!GameManager.IsRunOnPC);
        pauseButton.onClick.RemoveAllListeners();
        pauseButton.onClick.AddListener(() => {
            _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick);
            ActivatePauseWnd(true);
        });
    }

    void PlayerRespawnHandler(Fish f, bool isDie) =>
        DeactivateRespawnWnd();

    public void OnOpenDemoFin() {
        _audioPlayService.PlayVFX(VFX_clip.BubblePop);
        _audioPlayService.StopPlayerAudioSource();
        demoFinMsg.SetActive(true);
    }

    void PlayerLvlUpHandler(FishLevel level) {
        if (GameManager.IS_PC_DEMO && level >= FishLevel.Mackerel) {
            gameManager.GameSetPause(true);
            OnOpenDemoFin();
        }

        (bool prem, bool noadd) = ProgressSaver.IsPlayerHasAdImmunitet();
        if (!prem && !noadd) {//if doesnt have immunitet
            if (/*gameAdsManager*/_adsManager.OnShowInterstitialAd()) //only if ad will be shown
                ActivatePauseWnd(true);
        }
    }

    public void ActivatePauseWnd(bool isActivate) {
        if (gameManager != null)
            gameManager.GameSetPause(isActivate);

        pauseWnd.SetActive(isActivate);
    }


    void ActivateRespawnWnd(Fish f) {
#if UNITY_ANDROID
        if (GameStaticInfo.Gamemode == GameMode.Hard) {//witout ui
            ResetAndReloadGame_Hard();
            //playerSpawnService.RespawnPlayer(true, false);
            return;
        }
        //GameAdsManager.Instance.UpdateDailyRevivesTxt();
        respawnWnd.OnOpen();
#else
        /*spawnManager*/
        _playerSpawnService.RespawnPlayer(true, false);//just respawn without ui on pc
#endif
    }

    public void OnSureSaveExitGame() {
        _playerSpawnService.SavePlayerData();
#if UNITY_ANDROID
        gameResult.OnOpen();
#else
        _sceneLoader.OnLoad_MainMenuScene();
#endif
    }


    public void ResetAndReloadGame_Hard() {
        _gameManager.ResetWorldData();
        GameModeController.CurrentMode = GameMode.Hard;
        gameResult.AddShellsWithoutWnd();
        _sceneLoader.OnLoad_GameScene();
    }

    public void DeactivateRespawnWnd() => respawnWnd.OnClose();
}
