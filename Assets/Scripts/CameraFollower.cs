using UnityEngine;

/// <summary>
/// CameraFollower клас що змушує камеру слідукувати за гравцем
/// і додається як компонент до незалежного коллайдера що однаково дочірній до об'єкта Visibility Checker як і сама камера - 
/// CameraFollower is a class that makes the camera follow the player and is added as a component to an
/// independent collider that is equally a child of the Visibility Checker object as the camera itself.
/// </summary>
//[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CameraFollower : MonoBehaviour {
    [SerializeField] Camera cameraObj;
    [SerializeField] Transform dynamicCamera; //layer for obstacles
    private const float CAM_HEIGHT_OBSTACLE_OFFSET = 25f; // height offset for camera obstacle avoidance
    private const float CAM_HEIGHT_MAP_OFFSET = 50f; // height offset for camera obstacle avoidance
    private float currentHeightOffset = CAM_HEIGHT_OBSTACLE_OFFSET;
    bool isDetectObstacle = false; // flag for obstacle detection
    bool isActivateAvoidence = false;

    private Transform target; //player transform
    private readonly Vector3 initialPosition = new Vector3(0, 38.64f, -20.52f);
    private Vector3 targetPosition = Vector3.zero;
    private Vector3 currentVelocity;
    private Fish playerFish; //player fish component
    private const float DEFAULT_HEIGHT_OFFSET = 6.5f;
    private const float DEFAULT_FORWARD_OFFSET = 3.5f;
    private const float MIN_PROGRESS_VALUE = 0.1f; // minimal progress value
    private const float MAX_PROGRESS_RESPONS = 2f; // max progress value for camera follow
    private const float INITIAL_FOLLOW_SMOOTH_TIME = 2f; // initial smooth follow velocity
    private const float DEFAULT_FOLLOW_SMOOTH_TIME = .3f; // default smooth follow velocity
    private const float SMOOTH_EPSILON = .01f;
    private const float CAM_INIT_FAR_CLIP = 97f;
    private const float CAM_DEFAULT_FAR_CLIP = 160f;

    private float currentSmoothTime = INITIAL_FOLLOW_SMOOTH_TIME; // current smooth follow velocity
    private SpawnManager spawnManager;


    public void Init(SpawnManager spawnManager) {
        this.spawnManager = spawnManager;
        spawnManager.OnPlayerRespawn += ResetPlayerTarget;
        //spawnManager.OnPlayerDie += ReInitCameraFollowing;

        target = spawnManager.PlayerFish.transform/*pool[SpawnManager.PLAYER_INDEX].transform*/; //get player transform
        playerFish = spawnManager.PlayerFish;//target.GetComponent<Fish>(); //get player fish component
        //initialPosition = transform.position;
        cameraObj.farClipPlane = CAM_INIT_FAR_CLIP;
    }

    /// <summary>
    /// init after player respawn
    /// </summary>
    void ResetPlayerTarget(Fish player, bool isDie) {
        target = /*spawnManager.pool[SpawnManager.PLAYER_INDEX]*/player.transform; //get player transform
        playerFish = player;//target.GetComponent<Fish>();

        isActivateAvoidence = false;// while camera is not close enough to player position, do not activate obstacle avoidance
        currentSmoothTime = INITIAL_FOLLOW_SMOOTH_TIME;

        currentVelocity = Vector3.zero;
    }

    //void ReInitCameraFollowing() {
    //    isActivateAvoidence = false;// while camera is not close enough to player position, do not activate obstacle avoidance
    //    currentSmoothTime = INITIAL_FOLLOW_SMOOTH_TIME;

    //    currentVelocity = Vector3.zero;

    //    target = spawnManager.PlayerFish.transform;
    //    playerFish = spawnManager.PlayerFish;

    //    //cameraObj.transform.position = Vector3.SmoothDamp(
    //    //    transform.position,
    //    //    initialPosition,
    //    //    ref currentVelocity,
    //    //    DEFAULT_FOLLOW_SMOOTH_TIME,
    //    //    Mathf.Infinity, Time.unscaledDeltaTime); // reset camera position   
    //}

    // fixed update for camera follow becaurse target is moving with physics
    void LateUpdate() {
        DynamicCameraObjMovement();

        //if (isActivateAvoidence)
        ObstacleAvoidance();

        //Debug.Log(targetPosition);
    }

    void ObstacleAvoidance() {
        if (playerFish == null)
            return;

        const float AVOIDANCE_SPEED = 2.3f;
        const float MIN_DISTANCE_CANERA_TO_ORIGIN = 0.01f;
        const float FAR_CLIP_CHANGE_SPEED = 5f;
        float currentDistanceToOrigin = Vector3.Distance(transform.position, cameraObj.transform.position);

        if (isDetectObstacle && currentDistanceToOrigin < /*CAM_HEIGHT_OBSTACLE_OFFSET*/currentHeightOffset) {//if obstacle detected and we are not far enough, then do this
            cameraObj.transform.position =
                Vector3.Lerp(cameraObj.transform.position,
                cameraObj.transform.position - cameraObj.transform.forward * /*CAM_HEIGHT_OBSTACLE_OFFSET*/currentHeightOffset,
                Time.unscaledDeltaTime * AVOIDANCE_SPEED);

            cameraObj.farClipPlane = Mathf.Lerp(cameraObj.farClipPlane, CAM_DEFAULT_FAR_CLIP, Time.unscaledDeltaTime * FAR_CLIP_CHANGE_SPEED);
        }
        else if (!isDetectObstacle && Vector3.Distance(transform.position, cameraObj.transform.position) > MIN_DISTANCE_CANERA_TO_ORIGIN) {// if obstacle is not detected and we are far enough then return to normal position
            cameraObj.transform.position =
                Vector3.Lerp(cameraObj.transform.position,
                transform.position,
                Time.unscaledDeltaTime * AVOIDANCE_SPEED);

            cameraObj.farClipPlane = Mathf.Lerp(cameraObj.farClipPlane, CAM_INIT_FAR_CLIP, Time.unscaledDeltaTime * FAR_CLIP_CHANGE_SPEED); ;
        }
    }

    void DynamicCameraObjMovement() {
        if (playerFish != null) {
            float lvlCoefficient = 0.5f * (int)playerFish.Level; // coefficient for fish level influence on camera position
            float proggressCoefficient = 1f + Mathf.Min(playerFish.Progress, MAX_PROGRESS_RESPONS) + MIN_PROGRESS_VALUE; // ensure progress is not zero to avoid division by zero

            targetPosition = new Vector3(
                target.position.x,
                target.position.y + DEFAULT_HEIGHT_OFFSET + lvlCoefficient * proggressCoefficient,//height offset based on fish level
                target.position.z - DEFAULT_FORWARD_OFFSET - lvlCoefficient * proggressCoefficient
                );
        }
        else
            targetPosition = initialPosition;

        //initial camera following
        if ((currentSmoothTime - DEFAULT_FOLLOW_SMOOTH_TIME) > SMOOTH_EPSILON) {
            currentSmoothTime = Mathf.Lerp(currentSmoothTime, DEFAULT_FOLLOW_SMOOTH_TIME, Time.unscaledDeltaTime);
            /*transform*/
            dynamicCamera.position = Vector3.SmoothDamp(dynamicCamera.position, targetPosition, ref currentVelocity,
                currentSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);//, Mathf.Infinity, Time.unscaledDeltaTime for Independence from timeScale
            cameraObj.farClipPlane = Mathf.Lerp(cameraObj.farClipPlane, CAM_DEFAULT_FAR_CLIP, Time.unscaledDeltaTime);
        }
        else { //default camera following
            dynamicCamera.position = Vector3.SmoothDamp(dynamicCamera.position, targetPosition, ref currentVelocity,
                DEFAULT_FOLLOW_SMOOTH_TIME, Mathf.Infinity, Time.unscaledDeltaTime);

            if (!isActivateAvoidence)
                isActivateAvoidence = true;
        }
    }

    private void OnDisable() {
        if (spawnManager == null)
            return;

        spawnManager.OnPlayerRespawn -= ResetPlayerTarget;
        //spawnManager.OnPlayerDie -= ReInitCameraFollowing;
    }

    private void OnTriggerEnter(Collider collision) {
        //Debug.Log("Camera obstacle detection started");
        TriggerBehaviour(collision, true);
        //if (collision.CompareTag("Obstacle"))
        //    isDetectObstacle = true;

    }

    private void OnTriggerStay(Collider other) {
        TriggerBehaviour(other, true);
    }

    void OnTriggerExit(Collider collision) {
        //Debug.Log("Camera obstacle detection ended");
        TriggerBehaviour(collision, false);
    }

    void TriggerBehaviour(Collider collider, bool isEnter) {
        //if (collider.CompareTag("Obstacle")) {
        currentHeightOffset = CAM_HEIGHT_OBSTACLE_OFFSET;
        isDetectObstacle = isEnter;
        //}
        //else
        if (collider.CompareTag("MapReview") && isEnter)
            //if (isEnter)
            currentHeightOffset = CAM_HEIGHT_MAP_OFFSET;
        //else
        //    currentHeightOffset = CAM_HEIGHT_OBSTACLE_OFFSET;

        //currentHeightOffset = CAM_HEIGHT_MAP_OFFSET;
        //isDetectObstacle = isEnter;
        //}
    }
}
