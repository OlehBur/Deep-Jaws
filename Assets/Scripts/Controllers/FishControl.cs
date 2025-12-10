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
    [SerializeField] FishAnimManager fishAnim;

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
    private Fish fish;
    private Rigidbody rb;
    Vector3 direction = Vector3.zero;
    private Vector3 targetPosition;
    private Vector3 rotateVelocity = Vector3.zero;
    private bool isTargetSet = false; // flag to check if target is set
    private const float TIME_CHANGE_ESCAPE_DIR = 1.5f;
    private float currentEscapeTime = 0f;
    private const float WALKING_RADIUS_MULTIPLIER = .1f;
    private const float MIN_DISTANCE_TO_TARGET = 0.05f;
    private const float TIME_RECALC_WAY = .2f;

    //player movement
    private Vector2 moveInput;
    private Vector3 moveDir = Vector3.zero;
    private bool isMoved = false;
    private const float ROTATION_TIME_MULTIPLIER = .005f;

    //reaction to other fish
    private Collider[] overlapResults = new Collider[5];
    //private float ResponseRadius = /*enemy.Speed*1.4f*/12f;
    public SpawnManager spawnManager { get; private set; }
    private float contactTime = 0f; // time of contact with other fish


    public void Init(SpawnManager spawnManager, Fish fish) {
        this.spawnManager = spawnManager;
        IFish = this;
        this.fish = fish;
        IFish.CurrentTarget = null; // Reset current target when enabled

        rb = (rb == null) ?
            GetComponent<Rigidbody>() : rb;
        rb.constraints = GameStaticInfo.DEFAULT_FISH_CONSTRAINS_RIQIDBODY;//in case it was stunned before
        rb.linearVelocity = Vector3.zero; // Reset angular velocity to prevent unwanted rotation
        fishAnim = GetComponent<FishAnimManager>();

        CancelInvoke();
        SetDefaultState();

        if (fish.IsPlayer) {
            fish.IsActive = true;//player always active and visible, because always viwed on screen
            fish.IsVisible = true;
            JoyAxis.Axis = Vector2.zero;
        }

        SleepPhysics();
        JoyAxis.OnAxisChanged += OnMove;
    }

    //void Awake() {
    //    IEnemy = this;
    //    rb = GetComponent<Rigidbody>();
    //    fishLayerMask = LayerMask.GetMask("Entity");
    //}

    //void Start() {
    //    SetDefaultState(); // Set initial state after full initialization
    //}

    //private void OnEnable() {

    //    //Invoke(nameof(SetDefaultState), ³dleTime); //afrer respawn or on start
    //    //                                           //SetTargetMovement();
    //    //if (fish != null) {

    //    //}
    //}

    private void OnDisable() {
        // Cancel all delayed calls when disabled
        CancelInvoke();
        JoyAxis.OnAxisChanged -= OnMove;
        if (IFish != null)
            IFish.CurrentTarget = null;
    }

    private void OnDestroy() {
        JoyAxis.OnAxisChanged -= OnMove; // for avoiding a warning, since OnDisable is ignored by the compiler
    }

    private void OnMove(Vector2 input/*InputAction.CallbackContext context*/) {
        moveInput = input;

        isMoved = input != Vector2.zero;
    }

    void FixedUpdate() {
        if (fish.IsPlayer)
            PlayerMovement();
        else if (IsFishNeedUpdate())
            EnemyMovementBehaviour();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsFishNeedUpdate() {
        if (spawnManager != null && spawnManager.PlayerFish != null) {
            float distanceToPlayer = Vector3.Distance(spawnManager.PlayerFish.transform.position, transform.position);
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
        switch (state) {
            case EnemyState.Walking:
                if (!isTargetSet)
                    direction = IFish.Walking();
                else
                    direction = targetPosition - transform.position; //if the position is set, but we have passed by or something, we continue to reach it

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void PlayerMovement() {
        const float STOP_MOVING_ANGLE = 50f;

        if (!isMoved) //parasyte movement detection
            SleepPhysics();

        // rotation in dir of movement
        Vector3 newRot = Vector3./*Lerp*/SmoothDamp(
            transform.forward, moveDir.normalized,/*player.RotationSpeed*/ ref rotateVelocity,
            fish.RotationSpeed * ROTATION_TIME_MULTIPLIER, Mathf.Infinity, Time.unscaledDeltaTime);
        Quaternion newLook = Quaternion.LookRotation(newRot, Vector3.up);

        if (newRot != Vector3.zero) {
            Quaternion newSmoothRot = Quaternion.Slerp(rb.rotation, newLook, fish.RotationSpeed * Time.unscaledDeltaTime);

            fishAnim.RotateAnimation(transform.forward, newRot, Time.unscaledDeltaTime, .040f /*angleDetect_Smooth*//*ROTATE_ANGLE_ANIMATION_START*/);
            transform.rotation = newSmoothRot;
        }

        //movement
        float movingScalerByRotation = Mathf.Clamp01(1.1f - Mathf.Clamp01(Quaternion.Angle(transform.rotation, newLook) / STOP_MOVING_ANGLE)); //.1f to 1f  
        //Debug.Log(movingScalerByRotation);
        moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 targetPos = moveDir * fish.Speed * Time./*fixedDeltaTime*/unscaledDeltaTime * movingScalerByRotation;

        if (targetPos != Vector3.zero)
            rb.linearVelocity = targetPos;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ReactionToOtherFish() {
        int count = Physics.OverlapSphereNonAlloc(transform.position, fish.ResponseRadius, overlapResults, GameStaticInfo.ENTITY_LAYER_MASK);
        Fish closestFish = null;
        float dist = fish.ResponseRadius;
        for (int i = 0; i < count; i++) {
            Collider col = overlapResults[i];
            if (col.gameObject.transform.parent.gameObject == gameObject)
                continue;

            float newDist = Vector3.Distance(transform.position, col.transform.position);
            if (dist > newDist) {//get closest fish
                dist = newDist;
                //Debug.Log($"{name} see {col.name} on dist {dist}");
                GameStaticInfo.TryGetFish(col.transform.parent.gameObject, out closestFish);
                //closestFish = col.transform.parent.GetComponent<Fish>();//for attack or escape
            }
        }

        //if (IEnemy.CurrentTarget == null)
        //    return;
        //IEnemy.FishProximity(IEnemy.CurrentTarget);

        //update target only if fish found
        if (closestFish != null) {
            IFish.CurrentTarget = closestFish;
            IFish.FishProximity(closestFish);
        }
        else if (IFish?.CurrentTarget == null) {
            // if target is lost, return to walking
            //IFish.CurrentTarget = null;
            if (state != EnemyState.Walking)
                SetDefaultState();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetTargetMovement() {//set target position on surf for movement
        if (isTargetSet || fish == null)
            return;

        Vector3 randomDirection = Random.insideUnitSphere * WALKING_RADIUS_MULTIPLIER * fish.Speed;
        randomDirection += transform.position;//near pos obj
        //randomDirection.y = 0f;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, WALKING_RADIUS_MULTIPLIER * fish.ResponseRadius, NavMesh.AllAreas)) { //if point on surf
            targetPosition = hit.position;
            targetPosition.y = transform.position.y; // keep the same height as the fish
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

        return targetPosition - transform.position;
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

            Vector3 awayDir = (transform.position - IFish.CurrentTarget.transform.position); // move away from the target
            awayDir.y = 0f; //only horizontal movement
            Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

            targetPosition = transform.position + rotation * awayDir;
            targetPosition.y = transform.position.y; // keep the same height as the fish
            isTargetSet = true;
        }

        return targetPosition - transform.position;
    }

    Vector3 IFish.Attack() {
        if (IFish.CurrentTarget == null
            || !IFish.CurrentTarget.gameObject.activeInHierarchy) {//victim is dead or not active

            SetDefaultState();

            return Vector3.zero;
        }

        return IFish.CurrentTarget.transform.position - transform.position; // move towards
    }

    void Movement(Vector3 dir) {
        dir.y = 0; // only horizontal movement

        float currentDistance = dir.magnitude;
        currentDistance = (currentDistance < (fish.ScaleModifier.z + 1) * fish.JawForwardOffset) ?
            0 : currentDistance;//stop moving if enouch close for bite

        switch (state) {
            case EnemyState.Attack:
                if (IFish.CurrentTarget == null
                    || !IFish.CurrentTarget.gameObject.activeInHierarchy
                    || currentDistance > fish.ResponseRadius
                    || !fish.IsLegitimateTarget(IFish.CurrentTarget)
                    //|| IFish.CurrentTarget.Level > fish.Level
                    //|| (IFish.CurrentTarget.Level == fish.Level && IFish.CurrentTarget.CompareTag("Player") && fish.Level < GameStaticInfo.SHARK_INDEX_BARRIER)
                    //|| (IFish.CurrentTarget.Level == fish.Level && IFish.CurrentTarget.Progress >= fish.Progress)
                    ) {//if the target is lost

                    SetDefaultState();
                    return;
                }
                break;

            case EnemyState.Walking:
            case EnemyState.Escape:
            default://logic of changing state when the target is reached
                if (currentDistance < MIN_DISTANCE_TO_TARGET) {//magnitude - length of vect
                    SetDefaultState();

                    if (state == EnemyState.Walking)
                        SleepPhysics();
                    return;
                }
                break;
        }

        float smoother = Mathf.Clamp01(currentDistance * .3f) * .95f + .05f;//from .1f to 1f
        Vector3 move = dir.normalized * fish.Speed * Time.fixedDeltaTime * smoother;
        if (move != Vector3.zero)
            //rb.MovePosition(rb.position + move);
            rb.linearVelocity = move;
        else
            SleepPhysics();

        //Vector3 newRot = Vector3./*Lerp*/SmoothDamp(transform.forward, moveDir.normalized, ref rotateVelocity, Time.unscaledDeltaTime);

        // smooth rotation
        Vector3 rot = Vector3./*Lerp*/SmoothDamp(transform.forward, move.normalized, /*enemy.RotationSpeed*/ref rotateVelocity, Time.deltaTime);

        if (rot != Vector3.zero) {
            Quaternion newSmoothRot = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(rot, Vector3.up), fish.RotationSpeed * Time.deltaTime);
            //float angleDetect_Smooth = Mathf.SmoothDamp(20, 0, ref rotAnimVelocity, Time.fixedDeltaTime);

            //rb.MoveRotation(Quaternion.LookRotation(rot, Vector3.up));
            fishAnim?.RotateAnimation(transform.forward, rot, Time.deltaTime/*, angleDetect_Smooth*/);//call fish animation manager to rotate the fish model
            //rb.MoveRotation(newSmoothRot);
            transform.rotation = newSmoothRot;
        }
        //transform.forward = Vector3.Lerp(transform.forward, move.normalized, enemy.RotationSpeed); //also good solut 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetDefaultState() {
        isTargetSet = false; // reset target when changing 
        targetPosition = transform.position; // reset target position
        IFish.CurrentTarget = null;
        direction = Vector3.zero;
        state = EnemyState.Walking;
        currentEscapeTime = 0f;
        fishAnim?.RotateAnimation(transform.forward, transform.forward, Time.fixedDeltaTime);//zero rotation
        //SleepPhysics();
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
        //Transform parent = collision.collider.transform.parent;
        GameObject collisObj = collision.collider.gameObject;

        //if (parent != null && GameStaticInfo.TryGetFish(parent.gameObject, out Fish other)/*parent.TryGetComponent<Fish>(out Fish other)*/        //if collision with another fish
        //&& other == IEnemy.CurrentTarget //collision obj is our victim
        //    && enemy.IsLegitimateTarget(other)//victim is weaker
        //    ) {
        //    if (!enemy.Eat(other) && collisObj != null) {
        //        SetDefaultState();
        //        //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);//fish is eaten, deactivate and respawn
        //    }
        //}

        if (collisObj.layer == LayerMask.NameToLayer("Obstacle")
            || collisObj.layer == LayerMask.NameToLayer("AvoidDynamicCameraColliders")) //fish is stuck on obstacle
            BypassObstacle(collision.GetContact(0).point/*collision.collider.transform.position*/);

        SleepPhysics();
    }

    // while fish is in contact with another fish, it will deal damage to it
    private void OnCollisionStay(Collision collision) {
        //Transform parent = collision.collider.transform.parent;

        GameObject collisObj = collision.collider.gameObject;

        //if (parent != null && GameStaticInfo.TryGetFish(parent.gameObject, out Fish other)/* parent.TryGetComponent<Fish>(out Fish other)*/
        //        && other == IEnemy.CurrentTarget //collision obj is our victim
        //        && enemy.IsLegitimateTarget(other)//victim is weaker
        //        ) {

        //    contactTime += Time.fixedDeltaTime;

        //    //float defaultAttackFrequency = 1f / enemy.Level; // default attack (fish just stand) frequency 
        //    if (contactTime >= enemy.DeffaultAttackTime) {//damage each second
        //        contactTime = 0f;
        //        enemy.Eat(other);
        //        //if (!enemy.Eat(other) && collision.collider.gameObject != null)
        //        //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);
        //    }
        //}
        //else
        if (collisObj.layer == LayerMask.NameToLayer("Obstacle")
            || collisObj.layer == LayerMask.NameToLayer("AvoidDynamicCameraColliders")) //fish is stuck on obstacle
            BypassObstacle(collision.GetContact(0).point/*collider.transform.position*/);

        SleepPhysics();
    }


    void TryContactWithBehaviour(Collider collider, bool isStayContact) {
        //Transform parent = collider.transform.parent;
        Rigidbody victimRb = collider.attachedRigidbody;
        GameObject collisObj = collider.gameObject;

        //if other fish
        if (victimRb != null && transform != victimRb//not self collision
             && GameStaticInfo.TryGetFish(victimRb.gameObject, out Fish other)/*parent.TryGetComponent<Fish>(out Fish other)*/        //if collision with another fish
        && (fish.IsPlayer || other == IFish.CurrentTarget) //collision obj is our victim
            && fish.IsLegitimateTarget(other)//victim is weaker
            ) {

            if (isStayContact) {
                contactTime += (fish.IsPlayer) ? Time.unscaledDeltaTime : Time.fixedDeltaTime;

                //float defaultAttackFrequency = 1f / enemy.Level; // default attack (fish just stand) frequency 
                if (contactTime >= fish.DeffaultAttackTime) {//damage each second
                    contactTime = 0f;
                    fish.Eat(other);
                    //if (!enemy.Eat(other) && collision.collider.gameObject != null)
                    //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);

                    if (fish.IsPlayer)
                        CheckEatAchievements(other.Level);
                }
            }
            else {
                if (fish.IsPlayer)
                    CheckEatAchievements(other.Level);

                if (!fish.Eat(other)/* && collisObj != null*/) {//on enter
                    SetDefaultState();
                    //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);//fish is eaten, deactivate and respawn
                }
            }

            //if obstacle spawn zone (only sphere collider)
            if (!fish.IsPlayer && collider is SphereCollider && collisObj.layer == LayerMask.NameToLayer("AvoidDynamicCameraColliders"))
                BypassObstacle(collider.ClosestPoint(transform.position));

        }
    }


    private void OnCollisionExit(Collision collision) {
        SleepPhysics();
    }

    private void OnTriggerEnter(Collider other) {//some invisible obstacles like player spawn zone (only sphere collider)
                                                 //if (other is SphereCollider /*MeshCollider meshC && meshC.convex*/ &&
                                                 //   (other.gameObject.layer == LayerMask.NameToLayer("Obstacle")//fish is stuck on obstacle
                                                 //    || other.gameObject.layer == LayerMask.NameToLayer("AvoidDynamicCameraColliders")))
                                                 //    BypassObstacle(other.ClosestPoint(transform.position));
        if (other.tag == "Shield")
            SleepPhysics();
        else if (fish.Level != GameStaticInfo.PEACEFUL_FISH_LVL && !other.isTrigger)
            TryContactWithBehaviour(other, false);
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "Shield")
            SleepPhysics();
        else if (fish.Level != GameStaticInfo.PEACEFUL_FISH_LVL && !other.isTrigger)
            TryContactWithBehaviour(other, true);
    }
    bool CheckEatAchievements(FishLevel victimLvl) {
        if (victimLvl == GameStaticInfo.PEACEFUL_FISH_LVL)//if eat crab
            GPG_Manager.OnTriggerAchievement(AchievementKey.FriendNow);
        else if (victimLvl == fish.Level)//same lvl
            GPG_Manager.OnTriggerAchievement(AchievementKey.Cannibalism);
        else if (victimLvl == FishLevel.Mackarel && fish.Level == FishLevel.Moray)
            GPG_Manager.OnTriggerAchievement(AchievementKey.ThreatSnack);

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

        Vector3 newDir = (transform.position - obstaclePos); // move away from the target
        newDir.y = 0f; // only horizontal movement
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

        targetPosition = transform.position + rotation * newDir;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SleepPhysics() {
        direction = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep(); // freeze physics calculations
    }

    private void OnDrawGizmos() {
        if (fish == null || !gameObject.activeInHierarchy)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fish.ResponseRadius);

        if (isTargetSet) {
            switch (state) {
                case EnemyState.Attack:
                    if (IFish.CurrentTarget != null) {
                        Gizmos.color = Color.red;
                        //Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(transform.position, IFish.CurrentTarget.transform.position);
                        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);
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
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
