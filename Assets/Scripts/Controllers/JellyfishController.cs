using UnityEngine;


/// <summary>
/// Менеджер що контролює всі дочірні об'єкти (медузи) наче в кожного з них є контролер руху та ефектів
/// </summary>
public class JellyfishController : MonoBehaviour {
    //[SerializeField] float heightOffset;
    //[SerializeField] float widthOffset;
    //[SerializeField] float lenthOffset;
    //private Vector3 centerPos;

    [SerializeField] Transform waypoints;
    [SerializeField] float speed;
    [SerializeField] float maxRandYOffsetMultiplier;
    private float[] maxRandYOffsetMultipliers;
    //private float localWorldCenterY;
    private const float MOVEMENT_EPSILON = 0.1f;
    private const float ROTATION_EPSILON = 1e-4f;
    private int[] currentPointId;
    private float[] initYSinOffset;

    private Collider[] jellyColliders;
    private bool isJellyColliderActive;
    [SerializeField] AudioSource jellyAudioSource;
    [SerializeField] AudioClip jellyBiteClip;
    [SerializeField] float coldownTime;
    private float currentColdown;


    void Awake() {
        ////centerPos = new Vector3(transform.position.x, transform.position.y, transform.position.z - lenthOffset);

        //currentPointId = GetClosetPointId();
        //initYSinOffset = Random.value * Mathf.PI;
        //maxRandYOffsetMultiplier = Random.value * maxRandYOffsetMultiplier;
        //currentColdown = -coldownTime;

        ////localWorldCenterY = transform.TransformPoint(Vector3.zero).y;

        //////init coldown time during loading map - meduses doesnt bite in case if player spawned so close
        ////if (jellyCollider != null)
        ////    jellyCollider.enabled = false;

        InitMovementArrays();
 }

    //call from game manager
    /*public*/
    void InitMovementArrays() {
        int jellyfishCnt = transform.childCount;
        currentPointId = new int[jellyfishCnt];
        initYSinOffset = new float[jellyfishCnt];
        maxRandYOffsetMultipliers = new float[jellyfishCnt];

        if (IsJellifishHasBehaviour()) {//jelly fish has coldown and has effects etc.
            currentColdown = -coldownTime;//in case if player spawn in jellyfish
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
        if (!isJellyColliderActive && IsJellifishHasBehaviour()/*jellyCollider != null && !jellyCollider.enabled*/)
            PoisonColdownBeh();

        if (waypoints != null)
            MovementJellyfishes();
    }

    void PoisonColdownBeh() {
        currentColdown += Time.deltaTime;

        if (currentColdown > coldownTime) {
            //jellyCollider.enabled = true;
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
        Vector3 newPos = Vector3.MoveTowards(trans.position, target.position, speed * Time.deltaTime);
        trans.position = new Vector3(newPos.x,
            /*localWorldCenterY*/target.position.y + Mathf.Sin(initYOffset + Time.time) * maxYRandOffset * speed,
            newPos.z);
        //Debug.LogWarning();

        //rot
        Vector3 targPosSub = target.position - trans.position;
        targPosSub.y = 0f;//only hor
        if (targPosSub.sqrMagnitude > ROTATION_EPSILON) {
            Quaternion targRot = Quaternion.LookRotation(targPosSub, Vector3.up);
            trans.rotation = Quaternion.Slerp(trans.rotation, targRot, Time.deltaTime);
        }

        //next point if already arrive
        Vector2 horizPos = new Vector2(trans.position.x, trans.position.z);
        Vector2 horizTargetPos = new Vector2(target.position.x, target.position.z);
        if (Vector2.Distance(horizPos, horizTargetPos) < MOVEMENT_EPSILON)
            currentPoint = (currentPoint + 1) % waypoints.childCount;
    }


    //cooldown and sound only for player fish
    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player") && isJellyColliderActive) {
            //jellyCollider.enabled = false;
            isJellyColliderActive = false;
            SetActivateColliders(isJellyColliderActive);
            jellyAudioSource.PlayOneShot(jellyBiteClip);
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

        //Gizmos.DrawSphere(waypoints.GetChild(currentPointId).position, 5f);


        //Vector3 prevPos = centerPos;
        //prevPos.x += widthOffset;

        //for (int i = 1; i <= 25; i++) {
        //    float angle = (i * Mathf.PI * 2f) / 25f;
        //    float x = Mathf.Cos(angle) * widthOffset;
        //    float z = Mathf.Sin(angle) * lenthOffset;
        //    newPos = transform.position + new Vector3(x, 0, z);
        //    Gizmos.DrawLine(prevPos, newPos);
        //    prevPos = newPos;
        //}
    }
}
