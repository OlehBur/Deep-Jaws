using UnityEngine;
using VContainer;


/// <summary>
/// перевіряє наявність ворогів у зоні дії, котрі становлять небезпеку для гравця і переміщує свій погляд (з вказівником) на найближчого ворога 
/// checks for enemies in the area that pose a threat to the player and moves its gaze (with a pointer) to the nearest enemy
/// </summary>
public class EnemyDetector : MonoBehaviour {
    [SerializeField] GameTips gameTips;
    [SerializeField] GameObject pointer;

    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;

    private const float DETECTOR_RADIUS_MULTIPLIER = 1.8f; // multiplier for the detector radius to give the player a little advantage
    private float GetDetectorRadius ()=> (_playerFish != null) ?
        18f : _playerFish.DefaultInfo.ResponseRadius * DETECTOR_RADIUS_MULTIPLIER;//a value larger than the maximum response radius in the some fishes (12), to give the player a little advantage)))

    private Collider[] _colliders = new Collider[5];
    private Fish _playerFish;



    public void Init(Fish playerFish) {
        _playerFish = playerFish;
    }

    void Update() {
        if (_playerFish != null && _playerFish.IsActive) {
            float minDistance = GetDetectorRadius();
            int cnt = Physics.OverlapSphereNonAlloc(_playerFish.transform.position, minDistance, _colliders, GameStaticInfo.GetEntityLayerMask());
            
            if (cnt > 0) {
                Vector3 closestEnemyPos = Vector3.zero;
                Fish enemyFish = null;

                for (int i = 0; i < cnt; i++) {
                    if (_colliders[i] == null)
                        continue;

                    _spawnOrchestrator.TryGetFish(_colliders[i].transform.parent.gameObject, out enemyFish);
                    if (enemyFish == null || enemyFish == _playerFish)
                        continue;

                    bool isLegimate = enemyFish.IsLegitimateTarget(_playerFish);
                    //Debug.Log("Danger: " + isLegimate + ", cnt targets: " + cnt);

                    if (isLegimate) {
                        float distance = Vector3.Distance(_playerFish./*Rb*/transform.position, enemyFish.transform.position);
                        if (distance < minDistance) {
                            minDistance = distance;
                            closestEnemyPos = enemyFish.transform.position;
                        }
                    }
                }

                if (minDistance != GetDetectorRadius()) {
                    Vector3 lookPosInHorizontal = new Vector3(closestEnemyPos.x, /*playerFish.*/transform.position.y, closestEnemyPos.z);

                    transform.LookAt(lookPosInHorizontal, Vector3.up);
                    pointer.SetActive(true);
                    gameTips.OnEnemyTip();
                    GameStaticInfo.IS_ENEMY_DETECTOR_ENABLED = true;
                }
                else {
                    pointer.SetActive(false);
                    GameStaticInfo.IS_ENEMY_DETECTOR_ENABLED = false;
                }
            }
            else {
                pointer.SetActive(false);
                GameStaticInfo.IS_ENEMY_DETECTOR_ENABLED = false;
            }
        }
    }
}
