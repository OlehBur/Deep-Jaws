using UnityEngine;


/// <summary>
/// перевіряє наявність ворогів у зоні дії, котрі становлять небезпеку для гравця і переміщує свій погляд (з вказівником) на найближчого ворога 
/// checks for enemies in the area that pose a threat to the player and moves its gaze (with a pointer) to the nearest enemy
/// </summary>
public class EnemyDetector : MonoBehaviour {
    [SerializeField] GameObject pointer;
    private const float DETECTOR_RADIUS_MULTIPLIER = 1.8f; // multiplier for the detector radius to give the player a little advantage
    private float DETECTOR_RADIUS => (playerFish != null) ? 18f : playerFish.ResponseRadius * DETECTOR_RADIUS_MULTIPLIER;//a value larger than the maximum response radius in the some fishes (12), to give the player a little advantage)))
    private Collider[] colliders = new Collider[5];
    private Fish playerFish;

    public void Init(Fish playerFish) {
        this.playerFish = playerFish;
    }

    void Update() {
        if (playerFish != null) {
            int cnt = Physics.OverlapSphereNonAlloc(playerFish.transform.position, DETECTOR_RADIUS, colliders, GameStaticInfo.LAYER_MASK_ENTITY);
            if (cnt > 0) {
                float minDistance = DETECTOR_RADIUS + 1;
                Vector3 closestEnemyPos = Vector3.zero;
                Fish enemyFish = null;

                for (int i = 0; i < cnt; i++) {
                    if (colliders[i] == null)
                        continue;

                    GameStaticInfo.TryGetFish(colliders[i].transform.parent.gameObject, out enemyFish);
                    if (enemyFish != null && enemyFish != playerFish && enemyFish.IsLegitimateTarget(playerFish)) {

                        float distance = Vector3.Distance(playerFish.transform.position, enemyFish.transform.position);
                        if (distance < minDistance) {
                            minDistance = distance;
                            closestEnemyPos = enemyFish.transform.position;
                        }
                    }
                }

                if (minDistance != DETECTOR_RADIUS + 1) {
                    Vector3 lookPosInHorizontal = new Vector3(closestEnemyPos.x, /*playerFish.*/transform.position.y, closestEnemyPos.z);

                    transform.LookAt(lookPosInHorizontal, Vector3.up);
                    pointer.SetActive(true);
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

    //bool IsPlayerWeaker(Fish enemyFish) {
    //    return enemyFish.Level > playerFish.Level ||
    //           (enemyFish.Level == playerFish.Level && enemyFish.Progress > playerFish.Progress// enemy fish at the same level with higher progress attacks only if it is a shark
    //           && enemyFish.Level >= GameStaticInfo.SHARK_INDEX_BARRIER);
    //}
}
