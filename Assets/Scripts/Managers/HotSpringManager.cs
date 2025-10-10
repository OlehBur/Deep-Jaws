using UnityEngine;


/// <summary>
/// активує бульбашку кожен рандомний проміжок часу за умови відстутності вже активованої бульбашки 
/// activates a bubble at random intervals if no bubble is currently active.
/// </summary>
public class HotSpringManager : MonoBehaviour {
    [SerializeField] GameObject bubbleChild;
    [SerializeField] Transform spawnPoint;
    private const float MAX_SPAWN_INTERVAL = 8f; // maximum time between bubble spawns
    private const float MIN_SPAWN_INTERVAL = 1.5f; // minimum time between bubble spawns

    private float timeToSpawn;
    private float currentTime;

    void Start() {
        timeToSpawn = Random.Range(MIN_SPAWN_INTERVAL, MAX_SPAWN_INTERVAL);
    }


    void Update() {
        if (!bubbleChild.activeInHierarchy) {
            currentTime += Time.deltaTime;
            if (currentTime >= timeToSpawn) {
                currentTime = 0f;
                timeToSpawn = Random.Range(MIN_SPAWN_INTERVAL, MAX_SPAWN_INTERVAL);
                bubbleChild.transform.position = spawnPoint.position;
                bubbleChild.SetActive(true);
            }
        }
    }
}
