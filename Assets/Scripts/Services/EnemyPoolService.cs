using UnityEngine;


public interface IEnemyPoolService {
    void InitPool(Transform fishPoolParent);
    void RespawnEntitiesMap(FishLevel playerFishLVL);
}

public class EnemyPoolService: IEnemyPoolService {
    private GameObject[,] _enemyPool;
    private GameObject[] _pufferfishPool;
    private GameObject[] _cuttlefishPool;
    private GameObject[] _dartfishPool;

    private Fish[,] _enemyPoolComp;
    private Fish[] _pufferfishPoolComp;
    private Fish[] _cuttlefishPoolComp;
    private Fish[] _dartfishPoolComp;

    private static readonly float[] SPAWN_WEIGHTS_EASY = { .5f, .35f, .15f };
    private static readonly float[] SPAWN_WEIGHTS_EASY_START = { .65f, .33f, .02f };
    private static readonly float[] SPAWN_WEIGHTS_MEDIUM = { .4f, .4f, .2f };
    private static readonly float[] SPAWN_WEIGHTS_HARD = { .34f, .33f, .33f };


    public void InitPool(Transform fishPoolParent) {

    }

    public void RespawnEntitiesMap(FishLevel playerFishLVL) {
        //Debug.LogWarning("Disabled Reinit Entities map");
        for (int i = 0; i < GameStaticInfo.POOL_FISH_SIZE; i++)
            if (visible_poolFishObjects[i].activeInHierarchy && visible_poolFishComponents[i].Level > playerFishLVL) {
                visible_poolFishComponents[i].Die();
                //Debug.LogWarning("Respawn entity: " + visible_poolFishComponents[i]);
            }
    }

    void FishDieAct(GameObject gameObject, FishLevel level, bool isPlayer) {
        DeactivateRespawnPrefab(gameObject, level, isPlayer, true);
        //if (isPlayer)
        //    Debug.LogWarning("Player Dead");
    }
}
