using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class CaviarController : MonoBehaviour {
    //[SerializeField] SpawnManager spawnManager;
    [SerializeField] GameObject caviarModel;

    [Inject] private readonly IPlayerFishPoolService _playeyPoolService;
    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;

    private float _wavingAplitude;

    private const float WAVING_SPEED = 1.2f;
    private float _current_reEnablerTime = 0f;
    private float _defaultY;
    private float _randinInitProgress;

    private const float MAX_REENABLE_TIME = 300f; // max time to re-enable the caviar object
    private const string KEY_STR = "caviar_time_";
    private const int MAX_CAVIAR_EATEN_CNT = 35;//to trigger achievement Gourmet

    private static int PlayerEatedCaviarCnt { get; set; }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _wavingAplitude = (1.3f - transform.localScale.x) * .5f;
        _defaultY = transform.localPosition.y;
        _randinInitProgress = Random.Range(0f, 360f);
        _current_reEnablerTime = PlayerPrefs.GetFloat(KEY_STR + gameObject.name, 0f);
        caviarModel.SetActive(_current_reEnablerTime == 0f);
    }

    // Update is called once per frame
    void Update() {
        float y = Mathf.Sin((Time.time + _randinInitProgress) * WAVING_SPEED) * _wavingAplitude;
        transform.localPosition = new Vector3(transform.localPosition.x, _defaultY + y, transform.localPosition.z);

        if (!caviarModel.activeInHierarchy)
            if (_current_reEnablerTime >= MAX_REENABLE_TIME)
                ActivateCaviar();
            else
                _current_reEnablerTime += DJTime.DeltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        Transform parentFish = other.attachedRigidbody.transform;

        if (caviarModel.activeInHierarchy && other.isTrigger//only mouth (trigger collider)
            && parentFish.CompareTag("Player")
            && _spawnOrchestrator.TryGetFish(parentFish.gameObject, out Fish fish)) {
            fish.FeedCaviar();

            ++PlayerEatedCaviarCnt;

#if UNITY_ANDROID 
            if (PlayerEatedCaviarCnt >= MAX_CAVIAR_EATEN_CNT)
                GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_overseas_gourmet);
#endif

            caviarModel.SetActive(false);//disable caviar object
            _current_reEnablerTime = 0f;
        }
    }

    void OnDestroy() {
        PlayerPrefs.SetFloat(KEY_STR + gameObject.name, _current_reEnablerTime);
    }

    void ActivateCaviar() {
        if (_playeyPoolService.PlayerCurrentFish == null)
            return;

        if (Vector3.Distance(_playeyPoolService.PlayerCurrentFish.transform.position, transform.position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW)
            /*gameObject*/
            caviarModel.SetActive(true);
        else
            _current_reEnablerTime = 0f;
    }
}
