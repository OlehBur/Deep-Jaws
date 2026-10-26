using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

[RequireComponent(typeof(Fish))]
public class FishControl : MonoBehaviour, IFish {
    [SerializeField] FishAnim fishAnim;
    [SerializeField] Transform rotationTransform;
    [SerializeField] CapsuleCollider bodyCollider;
    [SerializeField] Fish fish;

    public Fish Fish => fish;
    public EnemyState State => _state;
    public Fish CurrentTarget { get; private set; }


    private EnemyState _state = EnemyState.Walking;
    private Rigidbody _rb;
    private Vector3 _direction = Vector3.zero;
    private Vector3 _targetPosition;
    private bool _isTargetSet = false; // flag to check if target is set
    private float _currentEscapeTime = 0f;
    //player movement
    private Vector2 _moveInput;
    private bool _isMoved = false;

    private const float TIME_CHANGE_ESCAPE_DIR = 3f;
    private const float WALKING_RADIUS_MULTIPLIER = 5f;
    private const float MIN_DISTANCE_TO_TARGET = 0.3f;
    private const float TIME_RECALC_WAY = .2f;
    private const float DIRECTION_ROTATE_MAGNITUDE_TRIGGER = .001f;
    private const float ESCAPE_ANGLE_HALF = 30f;
    private const float BYPASS_ANGLE_HALF = 60f;

    //reaction to other fish
    private readonly Collider[] overlapResults = new Collider[5];
    //public SpawnManager SpawnManager { get; private set; }
    private float _contactTime = 0f; // time of contact with other fish

    [Inject] private readonly IPlayerFishPoolService _playerProvider;
    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;



    public void Init(/*SpawnManager spawnManager*/) {
        //SpawnManager = spawnManager;
        //this.fish = fish;
        CurrentTarget = null; // Reset current target when enabled

        _rb = (_rb == null) ?
            GetComponent<Rigidbody>() : _rb;
        _rb.constraints = GameStaticInfo.DEFAULT_FISH_CONSTRAINS_RIQIDBODY;//in case it was stunned before
        _rb.linearVelocity = Vector3.zero; // Reset angular velocity to prevent unwanted rotation
        fishAnim = GetComponent<FishAnim>();

        CancelInvoke();
        SetDefaultState();

        if (fish.IsPlayer) {
            fish.IsActive = true;//player always active and visible, because always viwed on screen
            fish.IsVisible = true;
            JoyAxis.Axis = Vector2.zero;
            JoyAxis.OnAxisChanged += OnMove;
        }
        fish.OnFishUpdateSize += OnUpdateCollider;

        SleepPhysics();
    }

    /*public*/
    private void OnUpdateCollider(Vector3 localScale) {
        if (/*previewCollider &&*/ bodyCollider != null) {
            if (fish.Level == FishLevel.Crab) {//X height capsule
                bodyCollider.height = /*colliderDefaultHeight*/fish.DefaultInfo.ColliderDefaultHeight * localScale.x;
                bodyCollider.radius = /*colliderDefaultRadius */fish.DefaultInfo.ColliderDefaultRadius * localScale.z;
            }
            else {//Z height capsule
                bodyCollider.height = fish.DefaultInfo.ColliderDefaultHeight * localScale.z;
                bodyCollider.radius = fish.DefaultInfo.ColliderDefaultRadius * localScale.y;
            }
        }
    }

    //[SerializeField] Transform sizeTransform;
    //[SerializeField] bool previewCollider;
    //private void OnValidate() {
    //    if (previewCollider || bodyCollider == null)
    //        return;
    //    if (fish.Level == FishLevel.Crab) {//X height capsule
    //        bodyCollider.height = colliderDefaultHeight / Fish.GROWTH_MULTIPLIER * sizeTransform.localScale.x;
    //        bodyCollider.radius = colliderDefaultRadius / Fish.GROWTH_MULTIPLIER * sizeTransform.localScale.z;
    //    }
    //    else {//Z
    //        bodyCollider.height = colliderDefaultHeight / Fish.GROWTH_MULTIPLIER * sizeTransform.localScale.z;
    //        bodyCollider.radius = colliderDefaultRadius / Fish.GROWTH_MULTIPLIER * sizeTransform.localScale.y;
    //    }
    //    Debug.Log(colliderDefaultHeight / Fish.GROWTH_MULTIPLIER + " " + colliderDefaultRadius / Fish.GROWTH_MULTIPLIER);
    //}

    private void OnDisable() {
        // Cancel all delayed calls when disabled
        CancelInvoke();
        //SetDefaultState(); //HARD SHIT DO NOT UNCOMMENT !!!
        SleepPhysics();

        if (fish != null && fish.IsPlayer) {
            CurrentTarget = null;
            _isMoved = false;
            JoyAxis.Axis = Vector2.zero;
        }
    }

    private void OnDestroy() {
        //if (fish.IsPlayer)
        JoyAxis.OnAxisChanged -= OnMove; // for avoiding a warning, since OnDisable is ignored by the compiler
    }

    private void OnMove(Vector2 input/*InputAction.CallbackContext context*/) {
        _moveInput = input;
        //Debug.Log(input);
        _isMoved = input != Vector2.zero;
    }

    void FixedUpdate() {
        if (fish.IsPlayer)
            PlayerMovement();
        else if (IsFishNeedUpdate())
            EnemyMovementBehaviour();
        else
            SleepPhysics();// in case if during moving enemy enemy became inactive(stop)

    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsFishNeedUpdate() {
        if (_playerProvider.PlayerCurrentFish != null && _rb != null && _playerProvider.PlayerCurrentFish.Rb != null) {
            float distanceToPlayer = Vector3.Distance(_playerProvider.PlayerCurrentFish.Rb.position, _rb.position);
            if (distanceToPlayer > GameStaticInfo.PLAYER_MAX_DISTANCE_ACTIVITY) {
                if (fish.IsActive)
                    fish.IsActive = false;
                if (fish.IsVisible)
                    fish.IsVisible = false;

                return false; // do not update if too far from player
            }
            else {
                if (!fish.IsActive)
                    fish.IsActive = true;

                if (distanceToPlayer <= GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW && !fish.IsVisible)
                    fish.IsVisible = true;
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void EnemyMovementBehaviour() {
        switch (_state) {
            case EnemyState.Walking:
            if (!_isTargetSet)
                _direction = Walking();
            else
                _direction = _targetPosition - _rb.position; //if the position is set, but we have passed by or something, we continue to reach it

            ReactionToOtherFish();
            break;

            case EnemyState.Escape:
            _currentEscapeTime += DJTime.FixedDeltaTime;
            _direction = Escape();
            break;

            case EnemyState.Attack:
            _direction = Attack();
            break;
        }

        Movement(_direction);
    }

    //[SerializeField] float rotationSpeed = 1f;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void PlayerMovement() {
        if (!_isMoved) //parasyte movement detection
            SleepPhysics();

        _direction = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
        ApplyPhysicsMovement(fish.Speed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float GetMovingScalerByRotation(Quaternion currRotation, Vector3 moveDir) {
        if (moveDir == Vector3.zero)
            return 0f;

        Vector3 currentForward = currRotation * Vector3.forward;
        currentForward.y = 0;
        moveDir.y = 0;

        // clear ang fr 0° to 180° 
        float angle = Vector3.Angle(currentForward, moveDir);

        return Mathf.Lerp(1f, 0.5f, angle / 180f);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ReactionToOtherFish() {
        int count = Physics.OverlapSphereNonAlloc(_rb.position, fish./*ResponseRadius*/DefaultInfo.ResponseRadius, overlapResults, GameStaticInfo.GetEntityLayerMask());
        Fish closestFish = null;
        float dist = fish.DefaultInfo.ResponseRadius;
        for (int i = 0; i < count; i++) {
            if (overlapResults[i].attachedRigidbody == null || overlapResults[i].attachedRigidbody.gameObject == gameObject)
                continue;

            GameObject collideObj = overlapResults[i].attachedRigidbody.gameObject;
            float newDist = Vector3.Distance(_rb.position, collideObj.transform.position);
            if (dist > newDist) {//get closest fish
                dist = newDist;
                //Debug.Log($"{name} see {col.name} on dist {dist}");
                _spawnOrchestrator.TryGetFish(collideObj, out closestFish);//for attack or escape
            }
        }

        //update target only if fish found
        if (closestFish != null) {
            CurrentTarget = closestFish;
            FishProximity(closestFish);
        }
        else if (CurrentTarget == null) {
            // if target is lost, return to walking
            if (_state != EnemyState.Walking)
                SetDefaultState();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetTargetMovement() {//set target position on surf for movement
        if (_isTargetSet || fish == null)
            return;

        Vector3 randomDirection = Random.insideUnitSphere * RandomTargetPosMultiplier();
        randomDirection += _rb.position;//near pos obj

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, WALKING_RADIUS_MULTIPLIER * fish.DefaultInfo.ResponseRadius, NavMesh.AllAreas)) { //if point on surf
            _targetPosition = hit.position;
            _targetPosition.y = _rb.position.y; // keep the same height as the fish
            _isTargetSet = true;
            _state = EnemyState.Walking;
            _direction = Walking();
        }
        else {
            CancelInvoke(nameof(SetTargetMovement)); // cancel previous invoke if it exists
                                                     //³f no route is found then we are waiting for the interval
            Invoke(nameof(SetTargetMovement), TIME_RECALC_WAY);
        }
    }

    public Vector3 Walking() {
        SetTargetMovement();

        return _targetPosition - _rb.position;
    }

    public Vector3 Escape() {
        if (_currentEscapeTime >= TIME_CHANGE_ESCAPE_DIR || _targetPosition == _rb.position/*|| CurrentTarget == null*/) {//change new escape direction
            _currentEscapeTime = 0f;
            SetDefaultState();

            return Vector3.zero;
        }

        if (!_isTargetSet) {
            float randomAngle = Random.Range(-ESCAPE_ANGLE_HALF, ESCAPE_ANGLE_HALF);

            Vector3 awayDir = (_rb.position - CurrentTarget.Rb.position); // move away from the target
            awayDir.y = 0f; //only horizontal movement
            Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

            _targetPosition = _rb.position + rotation * awayDir * RandomTargetPosMultiplier();
            _targetPosition.y = _rb.position.y; // keep the same height as the fish
            _isTargetSet = true;
        }

        return _targetPosition - _rb.position;
    }

    float RandomTargetPosMultiplier() {
        const float MIN_RANGE = .3f;
        return Random.Range(MIN_RANGE, 1f) * fish.DefaultInfo.ResponseRadius * WALKING_RADIUS_MULTIPLIER;
    }

    public Vector3 Attack() {
        if (CurrentTarget == null
            || !CurrentTarget.gameObject.activeInHierarchy) {//victim is dead or not active
            SetDefaultState();

            return Vector3.zero;
        }

        return CurrentTarget.Rb.position - _rb.position; // move towards
    }

    void Movement(Vector3 dir) {
        dir.y = 0; // only horizontal movement

        float currentDistanceToTarget = dir.magnitude;
        currentDistanceToTarget = (currentDistanceToTarget < (fish.ScaleModifier.z + 1) * fish.DefaultInfo.JawFwdOffset) ?
            0 : currentDistanceToTarget;//stop moving if enouch close for bite

        switch (_state) {
            case EnemyState.Attack:
            if (CurrentTarget == null
                || !CurrentTarget.gameObject.activeInHierarchy
                || currentDistanceToTarget > fish.DefaultInfo.ResponseRadius
                || !fish.IsLegitimateTarget(CurrentTarget)
               ) {//if the target is lost
                SetDefaultState();
                return;
            }
            break;

            case EnemyState.Walking:
            case EnemyState.Escape:
            default://logic of changing state when the target is reached
            if (currentDistanceToTarget < MIN_DISTANCE_TO_TARGET) {//magnitude - length of vect
                SetDefaultState();
                return;
            }
            break;
        }

        _direction = dir.normalized;
        float smoother = Mathf.Clamp01(currentDistanceToTarget * .3f) * .95f + .05f; // from .1f to 1f
        ApplyPhysicsMovement(fish.Speed);
    }

    private void LateUpdate() {
        if (fish.IsActive) {
            //if (editableTransform == null) {
            //    Debug.Log(gameObject.name);
            //    return;
            //}

            float delta = (fish.IsPlayer) ?
                DJTime.UnscaledDeltaTime : DJTime.DeltaTime;
            Quaternion targetLook;

            if (_direction.sqrMagnitude > DIRECTION_ROTATE_MAGNITUDE_TRIGGER) {
                targetLook = Quaternion.LookRotation(_direction, Vector3.up);
                rotationTransform.rotation = Quaternion.RotateTowards(rotationTransform.rotation, targetLook, fish.RotationSpeed * delta);
                //rotationTransform.Rotate(Vector3.one);
            }
            else //standing still
                targetLook = rotationTransform.rotation;

            if (fishAnim != null && fishAnim.IsInited)
                fishAnim.RotateAnimation(rotationTransform.forward, _direction, delta);
        }
    }

    //[SerializeField] bool activatePhysRotation = true;
    protected void ApplyPhysicsMovement(float speed, float speedMultiplier = 1f) {
        // movement
        Vector3 targetPos = _direction * speed * speedMultiplier * GetMovingScalerByRotation(rotationTransform.rotation, _direction);

        if (targetPos != Vector3.zero) {
            targetPos.y = _rb.linearVelocity.y;
            _rb.linearVelocity = targetPos * DJTime.TimeScale;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetDefaultState() {
        SleepPhysics();
        _isTargetSet = false; // reset target when changing 
        _targetPosition = _rb.position; // reset target position
        CurrentTarget = null;
        _direction = Vector3.zero;
        //Debug.LogWarning("dir recalc disable");
        _state = EnemyState.Walking;
        //currentEscapeTime = 0f;
        Vector3 currentForward = rotationTransform.rotation * Vector3.forward;

        if (fishAnim != null && fishAnim.IsInited)
            fishAnim.RotateAnimation(rotationTransform.forward, currentForward, DJTime.FixedDeltaTime);//zero rotation
    }

    public void FishProximity(Fish otherFish) {
        if (fish.Level < otherFish.Level)
            _state = EnemyState.Escape;
        else if (fish.IsLegitimateTarget(otherFish, true) && fish.Level > GameStaticInfo.PEACEFUL_FISH_LVL) //u not crab
            _state = EnemyState.Attack;
        else
            _state = EnemyState.Walking; //If the player attacks the fish the same level, only then it will start attacking
    }

    void TryContactWithBehaviour(Collider collider, bool isStayContact) {
        if (collider.attachedRigidbody == null)
            return;

        GameObject victimObj = collider.attachedRigidbody.gameObject;
        //GameObject collisObj = collider.gameObject;

        //if other fish
        if (transform != victimObj//not self collision
             && _spawnOrchestrator.TryGetFish(victimObj, out Fish other)     //if collision with another fish
        && (fish.IsPlayer || other == CurrentTarget) //collision obj is our victim
            && fish.IsLegitimateTarget(other)//victim is weaker
            ) {

            if (isStayContact) {
                //Debug.Log("stay Bites: " + Time.time);
                _contactTime += (fish.IsPlayer) ?
                    DJTime.UnscaledDeltaTime : DJTime.FixedDeltaTime;

                if (_contactTime >= fish.DeffaultAttackTime) {//damage each second
                    _contactTime = 0f;
                    fish.Eat(other, out bool isStillAlive);
                    SleepPhysics();

                    if (fish.IsPlayer)
                        CheckEatAchievements(other.Level);
                }
            }
            else {
                fish.Eat(other, out bool isStillAlive);

                //Debug.Log("First Bite: " + Time.time);
                if (fish.IsPlayer)
                    CheckEatAchievements(other.Level);
                if (!isStillAlive) //on enter
                    SetDefaultState();
            }
        }
    }


    //private void OnCollisionExit(Collision collision) {
    //    SleepPhysics();//fix ugly rotation freeze
    //}

    private void OnTriggerEnter(Collider other) {
        if (fish.IsActive)
            TriggerEnterStayBehavior(other);//for first bite
    }

    private void OnTriggerStay(Collider other) {//mandatory funct for food at the time 
        if (fish.IsActive)
            TriggerEnterStayBehavior(other, true);
    }

    void TriggerEnterStayBehavior(Collider other, bool isStayTrigger = false) {
        if (other.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD))
            SleepPhysics();
        //else if ((!(state == EnemyState.Escape) || !isStayTrigger) && ( //commented because implemented in collision
        //    other.CompareTag(GameStaticInfo.TAG_POISON_TRIGGER)
        //    //|| other.CompareTag(GameStaticInfo.TAG_OBSTACLE)
        //    || other.CompareTag(GameStaticInfo.TAG_SPAWN_ZONE)))
        //    BypassObstacle(/*other.ClosestPoint(rb.position)*/GetPosiblyCollisionPoint()); 
        else if (fish.Level > GameStaticInfo.PEACEFUL_FISH_LVL && !other.isTrigger)
            TryContactWithBehaviour(other, isStayTrigger);
    }

    //Vector3 GetPosiblyCollisionPoint() => rb.position + rotationTransform.rotation * Vector3.forward * fish.JawForwardOffset;

    //obstacles etc.
    private void OnCollisionEnter(Collision collision) {
        if (fish.IsActive)
            CollisionEnterStayBehaviour(collision);
        //SleepPhysics();
    }
    private void OnCollisionStay(Collision collision) {
        if (fish.IsActive)
            CollisionEnterStayBehaviour(collision);
    }

    void CollisionEnterStayBehaviour(Collision collision) {
        GameObject collisObj = collision.collider.gameObject;

        if ((_state != EnemyState.Escape /*&& !collision.collider.isTrigger*/)
            && (collision.collider.CompareTag(GameStaticInfo.TAG_POISON_TRIGGER)//if poison object - IMMEDIATELY bypass
            || collisObj.CompareTag(GameStaticInfo.TAG_OBSTACLE)
            //|| (!fish.IsPlayer && collision.collider is SphereCollider && collisObj.CompareTag(GameStaticInfo.TAG_SPAWN_ZONE))
            || collisObj.layer == LayerMask.NameToLayer(GameStaticInfo.LAYER_OBSTACLE))
            /*|| collisObj.layer == LayerMask.NameToLayer(GameStaticInfo.LAYER_CAMERA_AVOID_COLLIDERS)*/)   //fish is stuck on obstacle
            BypassObstacle(collision.GetContact(0).point);
    }


    void CheckEatAchievements(FishLevel victimLvl) {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (victimLvl == GameStaticInfo.PEACEFUL_FISH_LVL)//if eat crab
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_youre_my_friend_now);
        else if (victimLvl == fish.Level)//same lvl
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_cannibalism);
        else if (victimLvl == FishLevel.Mackerel && fish.Level == FishLevel.Moray)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_former_threat_current_snack);
#endif
    }

    /// <summary>
    /// returns a direction for movement that is randomly opposite to the position of the obstacle
    /// </summary>
    /// <param name="obstaclePos"></param>
    /// <returns></returns>
    void BypassObstacle(Vector3 obstaclePos) {
        SleepPhysics();//only before bypass
        float randomAngle = Random.Range(-BYPASS_ANGLE_HALF, BYPASS_ANGLE_HALF);

        _state = EnemyState.Escape;
        _isTargetSet = true;
        CurrentTarget = null;

        Vector3 newDir = (_rb.position - obstaclePos); // move away from the target
        newDir.y = 0f; // only horizontal movement
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

        _targetPosition = _rb.position + rotation * newDir * RandomTargetPosMultiplier();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SleepPhysics() {
        _direction = Vector3.zero;
        if (_rb != null) {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            //rb.Sleep(); // freeze physics calculations
        }
    }

    private void OnDrawGizmos() {
        if (fish == null || !gameObject.activeInHierarchy || _rb == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_rb.position, fish.DefaultInfo.ResponseRadius);

        if (fish.IsPlayer) {//for player
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(_rb.position, fish.DefaultInfo.ResponseRadius);
            return;
        }

        if (_isTargetSet) {
            switch (_state) {
                case EnemyState.Attack:
                if (CurrentTarget != null) {
                    Gizmos.color = Color.red;
                    //Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(_rb.position, CurrentTarget.Rb.position);
                    Gizmos.DrawWireCube(_rb.position, Vector3.one * 1.5f);
                }
                return;

                case EnemyState.Escape:
                Gizmos.color = Color.blue;
                break;

                case EnemyState.Walking:
                Gizmos.color = Color.green;
                break;
            }
            if (_targetPosition == null)
                return;

            Gizmos.DrawWireSphere(_targetPosition, 0.5f);
            Gizmos.DrawLine(_rb.position, _targetPosition);
        }
    }
}
