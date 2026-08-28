using UnityEngine;


/// <summary>
/// A manager that controls all child objects (jellyfish)—it's as if each of them has its own motion and effects controller
/// </summary>
public class JellyfishController : MonoBehaviour {
    [SerializeField] Transform waypoints;
    [SerializeField] float speed;
    [SerializeField] float maxRandYOffsetMultiplier;
    private float[] maxRandYOffsetMultipliers;
    private const float MOVEMENT_EPSILON = 0.1f;
    private const float ROTATION_EPSILON = 1e-4f;
    private int[] currentPointId;
    private float[] initYSinOffset;

    private Collider[] jellyColliders;
    private bool isJellyColliderActive;
    [SerializeField] AudioManager audioManager;
    [SerializeField] float coldownTime;
    private float currentColdown;


    void Awake() {
        InitMovementArrays();
    }

    //call from game manager
    void InitMovementArrays() {
        int jellyfishCnt = transform.childCount;
        currentPointId = new int[jellyfishCnt];
        initYSinOffset = new float[jellyfishCnt];
        maxRandYOffsetMultipliers = new float[jellyfishCnt];

        if (IsJellifishHasBehaviour()) {//jelly fish has coldown and has effects etc.
            currentColdown = 0f;//in case if player spawn in jellyfish
            jellyColliders = new Collider[jellyfishCnt];
        }

        for (int i = 0; i < jellyfishCnt; i++) {
            currentPointId[i] = GetClosetPointId(transform.GetChild(i).position);
            initYSinOffset[i] = Random.value * Mathf.PI;
            maxRandYOffsetMultipliers[i] = Random.value * maxRandYOffsetMultiplier;

            if (IsJellifishHasBehaviour()) {
                jellyColliders[i] = transform.GetChild(i).GetComponent<Collider>();
                jellyColliders[i].enabled = false;
            }
        }
    }

    bool IsJellifishHasBehaviour() {
        return coldownTime != 0;
    }

    int GetClosetPointId(Vector3 pos) {
        if (waypoints == null)
            return 0;

        int maxInd = waypoints.childCount - 1;
        int closetId = 0;
        float dist = Vector3.Distance(waypoints.GetChild(closetId).transform.position, pos);

        for (int i = 1; i <= maxInd; i++) {
            float newDist = Vector3.Distance(waypoints.GetChild(i).transform.position, pos);

            if (newDist < dist) {
                dist = newDist;
                closetId = i;
            }
        }

        return closetId;
    }

    void Update() {
        if (!isJellyColliderActive && IsJellifishHasBehaviour())
            PoisonColdownBeh();

        if (waypoints != null)
            MovementJellyfishes();
    }

    void PoisonColdownBeh() {
        currentColdown += DJTime.DeltaTime;

        if (currentColdown > coldownTime) {
            isJellyColliderActive = true;
            SetActivateColliders(isJellyColliderActive);
            currentColdown = 0;
        }
    }

    void MovementJellyfishes() {
        int jellyfishCnt = transform.childCount;

        for (int i = 0; i < jellyfishCnt; i++) {
            Movement(ref currentPointId[i],
                transform.GetChild(i),
                initYSinOffset[i],
                maxRandYOffsetMultipliers[i]);
        }
    }

    void Movement(ref int currentPoint, Transform trans, float initYOffset, float maxYRandOffset) {
        //pos
        Transform target = waypoints.GetChild(currentPoint);
        Vector3 newPos = Vector3.MoveTowards(trans.position, target.position, speed * DJTime.DeltaTime);
        trans.position = new Vector3(newPos.x,
            target.position.y + Mathf.Sin(initYOffset + Time.time) * maxYRandOffset * speed,
            newPos.z);
        //Debug.LogWarning();

        //rot
        Vector3 targPosSub = target.position - trans.position;
        targPosSub.y = 0f;//only hor
        if (targPosSub.sqrMagnitude > ROTATION_EPSILON) {
            Quaternion targRot = Quaternion.LookRotation(targPosSub, Vector3.up);
            trans.rotation = Quaternion.Slerp(trans.rotation, targRot, DJTime.DeltaTime);
        }

        //next point if already arrive
        Vector2 horizPos = new Vector2(trans.position.x, trans.position.z);
        Vector2 horizTargetPos = new Vector2(target.position.x, target.position.z);
        if (Vector2.Distance(horizPos, horizTargetPos) < MOVEMENT_EPSILON)
            currentPoint = (currentPoint + 1) % waypoints.childCount;
    }


    //cooldown and sound only for player fish
    private void OnTriggerStay(Collider other) {
        if (isJellyColliderActive && !other.isTrigger && other.CompareTag(GameStaticInfo.TAG_PLAYER)) {
            isJellyColliderActive = false;
            SetActivateColliders(isJellyColliderActive);
            audioManager.PlayVFX(VFX_clip.Hydra);
        }
    }

    void SetActivateColliders(bool isActive) {
        int jellyfishCnt = transform.childCount;

        for (int i = 0; i < jellyfishCnt; i++)
            jellyColliders[i].enabled = isActive;
    }

    private void OnDrawGizmos() {
        if (waypoints == null)
            return;

        Gizmos.color = Color.cyan;
        Vector3 newPos = waypoints.GetChild(waypoints.childCount - 1).position;

        foreach (Transform t in waypoints) {
            Gizmos.DrawLine(newPos, t.position);
            newPos = t.position;
        }
    }
}
