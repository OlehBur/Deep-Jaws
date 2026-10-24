using UnityEngine;
using VContainer;
//using VContainer.Unity;



public class SpawnOrchestrator : MonoBehaviour/*IInitializable*/ {
    [Inject] private readonly IPlayerPoolService _playerSpawnService;
    [Inject] private readonly IEnemyPoolService _enemyPoolService;
    [Inject] private readonly MainmenuManager _mainmenuManager;

    [SerializeField] Transform fishPoolParent;
    [SerializeField] Transform weakFishSpawnPointsParent;
    [SerializeField] Transform strongFishSpawnPointsParent;
    [SerializeField] Transform sharkSpawnPointsParent;
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

        _enemyPoolService.Init(fishPoolParent);
        //_enemyPoolService.SetPlayerProvider(_playerSpawnService);
        _enemyPoolService.SetSpawnPoints(weakFishSpawnPointsParent, strongFishSpawnPointsParent, sharkSpawnPointsParent);

        //first spawns
        _playerSpawnService.InitialSpawnPrefabs();
        _enemyPoolService.InitialSpawnPrefabs();
    }

    public void OnSceneExit() {
        _playerSpawnService.SavePlayerData();
    }
}
