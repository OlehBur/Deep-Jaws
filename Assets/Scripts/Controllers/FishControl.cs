using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Fish))]
public class FishControl : MonoBehaviour, IFish {
    /// <summary>
    /// variables made through the inspector but only 
    /// available through the enemy interface fields
    /// </summary>
    [SerializeField] float ³dleTime;
    [SerializeField] float timeToIdle;
    [SerializeField] FishAnim fishAnim;
    [SerializeField] Transform rotationTransform;
    [SerializeField] CapsuleCollider bodyCollider;
    [SerializeField] float colliderDefaultHeight = 1f;
    [SerializeField] float colliderDefaultRadius = 0.5f;

    private IFish IFish;
    Fish IFish.CurrentTarget { get; set; }
    float IFish.IdleTime => ³dleTime;
    float IFish.TimeToIdle => timeToIdle;

    private EnemyState state = EnemyState.Walking;
    /// <summary>
    /// The State is hidden to change the outside of the class but is available for reading and sent to a variable that can change in class
    /// </summary>
    public EnemyState State => state;

    //movement
    /*private*/
    [SerializeField] Fish fish;
    public Fish Fish => fish;

    private Rigidbody rb;
    private Vector3 direction = Vector3.zero;
    private Vector3 targetPosition;
    //private Vector3 rotateVelocity = Vector3.zero;
    private bool isTargetSet = false; // flag to check if target is set
    private const float TIME_CHANGE_ESCAPE_DIR = 3f;
    private float currentEscapeTime = 0f;
    private const float WALKING_RADIUS_MULTIPLIER = 5f;
    private const float MIN_DISTANCE_TO_TARGET = 0.3f;
    private const float TIME_RECALC_WAY = .2f;
    private const float DIRECTION_ROTATE_MAGNITUDE_TRIGGER = .001f;
    private const float ESCAPE_ANGLE_HALF = 30f;
    private const float BYPASS_ANGLE_HALF = 60f;

    //player movement
    private Vector2 moveInput;
    //private Vector3 moveDir = Vector3.zero;
    //private Vector3 newRotation;
    //private Vector3 currentForward;
    private bool isMoved = false;
    //private const float ROTATION_SMOOTH_TIME = .1f;

    //reaction to other fish
    private readonly Collider[] overlapResults = new Collider[5];
    //private float ResponseRadius = /*enemy.Speed*1.4f*/12f;
    public SpawnManager SpawnManager { get; private set; }
    private float contactTime = 0f; // time of contact with other fish


    public void Init(SpawnManager spawnManager/*, Fish fish*/) {
        SpawnManager = spawnManager;
        IFish = this;
        //this.fish = fish;
        IFish.CurrentTarget = null; // Reset current target when enabled

        rb = (rb == null) ?
            GetComponent<Rigidbody>() : rb;
        rb.constraints = GameStaticInfo.DEFAULT_FISH_CONSTRAINS_RIQIDBODY;//in case it was stunned before
        rb.linearVelocity = Vector3.zero; // Reset angular velocity to prevent unwanted rotation
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

    /*public*/private void OnUpdateCollider(Vector3 localScale) {
        if (/*previewCollider &&*/ bodyCollider != null) {
            if (fish.Level == FishLevel.Crab) {//X height capsule
                bodyCollider.height = colliderDefaultHeight * localScale.x;
                bodyCollider.radius = colliderDefaultRadius * localScale.z;
            }
            else {//Z height capsule
                bodyCollider.height = colliderDefaultHeight * localScale.z;
                bodyCollider.radius = colliderDefaultRadius * localScale.y;
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

    //void Awake() {
    //    IEnemy = this;
    //    rb = GetComponent<Rigidbody>();
    //    fishLayerMask = LayerMask.GetMask("Entity");
    //}

    //void Start() {
    //    SetDefaultState(); // Set initial state after full initialization
    //}

    private void OnDisable() {
        // Cancel all delayed calls when disabled
        CancelInvoke();
        //SetDefaultState(); //HARD SHIT DO NOT UNCOMMENT !!!
        SleepPhysics();
        //JoyAxis.OnAxisChanged -= OnMove;

        if (fish != null && fish.IsPlayer) {
            if (IFish != null)
                IFish.CurrentTarget = null;
            isMoved = false;
            JoyAxis.Axis = Vector2.zero;
            //JoyAxis.isReset = true;
        }
    }

    private void OnDestroy() {
        //if (fish.IsPlayer)
        JoyAxis.OnAxisChanged -= OnMove; // for avoiding a warning, since OnDisable is ignored by the compiler
    }

    private void OnMove(Vector2 input/*InputAction.CallbackContext context*/) {
        moveInput = input;
        //Debug.Log(input);
        isMoved = input != Vector2.zero;
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
        if (SpawnManager != null && SpawnManager.PlayerFish != null && rb != null && SpawnManager.PlayerFish.Rb != null) {
            float distanceToPlayer = Vector3.Distance(SpawnManager.PlayerFish./*transform*/Rb.position, /*transform.position*/rb.position);
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
        if (IFish == null)
            return;

        switch (state) {
            case EnemyState.Walking:
            if (!isTargetSet)
                direction = IFish.Walking();
            else
                direction = targetPosition - rb.position; //if the position is set, but we have passed by or something, we continue to reach it

            ReactionToOtherFish();
            break;

            case EnemyState.Escape:
            currentEscapeTime += Time.fixedDeltaTime;
            direction = IFish.Escape();
            break;

            case EnemyState.Attack:
            direction = IFish.Attack();
            break;
        }

        Movement(direction);
    }

    //[SerializeField] float rotationSpeed = 1f;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void PlayerMovement() {
        if (!isMoved) //parasyte movement detection
            SleepPhysics();

        direction = new Vector3(moveInput.x, 0, moveInput.y).normalized;
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
        int count = Physics.OverlapSphereNonAlloc(rb.position, fish.ResponseRadius, overlapResults, GameStaticInfo.LAYER_MASK_ENTITY);
        Fish closestFish = null;
        float dist = fish.ResponseRadius;
        for (int i = 0; i < count; i++) {
            if(overlapResults[i].attachedRigidbody==null || overlapResults[i].attachedRigidbody.gameObject == gameObject)
                continue;

            GameObject collideObj = overlapResults[i].attachedRigidbody.gameObject;
            float newDist = Vector3.Distance(rb.position, collideObj.transform.position);
            if (dist > newDist) {//get closest fish
                dist = newDist;
                //Debug.Log($"{name} see {col.name} on dist {dist}");
                GameStaticInfo.TryGetFish(collideObj, out closestFish);//for attack or escape
            }
        }

        //update target only if fish found
        if (closestFish != null) {
            IFish.CurrentTarget = closestFish;
            IFish.FishProximity(closestFish);
        }
        else if (IFish?.CurrentTarget == null) {
            // if target is lost, return to walking
            if (state != EnemyState.Walking)
                SetDefaultState();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetTargetMovement() {//set target position on surf for movement
        if (isTargetSet || fish == null)
            return;

        Vector3 randomDirection = Random.insideUnitSphere * RandomTargetPosMultiplier();
        randomDirection += rb.position;//near pos obj

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, WALKING_RADIUS_MULTIPLIER * fish.ResponseRadius, NavMesh.AllAreas)) { //if point on surf
            targetPosition = hit.position;
            targetPosition.y = rb.position.y; // keep the same height as the fish
            isTargetSet = true;
            state = EnemyState.Walking;
            direction = IFish.Walking();
        }
        else {
            CancelInvoke(nameof(SetTargetMovement)); // cancel previous invoke if it exists
                                                     //³f no route is found then we are waiting for the interval
            Invoke(nameof(SetTargetMovement), TIME_RECALC_WAY);
        }
    }

    Vector3 IFish.Walking() {
        SetTargetMovement();

        return targetPosition - rb.position;
    }

    Vector3 IFish.Escape() {
        if (currentEscapeTime >= TIME_CHANGE_ESCAPE_DIR || targetPosition == rb.position/*|| IFish.CurrentTarget == null*/) {//change new escape direction
            currentEscapeTime = 0f;
            SetDefaultState();

            return Vector3.zero;
        }

        if (!isTargetSet) {
            float randomAngle = Random.Range(-ESCAPE_ANGLE_HALF, ESCAPE_ANGLE_HALF);

            Vector3 awayDir = (rb.position - IFish.CurrentTarget.Rb.position); // move away from the target
            awayDir.y = 0f; //only horizontal movement
            Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

            targetPosition = rb.position + rotation * awayDir * RandomTargetPosMultiplier();
            targetPosition.y = rb.position.y; // keep the same height as the fish
            isTargetSet = true;
        }

        return targetPosition - rb.position;
    }

    float RandomTargetPosMultiplier() {
        const float MIN_RANGE = .3f;
        return Random.Range(MIN_RANGE, 1f) * fish.ResponseRadius * WALKING_RADIUS_MULTIPLIER;
    }

    Vector3 IFish.Attack() {
        if (IFish.CurrentTarget == null
            || !IFish.CurrentTarget.gameObject.activeInHierarchy) {//victim is dead or not active
            SetDefaultState();

            return Vector3.zero;
        }

        return IFish.CurrentTarget.Rb.position - rb.position; // move towards
    }

    void Movement(Vector3 dir) {
        dir.y = 0; // only horizontal movement

        float currentDistanceToTarget = dir.magnitude;
        currentDistanceToTarget = (currentDistanceToTarget < (fish.ScaleModifier.z + 1) * fish.JawForwardOffset) ?
            0 : currentDistanceToTarget;//stop moving if enouch close for bite

        switch (state) {
            case EnemyState.Attack:
            if (IFish.CurrentTarget == null
                || !IFish.CurrentTarget.gameObject.activeInHierarchy
                || currentDistanceToTarget > fish.ResponseRadius
                || !fish.IsLegitimateTarget(IFish.CurrentTarget)
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

        direction = dir.normalized;
        float smoother = Mathf.Clamp01(currentDistanceToTarget * .3f) * .95f + .05f; // from .1f to 1f
        ApplyPhysicsMovement(fish.Speed);
    }

    private void LateUpdate() {
        if (fish.IsActive) {
            //if (editableTransform == null) {
            //    Debug.Log(gameObject.name);
            //    return;
            //}

            float delta = (fish.IsPlayer) ? Time.unscaledDeltaTime : Time.deltaTime;
            Quaternion targetLook;

            if (direction.sqrMagnitude > DIRECTION_ROTATE_MAGNITUDE_TRIGGER) {
                targetLook = Quaternion.LookRotation(direction, Vector3.up);
                rotationTransform.rotation = Quaternion.RotateTowards(rotationTransform.rotation, targetLook, fish.RotationSpeed * delta);
            }
            else //standing still
                targetLook = rotationTransform.rotation;

            if (fishAnim != null && fishAnim.IsInited)
                fishAnim.RotateAnimation(rotationTransform.forward, /*targetLook * Vector3.forward*/direction, delta);
        }
    }

    //[SerializeField] bool activatePhysRotation = true;
    protected void ApplyPhysicsMovement(float speed, float speedMultiplier = 1f) {
        // movement
        Vector3 targetPos = direction * speed * speedMultiplier * GetMovingScalerByRotation(rotationTransform.rotation, direction);

        if (targetPos != Vector3.zero) {
            targetPos.y = rb.linearVelocity.y;
            rb.linearVelocity = targetPos;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetDefaultState() {
        SleepPhysics();
        isTargetSet = false; // reset target when changing 
        targetPosition = rb.position; // reset target position
        if (IFish != null)
            IFish.CurrentTarget = null;
        direction = Vector3.zero;
        //Debug.LogWarning("dir recalc disable");
        state = EnemyState.Walking;
        //currentEscapeTime = 0f;
        Vector3 currentForward = rotationTransform.rotation * Vector3.forward;

        if (fishAnim != null && fishAnim.IsInited)
            fishAnim?.RotateAnimation(rotationTransform.forward, currentForward, Time.fixedDeltaTime);//zero rotation
    }

    void IFish.FishProximity(Fish otherFish) {
        if (fish.Level < otherFish.Level)
            state = EnemyState.Escape;
        else if (fish.IsLegitimateTarget(otherFish, true) && fish.Level > GameStaticInfo.PEACEFUL_FISH_LVL) //u not crab
            state = EnemyState.Attack;
        else
            state = EnemyState.Walking; //If the player attacks the fish the same level, only then it will start attacking
    }

    void TryContactWithBehaviour(Collider collider, bool isStayContact) {
        if (collider.attachedRigidbody == null)
            return;

        GameObject victimObj = collider.attachedRigidbody.gameObject;
        //GameObject collisObj = collider.gameObject;

        //if other fish
        if (/*victimObj != null && */transform != victimObj//not self collision
             && GameStaticInfo.TryGetFish(victimObj, out Fish other)     //if collision with another fish
        && (fish.IsPlayer || other == IFish.CurrentTarget) //collision obj is our victim
            && fish.IsLegitimateTarget(other)//victim is weaker
            ) {

            if (isStayContact) {
                //Debug.Log("stay Bites: " + Time.time);
                contactTime += (fish.IsPlayer) ?
                    Time.unscaledDeltaTime : Time.fixedDeltaTime;

                if (contactTime >= fish.DeffaultAttackTime) {//damage each second
                    contactTime = 0f;
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

    Vector3 GetPosiblyCollisionPoint() => rb.position + rotationTransform.rotation * Vector3.forward * fish.JawForwardOffset;

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

        if ((state != EnemyState.Escape /*&& !collision.collider.isTrigger*/)
            && (collision.collider.CompareTag(GameStaticInfo.TAG_POISON_TRIGGER)//if poison object - IMMEDIATELY bypass
            || collisObj.CompareTag(GameStaticInfo.TAG_OBSTACLE)
            //|| (!fish.IsPlayer && collision.collider is SphereCollider && collisObj.CompareTag(GameStaticInfo.TAG_SPAWN_ZONE))
            || collisObj.layer == LayerMask.NameToLayer(GameStaticInfo.LAYER_OBSTACLE))
            /*|| collisObj.layer == LayerMask.NameToLayer(GameStaticInfo.LAYER_CAMERA_AVOID_COLLIDERS)*/)   //fish is stuck on obstacle
            BypassObstacle(collision.GetContact(0).point);
    }


    bool CheckEatAchievements(FishLevel victimLvl) {
        if (victimLvl == GameStaticInfo.PEACEFUL_FISH_LVL)//if eat crab
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_youre_my_friend_now);
        else if (victimLvl == fish.Level)//same lvl
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_cannibalism);
        else if (victimLvl == FishLevel.Mackarel && fish.Level == FishLevel.Moray)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_former_threat_current_snack);

        return true;
    }

    /// <summary>
    /// returns a direction for movement that is randomly opposite to the position of the obstacle
    /// </summary>
    /// <param name="obstaclePos"></param>
    /// <returns></returns>
    void BypassObstacle(Vector3 obstaclePos) {
        SleepPhysics();//only before bypass
        float randomAngle = Random.Range(-BYPASS_ANGLE_HALF, BYPASS_ANGLE_HALF);

        state = EnemyState.Escape;
        isTargetSet = true;
        IFish.CurrentTarget = null;

        Vector3 newDir = (rb.position - obstaclePos); // move away from the target
        newDir.y = 0f; // only horizontal movement
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

        targetPosition = rb.position + rotation * newDir * RandomTargetPosMultiplier();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SleepPhysics() {
        direction = Vector3.zero;
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            //rb.Sleep(); // freeze physics calculations
        }
    }

    private void OnDrawGizmos() {
        if (fish == null || !gameObject.activeInHierarchy || rb == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.position, fish.ResponseRadius);

        if (fish.IsPlayer) {//for player
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(rb.position, fish.ResponseRadius);
            return;
        }

        if (isTargetSet) {
            switch (state) {
                case EnemyState.Attack:
                if (IFish.CurrentTarget != null) {
                    Gizmos.color = Color.red;
                    //Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(rb.position, IFish.CurrentTarget.Rb.position);
                    Gizmos.DrawWireCube(rb.position, Vector3.one * 1.5f);
                }
                return;

                case EnemyState.Escape:
                Gizmos.color = Color.blue;
                break;

                case EnemyState.Walking:
                Gizmos.color = Color.green;
                break;
            }
            if (targetPosition == null)
                return;

            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            Gizmos.DrawLine(rb.position, targetPosition);
        }
    }
}
