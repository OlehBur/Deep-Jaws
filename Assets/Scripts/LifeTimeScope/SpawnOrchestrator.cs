using UnityEngine;
using VContainer;
//using VContainer.Unity;



public class SpawnOrchestrator : MonoBehaviour/*IInitializable*/ {
    [Inject] private readonly IPlayerPoolService _playerSpawnService;
    [Inject] private readonly IEnemyPoolService _enemyPoolService;
    [Inject] private readonly MainmenuManager _mainmenuManager;

    [SerializeField] Transform fishPoolParent;
    [SerializeField] GameObject enemyDetectorPrefab;

    // VContainer construct!
    //[Inject]
    //public SpawnOrchestrator(PlayerSpawnService playerSpawnService, EnemyPoolService enemyPoolService, MainmenuManager mainmenuManager) {
    //    _playerSpawnService = playerSpawnService;
    //    _enemyPoolService = enemyPoolService;
    //    _mainmenuManager = mainmenuManager;
    //}

    public void /*Initialize*/Start() {
        _enemyPoolService.InitPool(fishPoolParent);
        _playerSpawnService.InitPool(fishPoolParent);
    }
}
