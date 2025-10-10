using UnityEngine;


[RequireComponent(typeof(Fish))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControl : MonoBehaviour/*, IPlayerFish*/ {
    //private Fish player;
    ////private IPlayerFish IPlayer;

    ////movement
    //private Rigidbody rb;
    ////private Vector2 moveInput;
    ////private Vector3 moveDir = Vector3.zero;
    ////private Vector3 rotateVelocity = Vector3.zero;
    //////private PlayerActionsInput inputActions;                          
    ////private bool isMoved = false;
    ////private FishAnimManager fishAnim;

    //// collision
    ////private SpawnManager spawnManager;
    //private float contactTime = 0f; // time spent in collision
    ////private const float ROTATE_ANGLE_ANIMATION_START = 10f; // angle of max rotation visualizing animation 


    //public void Init(SpawnManager sm, Fish fishComponent) {
    //    IPlayer = this;
    //    player = fishComponent;//GetComponent<Fish>();
    //    //player.OnFishLvlUp += IPlayer.Grow;
    //    //player.OnFishLvlUp += IPlayer.UpdateModel;
    //    rb = (rb == null) ?
    //        GetComponent<Rigidbody>() : rb;
    //    fishAnim = GetComponent<FishAnimManager>();
    //    //inputActions = new PlayerActionsInput();
    //    //spawnManager = sm;
    //    player.IsActive = true;//player always active and visible, because always viwed on screen
    //    player.IsVisible = true;
    //}

    //void FixedUpdate() {
    //    IPlayer.Movement();
    //}

    //void IPlayerFish.Movement() {
    //    if (!isMoved) //parasyte movement detection
    //        SleepPhysics();

    //    moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;

    //    //Vector3 force = moveDir * player.Speed * Time.fixedDeltaTime;
    //    //rb.AddForce(force, ForceMode.VelocityChange);

    //    Vector3 targetPos = moveDir * player.Speed * Time./*fixedDeltaTime*/unscaledDeltaTime;
    //    if (targetPos != Vector3.zero)
    //        //rb.MovePosition(rb.position + targetPos);

    //        rb.linearVelocity = targetPos;

    //    // rotation in dir of movement
    //    Vector3 newRot = Vector3./*Lerp*/SmoothDamp(transform.forward, moveDir.normalized,/*player.RotationSpeed*/ ref rotateVelocity, Time.unscaledDeltaTime);

    //    //Vector3 desiredForward = moveDir.normalized;
    //    //Quaternion desiredRotation = Quaternion.LookRotation(desiredForward, Vector3.up);
    //    //Quaternion deltaRotation = desiredRotation * Quaternion.Inverse(rb.rotation);
    //    //// Перетворення кватерніона в кутову швидкість
    //    //deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
    //    //Vector3 angularVelocity = rotationAxis * angleInDegrees * Mathf.Deg2Rad / Time.fixedDeltaTime;
    //    //// Обмеження швидкості обертання
    //    //angularVelocity = Vector3.ClampMagnitude(angularVelocity, player.RotationSpeed*25);
    //    //rb.angularVelocity = angularVelocity;


    //    if (newRot != Vector3.zero) {
    //        Quaternion newSmoothRot = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(newRot, Vector3.up), player.RotationSpeed * Time.unscaledDeltaTime);
    //        //float angleDetect_Smooth = Mathf.SmoothDamp(0, 100, ref rotAnimVelocity, Time.fixedDeltaTime);
    //        //rb.angularVelocity = Quaternion.LookRotation(newRot, Vector3.up).eulerAngles;

    //        fishAnim.RotateAnimation(transform.forward, newRot, Time.unscaledDeltaTime, 10f /*angleDetect_Smooth*//*ROTATE_ANGLE_ANIMATION_START*/);
    //        transform.rotation = newSmoothRot;
    //        //rb.rotation = newSmoothRot;
    //        //rb.MoveRotation(newSmoothRot/*Quaternion.LookRotation(newRot, Vector3.up)*/);
    //    }

    //    //transform.forward = Vector3.Lerp(transform.forward, /*targetPos*/moveDir.normalized, player.RotationSpeed);
    //}

    //void IPlayerFish.Grow() {
    //    player.SetLvl((int)player.Level + 1);
    //}

    //void IPlayerFish.UpdateModel() {
    //    spawnManager.DeactivatePrefab(gameObject, false);
    //    player.OnFishLvlUp?
    //}

    //private void OnEnable() {
    //    rb = (rb == null) ?
    //        GetComponent<Rigidbody>() : rb;
    //    SleepPhysics();
    //    JoyAxis.OnAxisChanged += OnMove;
    //    //inputActions.Player.Enable();
    //    //inputActions.Player.Move.performed += OnMove;
    //    //inputActions.Player.Move.canceled += OnMove;
    //}

    //private void OnDisable() {
    //    //inputActions.Player.Move.performed -= OnMove;
    //    //inputActions.Player.Move.canceled -= OnMove;
    //    //inputActions.Player.Disable();
    //    JoyAxis.OnAxisChanged -= OnMove;

    //    //player.OnFishLvlUp -= IPlayer.Grow;
    //    //player.OnFishLvlUp -= IPlayer.UpdateModel;
    //}
    //private void OnDestroy() {
    //    JoyAxis.OnAxisChanged -= OnMove; // for avoiding a warning, since OnDisable is ignored by the compiler
    //}

    //private void OnMove(Vector2 input/*InputAction.CallbackContext context*/) {
    //    //moveInput = context.ReadValue<Vector2>();
    //    //moveInput = new Vector2(moveInput.x, moveInput.y);
    //    //(float)Math.Round(moveInput.x, 2),
    //    //(float)Math.Round(moveInput.y, 2)
    //    //);
    //    moveInput = input;

    //    isMoved = input != Vector2.zero;
    //}

    //private void OnCollisionEnter(Collision collision) {
    //    //if (IsLegimateTarget(out Fish other, collision)) {
    //    //    //if (CheckEatAchievements(other.Level) && !player.Eat(other))
    //    //    //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);//fish is eaten, deactivate and respawn
    //    //    CheckEatAchievements(other.Level);
    //    //    player.Eat(other);
    //    //}
    //    //else {     //any other object collusion
    //    //    if (rb == null)
    //    //        rb = GetComponent<Rigidbody>();

    //    //    SleepPhysics();
    //    //    //Debug.Log("Player boomed");
    //    //}

    //    SleepPhysics();
    //}

    //private void OnCollisionStay(Collision collision) {
    //    //if (IsLegimateTarget(out Fish other, collision)) {

    //    //    contactTime += Time./*deltaTime*/unscaledDeltaTime;

    //    //    if (contactTime >= player.DeffaultAttackTime) {//damage each interval
    //    //        contactTime = 0f;
    //    //        //if (CheckEatAchievements(other.Level) && !player.Eat(other))
    //    //        //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);
    //    //        CheckEatAchievements(other.Level);
    //    //        player.Eat(other);
    //    //    }
    //    //}

    //    SleepPhysics();
    //}

    //private void OnTriggerEnter(Collider other) {
    //    ContactBehaviour(other, false);
    //}

    //private void OnTriggerStay(Collider other) {
    //    ContactBehaviour(other, true);
    //}

    //void ContactBehaviour(Collider collider, bool isStayContact) {
    //    if (!collider.isTrigger && IsLegimateTarget(out Fish other, collider)) {//other not trigger

    //        if (isStayContact) {
    //            contactTime += Time./*deltaTime*/unscaledDeltaTime;

    //            if (contactTime >= player.DeffaultAttackTime) {//damage each interval
    //                contactTime = 0f;
    //                //if (CheckEatAchievements(other.Level) && !player.Eat(other))
    //                //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);
    //                CheckEatAchievements(other.Level);
    //                player.Eat(other);
    //            }
    //        }
    //        else {
    //            //if (CheckEatAchievements(other.Level) && !player.Eat(other))
    //            //spawnManager.DeactivatePrefab(collision.collider.transform.parent.gameObject);//fish is eaten, deactivate and respawn
    //            CheckEatAchievements(other.Level);
    //            player.Eat(other);
    //        }
    //    }

    //}

    //bool CheckEatAchievements(FishLevel victimLvl) {
    //    if (victimLvl == GameStaticInfo.PEACEFUL_FISH_LVL)//if eat crab
    //        GPG_Manager.OnTriggerAchievement(AchievementKey.FriendNow);
    //    else if (victimLvl == player.Level)//same lvl
    //        GPG_Manager.OnTriggerAchievement(AchievementKey.Cannibalism);
    //    else if (victimLvl == FishLevel.Mackarel && player.Level == FishLevel.Moray)
    //        GPG_Manager.OnTriggerAchievement(AchievementKey.ThreatSnack);

    //    return true;
    //}

    //bool IsLegimateTarget(out Fish other, Collider collider) {
    //    other = null;
    //    Transform parent = collider.transform.parent;

    //    return (collider != null && parent != null && parent.gameObject.layer == LayerMask.NameToLayer("Entity")
    //  && GameStaticInfo.TryGetFish(parent.gameObject, out other)//parent.TryGetComponent<Fish>(out other)    //transform.parent bacause collider is a child of fish prefab
    //  /*&& other.Level > GameStaticInfo.UNBEATABLE_FISH_LVL*/ && (other.Level < player.Level
    //  || (other.Level == player.Level && other.Progress < player.Progress)));//other is weaker or equal to player
    //}

    //void SleepPhysics() {
    //    ////isCollided = true;
    //    //moveDir = Vector3.zero;
    //    rb.linearVelocity = Vector3.zero;
    //    rb.angularVelocity = Vector3.zero;
    //    rb.Sleep(); // stop physics calculations

    //    //////rb.freezeRotation = true; // stop rotation

    //    //////rb.linearVelocity = moveDir * player.Speed;

    //    //rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    //    //yield return new WaitForFixedUpdate();
    //    //rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;//onlu Y freeze
    //}

}

