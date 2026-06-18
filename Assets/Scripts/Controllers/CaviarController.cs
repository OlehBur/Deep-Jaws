using Unity.VisualScripting;
using UnityEngine;

public class CaviarController : MonoBehaviour {
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] GameObject caviarModel;
    private const float WAVING_SPEED = 1.2f;
    private /*const*/ float WAVING_AMPLITUDE;
    private const float MAX_REENABLE_TIME = 300f; // max time to re-enable the caviar object
    private float current_reEnablerTime = 0f;
    private float defaultY;
    private float randinInitProgress;
    private const string KEY_STR = "caviar_time_";
    private static int playerEatedCaviarCnt { get; set; }
    private const int MAX_CAVIAR_EATEN_CNT = 35;//to trigger achievement Gourmet


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        WAVING_AMPLITUDE = (1.3f - transform.localScale.x) * .5f;
        defaultY = transform.localPosition.y;
        randinInitProgress = Random.Range(0f, 360f);
        current_reEnablerTime = PlayerPrefs.GetFloat(KEY_STR + gameObject.name, 0f);
        caviarModel.SetActive(current_reEnablerTime == 0f);
    }

    // Update is called once per frame
    void Update() {       
        float y = Mathf.Sin((Time.time + randinInitProgress) * WAVING_SPEED) * WAVING_AMPLITUDE;
        transform.localPosition = new Vector3(transform.localPosition.x, defaultY + y, transform.localPosition.z);

        if (!caviarModel.activeInHierarchy)
            if (current_reEnablerTime >= MAX_REENABLE_TIME)
                ActivateCaviar();
            else
                current_reEnablerTime += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        Transform parentFish = other.transform.parent;

        if (caviarModel.activeInHierarchy && other.isTrigger//only mouth (trigger collider)
            && parentFish.CompareTag("Player")
            && GameStaticInfo.TryGetFish(parentFish.gameObject, out Fish fish)) {
            fish.FeedCaviar();

            ++playerEatedCaviarCnt;
            if (playerEatedCaviarCnt >= MAX_CAVIAR_EATEN_CNT)
                GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_overseas_gourmet);

            caviarModel.SetActive(false);//disable caviar object
            current_reEnablerTime = 0f;
        }
    }

    void OnDestroy() {
        PlayerPrefs.SetFloat(KEY_STR + gameObject.name, current_reEnablerTime);
    }

    void ActivateCaviar() {
        if (spawnManager.PlayerFish == null)
            return;

        if (Vector3.Distance(spawnManager.PlayerFish.transform.position, transform.position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW)
            /*gameObject*/
            caviarModel.SetActive(true);
        else 
            current_reEnablerTime = 0f;
    }
}
