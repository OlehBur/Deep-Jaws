using UnityEngine;
using VContainer;

/// <summary>
/// CameraFollower забезпечує плавне слідування за гравцем із динамічним
/// обминанням перешкод через SphereCast та підтримкою режиму огляду мапи.
/// </summary>
public class CameraFollower : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Camera cameraObj;
    [SerializeField] private Transform dynamicCamera;
    [Header("Obstacle Avoidance Settings")]
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float cameraRadius = 0.4f;
    [SerializeField] private float minDistanceToPlayer = 1.5f;
    [SerializeField] private float playerPivotHeight = 1.0f;

    public static CameraFollower Instance { get; private set; }
    public bool IsMapViewActive { get; set; } = false;

    private const float DEFAULT_HEIGHT_OFFSET = 6.5f;
    private const float DEFAULT_FORWARD_OFFSET = 3.5f;
    private const float MAX_PROGRESS_RESPONS = 2f;
    private const float MIN_PROGRESS_VALUE = 0.1f;
    private const float INITIAL_FOLLOW_SMOOTH_TIME = 2f;
    private const float DEFAULT_FOLLOW_SMOOTH_TIME = 0.3f;
    private const float SMOOTH_EPSILON = 0.01f;
    private const float CAM_INIT_FAR_CLIP = 97f;
    private const float CAM_DEFAULT_FAR_CLIP = 160f;
    private const float MAP_VIEW_HEIGHT_OFFSET = 50f;
    private const float MAP_VIEW_FORWARD_OFFSET = 20f;


    [Inject] private readonly IPlayerFishPoolService _playerPoolService;

    private Transform _target;
    private Fish _playerFish;
    private Vector3 _initialPosition = new Vector3(0, 38.64f, -20.52f);
    private Vector3 _currentVelocity;
    private float _currentSmoothTime = INITIAL_FOLLOW_SMOOTH_TIME;



    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    void /*Init*/Start(/*SpawnManager spawnManager*/) {
        //this.spawnManager = spawnManager;
        _playerPoolService.OnPlayerRespawn += ResetPlayerTarget;

        if (_playerPoolService.PlayerCurrentFish != null) {
            _target = _playerPoolService.PlayerCurrentFish.transform;
            _playerFish = _playerPoolService.PlayerCurrentFish;
        }

        if (cameraObj != null)
            cameraObj.farClipPlane = CAM_INIT_FAR_CLIP;
    }

    private void ResetPlayerTarget(Fish player, bool isDie) {
        if (player == null)
            return;

        _target = player.transform;
        _playerFish = player;

        _currentSmoothTime = INITIAL_FOLLOW_SMOOTH_TIME;
        _currentVelocity = Vector3.zero;
    }

    private void LateUpdate() {
        UpdateCameraMovement();
    }

    private void UpdateCameraMovement() {
        Vector3 desiredPosition;

        if (_playerFish != null && _target != null) {
            float lvlCoefficient = 0.5f * (int)_playerFish.Level;
            float progressCoefficient = 1f + Mathf.Min(_playerFish.Progress, MAX_PROGRESS_RESPONS) + MIN_PROGRESS_VALUE;

            float calculatedHeight = DEFAULT_HEIGHT_OFFSET + lvlCoefficient * progressCoefficient;
            float calculatedForward = DEFAULT_FORWARD_OFFSET + lvlCoefficient * progressCoefficient;

            if (IsMapViewActive) {
                calculatedHeight += MAP_VIEW_HEIGHT_OFFSET;
                calculatedForward += MAP_VIEW_FORWARD_OFFSET;
            }

            desiredPosition = new Vector3(
                _target.position.x,
                _target.position.y + calculatedHeight,
                _target.position.z - calculatedForward
            );
        }
        else {
            desiredPosition = _initialPosition;
        }

        //dynam avoidamc obstacles ( SphereCast)
        Vector3 finalTargetPosition = desiredPosition;

        if (_target != null && !IsMapViewActive) {
            // point on player (begin ray)
            Vector3 pivotPoint = _target.position + Vector3.up * playerPivotHeight;
            Vector3 rayDirection = desiredPosition - pivotPoint;
            float maxDistance = rayDirection.magnitude;

            if (maxDistance > 0.001f) {
                rayDirection.Normalize();

                // sphere in cam dir
                if (Physics.SphereCast(pivotPoint, cameraRadius, rayDirection, out RaycastHit hit, maxDistance, obstacleMask, QueryTriggerInteraction.Ignore)) {
                    //only obst offset
                    float safeDistance = Mathf.Max(hit.distance - cameraRadius, minDistanceToPlayer);
                    finalTargetPosition = pivotPoint + rayDirection * safeDistance;
                }
            }
        }

        // Smooth cam movement
        if ((_currentSmoothTime - DEFAULT_FOLLOW_SMOOTH_TIME) > SMOOTH_EPSILON) {
            _currentSmoothTime = Mathf.Lerp(_currentSmoothTime, DEFAULT_FOLLOW_SMOOTH_TIME, Time.unscaledDeltaTime);
        }
        else {
            _currentSmoothTime = DEFAULT_FOLLOW_SMOOTH_TIME;
        }

        dynamicCamera.position = Vector3.SmoothDamp(
            dynamicCamera.position,
            finalTargetPosition,
            ref _currentVelocity,
            _currentSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        // Far Clip Plane
        if (cameraObj != null) {
            float targetFarClip = IsMapViewActive ? CAM_DEFAULT_FAR_CLIP : CAM_DEFAULT_FAR_CLIP;
            cameraObj.farClipPlane = Mathf.Lerp(cameraObj.farClipPlane, targetFarClip, Time.unscaledDeltaTime);
        }
    }

    private void OnDisable() {
        if (_playerPoolService != null) 
            _playerPoolService.OnPlayerRespawn -= ResetPlayerTarget;
    }
}