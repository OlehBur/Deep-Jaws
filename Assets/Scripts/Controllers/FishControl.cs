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
    [SerializeField] Transform editableTransform;

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
    private Vector3 rotateVelocity = Vector3.zero;
    private bool isTargetSet = false; // flag to check if target is set
    private const float TIME_CHANGE_ESCAPE_DIR = 1.5f;
    private float currentEscapeTime = 0f;
    private const float WALKING_RADIUS_MULTIPLIER = 10f;
    private const float MIN_DISTANCE_TO_TARGET = 0.05f;
    private const float TIME_RECALC_WAY = .2f;
    private const float DIRECTION_TRIGGER_MAGNITUDE = .001f;

    //player movement
    private Vector2 moveInput;
    //private Vector3 moveDir = Vector3.zero;
    //private Vector3 newRotation;
    private Vector3 currentForward;
    private bool isMoved = false;
    private const float ROTATION_SMOOTH_TIME = .1f;

    //reaction to other fish
    private Collider[] overlapResults = new Collider[5];
    //private float ResponseRadius = /*enemy.Speed*1.4f*/12f;
    public SpawnManager spawnManager { get; private set; }
    private float contactTime = 0f; // time of contact with other fish


    public void Init(SpawnManager spawnManager/*, Fish fish*/) {
        this.spawnManager = spawnManager;
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

        SleepPhysics();
    }

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
        //if (fish.IsActive) {
        if (fish.IsPlayer)
            PlayerMovement();
        else if (IsFishNeedUpdate())
            EnemyMovementBehaviour();
        //}
    }

    //void LateUpdate() {
    //    if (fish.IsPlayer) 
    //        fishAnim.RotateAnimation(currentForward, newRotation, Time.unscaledDeltaTime);
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsFishNeedUpdate() {
        if (spawnManager != null && spawnManager.PlayerFish != null && rb != null && spawnManager.PlayerFish.Rb != null) {
            float distanceToPlayer = Vector3.Distance(spawnManager.PlayerFish./*transform*/Rb.position, /*transform.position*/rb.position);
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
                direction = targetPosition - /*transform.position*/rb.position; //if the position is set, but we have passed by or something, we continue to reach it

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
        ApplyPhysicsMovement(fish.Speed, fish.RotationSpeed, Time.fixedUnscaledDeltaTime);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float GetMovingScalerByRotation(Quaternion currRotation, Quaternion newRotation, Vector3 moveDir) {
        //if (moveDir == Vector3.zero)
        //    return 0f;

        //const float STOP_MOVING_ANGLE = 180f;

        //Quaternion newLook = Quaternion.LookRotation(moveDir, Vector3.up);
        //return Mathf.Clamp01(1.1f - Mathf.Clamp01(Quaternion.Angle(currRotation, newRotation) / STOP_MOVING_ANGLE)); //.1f to 1f  
        if (moveDir == Vector3.zero)
            return 0f;

        float angle = Quaternion.Angle(currRotation, newRotation);

        // if andf == 180, ret 0.5 (speed = 50%)
        // ç Remap: fr 0..180 deg to 1.0..0.5
        return Mathf.Lerp(1f, .5f, angle / 180f);

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ReactionToOtherFish() {
        int count = Physics.OverlapSphereNonAlloc(/*transform.position*/rb.position, fish.ResponseRadius, overlapResults, GameStaticInfo.ENTITY_LAYER_MASK);
        Fish closestFish = null;
        float dist = fish.ResponseRadius;
        for (int i = 0; i < count; i++) {
            Collider col = overlapResults[i];
            if (col.gameObject.transform.parent.gameObject == gameObject)
                continue;

            float newDist = Vector3.Distance(/*transform.position*/rb.position, col.transform.position);
            if (dist > newDist) {//get closest fish
                dist = newDist;
                //Debug.Log($"{name} see {col.name} on dist {dist}");
                GameStaticInfo.TryGetFish(col.transform.parent.gameObject, out closestFish);
                //closestFish = col.transform.parent.GetComponent<Fish>();//for attack or escape
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

        Vector3 randomDirection = Random.insideUnitSphere * fish.Speed * WALKING_RADIUS_MULTIPLIER;
        randomDirection += /*transform.position*/rb.position;//near pos obj

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, WALKING_RADIUS_MULTIPLIER * fish.ResponseRadius, NavMesh.AllAreas)) { //if point on surf
            targetPosition = hit.position;
            targetPosition.y = /*transform*/rb.position.y; // keep the same height as the fish
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

        return targetPosition - /*transform.position*/rb.position;
    }

    Vector3 IFish.Escape() {
        if (currentEscapeTime >= TIME_CHANGE_ESCAPE_DIR || IFish.CurrentTarget == null) {//change new escape direction
            currentEscapeTime = 0f;
            SetDefaultState();

            return Vector3.zero;
        }

        if (!isTargetSet) {
            const float ESCAPE_ANGLE_HALF = 55f;
            float randomAngle = Random.Range(-ESCAPE_ANGLE_HALF, ESCAPE_ANGLE_HALF);

            Vector3 awayDir = (/*transform.position*/rb.position - IFish.CurrentTarget./*transform*/Rb.position); // move away from the target
            awayDir.y = 0f; //only horizontal movement
            Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

            targetPosition = /*transform.position*/rb.position + rotation * awayDir;
            targetPosition.y = /*transform*/rb.position.y; // keep the same height as the fish
            isTargetSet = true;
        }

        return targetPosition - /*transform.position*/rb.position;
    }

    Vector3 IFish.Attack() {
        if (IFish.CurrentTarget == null
            || !IFish.CurrentTarget.gameObject.activeInHierarchy) {//victim is dead or not active

            SetDefaultState();

            return Vector3.zero;
        }

        return IFish.CurrentTarget./*transform*/Rb.position - /*transform.position*/rb.position; // move towards
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

                if (state == EnemyState.Walking)
                    SleepPhysics();
                return;
            }
            break;
        }

        direction = dir.normalized;
        float smoother = Mathf.Clamp01(currentDistanceToTarget * .3f) * .95f + .05f; // from .1f to 1f
        ApplyPhysicsMovement(fish.Speed, fish.RotationSpeed, Time.fixedDeltaTime, smoother);
    }

    ////test without physics
    //[SerializeField] bool activateTransformRotation = false;
    //private void Update() {
    //    if (activateTransformRotation) {
    //        float angleStep = fish.RotationSpeed * .35f * Time.deltaTime;
    //        Quaternion stepRotation = Quaternion.Euler(0f, angleStep, 0f);
    //        Quaternion testRotation = transform.rotation * stepRotation;

    //        if (fishAnim != null && fishAnim.IsInited) {
    //            Vector3 currentForward = transform.rotation * Vector3.forward;
    //            Vector3 nextForward = testRotation * Vector3.forward;
    //            //fishAnim.RotateAnimation(currentForward, nextForward, Time.deltaTime);
    //        }

    //        transform.rotation = testRotation;
    //    }
    //}

    private void LateUpdate() {
        //if (fish.IsActive) {
        //if (editableTransform == null) {
        //    Debug.Log(gameObject.name);
        //    return;
        //}

        float delta = (fish.IsPlayer) ? Time.unscaledDeltaTime : Time.deltaTime;
        Quaternion targetLook;

        if (direction.sqrMagnitude > DIRECTION_TRIGGER_MAGNITUDE) {
            targetLook = Quaternion.LookRotation(direction, Vector3.up);
            editableTransform.rotation = Quaternion.RotateTowards(editableTransform.rotation, targetLook, fish.RotationSpeed * delta);
        }
        else //standing still
            targetLook = editableTransform.rotation;

        if (fishAnim != null && fishAnim.IsInited)
            fishAnim.RotateAnimation(editableTransform.forward, targetLook * Vector3.forward, delta);
        //}
    }

    //[SerializeField] bool activatePhysRotation = true;
    protected void ApplyPhysicsMovement(float speed, float rotationSpeed, float deltaTime, float speedMultiplier = 1f) {
        //rotation
        Quaternion targetLook =
        (direction.sqrMagnitude > DIRECTION_TRIGGER_MAGNITUDE) ?
            Quaternion.LookRotation(direction, Vector3.up) :
            transform.rotation; //if standing

        //Vector3 currentForward = rb.rotation * Vector3.forward;
        //Vector3 targetForward = targetLook * Vector3.forward;

        //float maxDegreesPerDelta = rotationSpeed * deltaTime;
        //Quaternion newSmoothRot = Quaternion.RotateTowards(rb.rotation, targetLook, maxDegreesPerDelta);

        //if (fishAnim != null && fishAnim.IsInited)
        //    fishAnim.RotateAnimation(currentForward, targetForward, deltaTime);

        //rb.MoveRotation(newSmoothRot);

        // movement
        Vector3 targetPos = direction * speed * speedMultiplier * GetMovingScalerByRotation(rb.rotation, targetLook, direction);

        if (targetPos != Vector3.zero) {
            targetPos.y = rb.linearVelocity.y;
            rb.linearVelocity = targetPos;
        }

        //// === TEST MODE: ignore input> just rotate around ===
        //if (activatePhysRotation) {
        //    float angleStep = rotationSpeed * .35f * deltaTime;
        //    Quaternion stepRotation = Quaternion.Euler(0f, angleStep, 0f);        //"rot angleStep with Y"
        //    Quaternion testRotation = rb.rotation * stepRotation;

        //    if (fishAnim != null && fishAnim.IsInited) {
        //        Vector3 currentForward = rb.rotation * Vector3.forward;
        //        Vector3 nextForward = testRotation * Vector3.forward;
        //        //fishAnim.RotateAnimation(currentForward, nextForward, deltaTime);
        //    }
        //    rb.MoveRotation(testRotation);
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetDefaultState() {
        isTargetSet = false; // reset target when changing 
        targetPosition = /*transform.position*/rb.position; // reset target position
        if (IFish != null)
            IFish.CurrentTarget = null;
        direction = Vector3.zero;
        state = EnemyState.Walking;
        currentEscapeTime = 0f;
        Vector3 currentForward = rb.rotation * Vector3.forward;

        if (fishAnim != null && fishAnim.IsInited)
            fishAnim?.RotateAnimation(/*transform.forward*/currentForward, /*transform.forward*/currentForward, Time.fixedDeltaTime);//zero rotation
    }

    void IFish.FishProximity(Fish otherFish) {
        if (fish.Level < otherFish.Level) {
            state = EnemyState.Escape;
        }
        else if (fish.IsLegitimateTarget(otherFish, true) && fish.Level != GameStaticInfo.PEACEFUL_FISH_LVL) {//not crab
            state = EnemyState.Attack;
        }
        else {
            state = EnemyState.Walking; //If the player attacks the fish the same level, only then it will start attacking
        }
    }

    // while fish is in contact with another fish, if it is weaker, it will be eaten
    private void OnCollisionEnter(Collision collision) {
        GameObject collisObj = collision.collider.gameObject;

        if (collisObj.layer == LayerMask.NameToLayer("Obstacle")
            || collisObj.layer == LayerMask.NameToLayer("AvoidDynamicCameraColliders")) //fish is stuck on obstacle
            BypassObstacle(collision.GetContact(0).point);

        SleepPhysics();
    }

    // while fish is in contact with another fish, it will deal damage to it
    private void OnCollisionStay(Collision collision) {
        GameObject collisObj = collision.collider.gameObject;

        if (collisObj.layer == LayerMask.NameToLayer("Obstacle")
            || collisObj.layer == LayerMask.NameToLayer("AvoidDynamicCameraColliders")) //fish is stuck on obstacle
            BypassObstacle(collision.GetContact(0).point/*collider.transform.position*/);

        SleepPhysics();
    }


    void TryContactWithBehaviour(Collider collider, bool isStayContact) {
        Rigidbody victimRb = collider.attachedRigidbody;
        GameObject collisObj = collider.gameObject;

        //if other fish
        if (victimRb != null && transform != victimRb//not self collision
             && GameStaticInfo.TryGetFish(victimRb.gameObject, out Fish other)     //if collision with another fish
        && (fish.IsPlayer || other == IFish.CurrentTarget) //collision obj is our victim
            && fish.IsLegitimateTarget(other)//victim is weaker
            ) {

            if (isStayContact) {
                contactTime += (fish.IsPlayer) ?
                    Time.unscaledDeltaTime : Time.fixedDeltaTime;

                if (contactTime >= fish.DeffaultAttackTime) {//damage each second
                    contactTime = 0f;
                    fish.Eat(other);

                    if (fish.IsPlayer)
                        CheckEatAchievements(other.Level);
                }
            }
            else {
                if (fish.IsPlayer)
                    CheckEatAchievements(other.Level);

                if (!fish.Eat(other)) {//on enter
                    SetDefaultState();
                }
            }
            //if obstacle spawn zone (only sphere collider)
            if (!fish.IsPlayer && collider is SphereCollider && collisObj.layer == LayerMask.NameToLayer("AvoidDynamicCameraColliders"))
                BypassObstacle(collider.ClosestPoint(/*transform.position*/rb.position));

        }
    }


    //private void OnCollisionExit(Collision collision) {
    //    SleepPhysics();
    //}

    private void OnTriggerEnter(Collider other) {//some invisible obstacles like player spawn zone (only sphere collider)                                                 
        if (other.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD))
            SleepPhysics();
        else if (fish.Level != GameStaticInfo.PEACEFUL_FISH_LVL && !other.isTrigger)
            TryContactWithBehaviour(other, false);
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD))
            SleepPhysics();
        else if (fish.Level != GameStaticInfo.PEACEFUL_FISH_LVL && !other.isTrigger)
            TryContactWithBehaviour(other, true);
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
        const float BYPASS_ANGLE_HALF = 90f;
        float randomAngle = Random.Range(-BYPASS_ANGLE_HALF, BYPASS_ANGLE_HALF);

        state = EnemyState.Walking;
        isTargetSet = true;
        IFish.CurrentTarget = null;

        Vector3 newDir = (/*transform.position*/rb.position - obstaclePos); // move away from the target
        newDir.y = 0f; // only horizontal movement
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

        targetPosition = /*transform.position*/rb.position + rotation * newDir;
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
        if (fish == null || !gameObject.activeInHierarchy)
            return;

        if (fish.IsPlayer) {//for player
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(rb.position, fish.ResponseRadius);
            return;
        }

        //for anemies stages
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.position, fish.ResponseRadius);

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
            Gizmos.DrawLine(/*transform.position*/rb.position, targetPosition);
        }
    }
}
