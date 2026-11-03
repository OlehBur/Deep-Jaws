using UnityEngine;
using VContainer;
//using VContainer.Unity;


public interface ISpawnOrchestrator {
    bool TryGetFish(GameObject fishObject, out Fish fish);
}

public class SpawnOrchestrator : MonoBehaviour, ISpawnOrchestrator/*IInitializable*/ {
    [Inject] private readonly IVFXPoolService _vfxPoolManager;
    [Inject] private readonly IPlayerFishPoolService _playerSpawnService;
    [Inject] private readonly IEnemyFishPoolService _enemyPoolService;
    [Inject] private readonly IPermamentFishPoolService _permamentFishPoolService;
    //[Inject] private readonly MainmenuManager _mainmenuManager;

    [SerializeField] Transform fishPoolParent;
    [SerializeField] Transform weakFishSpawnPointsParent;
    [SerializeField] Transform strongFishSpawnPointsParent;
    [SerializeField] Transform sharkSpawnPointsParent;
    [SerializeField] Transform pufferSpawnPointsParent;
    [SerializeField] Transform cuttleSpawnPointsParent;
    [SerializeField] GameObject enemyDetectorPrefab;

    // VContainer construct!
    //[Inject]
    //public SpawnOrchestrator(PlayerSpawnService playerSpawnService, EnemyPoolService enemyPoolService, MainmenuManager mainmenuManager) {
    //    _playerSpawnService = playerSpawnService;
    //    _enemyPoolService = enemyPoolService;
    //    _mainmenuManager = mainmenuManager;
    //}

    public void /*Initialize*/Start() {
        //init
        _playerSpawnService.Init(fishPoolParent);
        _permamentFishPoolService.Init(fishPoolParent);
        _enemyPoolService.Init(fishPoolParent);
        //_enemyPoolService.SetPlayerProvider(_playerSpawnService);
        _enemyPoolService.SetSpawnPoints(weakFishSpawnPointsParent, strongFishSpawnPointsParent, sharkSpawnPointsParent);
        _permamentFishPoolService.SetSpawnPoints(weakFishSpawnPointsParent, strongFishSpawnPointsParent, sharkSpawnPointsParent, pufferSpawnPointsParent, cuttleSpawnPointsParent);

        //first spawns
        _playerSpawnService.InitialSpawnPrefabs();
        _permamentFishPoolService.InitialSpawnPrefabs();
        _enemyPoolService.InitialSpawnPrefabs();

        _playerSpawnService.OnPlayerLVLup += CheckPlayerLvlAchievements;//if current player respawn not called from save, check achievements maybe new lvl reached 
        _playerSpawnService.OnPlayerRespawn += SpawnPlayerSpawnParticles;
        _permamentFishPoolService.OnFishDie += EnemyFishSpawnDeadParticles;
        _enemyPoolService.OnFishDie += EnemyFishSpawnDeadParticles;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (GameStaticInfo.Gamemode == GameMode.Hard)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_the_hard_way);
#endif
    }

    void SpawnPlayerSpawnParticles(Fish playerFish, bool isDie) {
        FishVFX bubbleType = isDie ? FishVFX.DeadBubblesParticle : FishVFX.BubblesLvlUp;

        _vfxPoolManager.ActivateObjectFromPool(bubbleType, fishPoolParent,
            playerFish.transform.localPosition, Vector3.one, Quaternion.identity);
    }

    void EnemyFishSpawnDeadParticles(Fish currentFish) {
        if (currentFish.IsVisible) {
            _vfxPoolManager.ActivateObjectFromPool(FishVFX.SkeletonParticle, fishPoolParent,
                currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
            _vfxPoolManager.ActivateObjectFromPool(FishVFX.DeadBubblesParticle,
                fishPoolParent, currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
        }
    }


    public bool TryGetFish(GameObject fishObject, out Fish fish) {
        return _playerSpawnService.TryGetFish(fishObject, out fish) ||//players
            _enemyPoolService.TryGetFish(fishObject, out fish) ||//default enemies
            _permamentFishPoolService.TryGetFish(fishObject, out fish);//cucttlefishes | dartfhis | pufferfish
    }

    //public void OnSceneExit() {
    //    _playerSpawnService.SavePlayerData();
    //}


    void CheckPlayerLvlAchievements(FishLevel playerLvl) {
#if UNITY_ANDROID && !UNITY_EDITOR
        switch (playerLvl) {
            case FishLevel.Mackerel:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_so_thats_how_it_is);
            break;
            //case FishLevel.Moray://not 
            //    break;
            case FishLevel.Lionfish:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_i_am_the_danger);
            break;
            case FishLevel.Barracuda:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_mackerel_again);
            break;
            case FishLevel.Tuna:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_finally_true_speed);
            break;
            case FishLevel.HammerShark:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_got_a_hammer__now_bring_the_nails);
            break;
            case FishLevel.TigerShark:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_grrr_meow);
            break;
            case FishLevel.WhiteShark:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_but_its_gray);
            break;
        }
#endif
    }

    private void /*OnApplicationQuit*/OnDisable() {
        if (GameModeController.CurrentMode != GameMode.Hard)//in case of death & automatic scene reload
            _playerSpawnService.SavePlayerData();

        _playerSpawnService.OnPlayerLVLup -= CheckPlayerLvlAchievements;//if current player respawn not called from save, check achievements maybe new lvl reached 
        _playerSpawnService.OnPlayerRespawn -= SpawnPlayerSpawnParticles;
        _permamentFishPoolService.OnFishDie -= EnemyFishSpawnDeadParticles;
        _enemyPoolService.OnFishDie -= EnemyFishSpawnDeadParticles;

        _playerSpawnService.OnDestroy();
        _permamentFishPoolService.OnDestroy();
        _enemyPoolService.OnDestroy();
    }
}
