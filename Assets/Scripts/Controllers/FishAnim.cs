using UnityEngine;


[RequireComponent(typeof(Fish))]
public class FishAnim : MonoBehaviour {
    [SerializeField] Fish fish;
    [SerializeField] float smoothRotateSpeed = 5f;
    [SerializeField] float angleRotDetect = 12f;
    //[SerializeField] Rigidbody rb;
    [SerializeField] Animator animator; // animator for fish animations (crab has no animator and dartfish)
    //[SerializeField] AnimationClip idleClip;
    [SerializeField] Renderer fishSkinnedMesh; //To get Mesh material
    [SerializeField] Transform editableTransform;
    [SerializeField] Vector3 biteParticleLocPos;
    [SerializeField] float bubbleMaxXoffset = 0; //change the width of the bubble in the rotation (to make the ends of the tail out)
    [SerializeField] bool isUseRotatingShader = false;

    public Animator FishAnimator { get { return animator; } private set { animator = value; } }
    public Renderer FishSkinnedMesh => fishSkinnedMesh;
    public bool IsInited { get; private set; } = false;

    private FishAdditionalCompManager fishAddCompMngr;
    private AudioManager audioManager;
    private MaterialPropertyBlock matPropBlock;
    private float matDefSprintSpeed; // default sprint speed in the material
    //private Material fishMat;
    private float maxShapeRotationVal;
    private EffectManager effectManager;

    private const float BUBBLE_INIT_X_OFFSET = 0;
    private const string BITE_TRIGGER_NAME = "bite"; // name of the trigger for bite animation
    private const float SPRINT_SPEED_MULTIPLIER = 2f; // multiplier for sprint speed


    public void Init(AudioManager audioManager, EffectManager effectManager, FishAdditionalCompManager fishAddCompMngr/*Animator animator*/) {
        this.audioManager = audioManager;
        this.effectManager = effectManager;
        this.fishAddCompMngr = fishAddCompMngr;
        if (fish.IsPlayer)
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        IsInited = true;
    }

    void OnEnable() {
        //init set prop block
        if (fishSkinnedMesh == null) {
            matDefSprintSpeed = 1f;
        }
        else {
            InitPropBlock(ref matPropBlock, ref fishSkinnedMesh);
            matDefSprintSpeed = SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_Frequency", true); // get the default sprint speed from the material
        }

        if (!isUseRotatingShader)
            return;

        fish.OnFishEat += FishEatAnim;/* Debug.LogWarning("eat fish event");*/

        maxShapeRotationVal = (fishSkinnedMesh == null) ?
            0f : SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_MaxRotationValue", true);

        if (animator != null) {
            animator.ResetTrigger(BITE_TRIGGER_NAME);
            animator.Rebind();  // reset all to def stage (fix open mouth death)

            ////forse attath data from first keyframe to bones
            //if (idleClip != null)
            //    idleClip.SampleAnimation(gameObject, 0f);

            // restart animator            
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);//force apply fisrt keyframe of anim
        }
    }

    private static void InitPropBlock(ref MaterialPropertyBlock matPropBlock, ref Renderer skinnedMesh) {
        matPropBlock = new MaterialPropertyBlock();
        SetGetMaterialWithPropBlock(ref skinnedMesh, ref matPropBlock, "_Frequency", false, skinnedMesh.sharedMaterial.GetFloat("_Frequency"));
        SetGetMaterialWithPropBlock(ref skinnedMesh, ref matPropBlock, "_MaxRotationValue", false, skinnedMesh.sharedMaterial.GetFloat("_MaxRotationValue"));
        SetGetMaterialWithPropBlock(ref skinnedMesh, ref matPropBlock, "_RotationRad", false, skinnedMesh.sharedMaterial.GetFloat("_RotationRad"));
    }

    public static float SetGetMaterialWithPropBlock(ref Renderer renderer, ref MaterialPropertyBlock mapBlock, string paramName, bool isGet, float value = 0) {
        //Debug.LogWarning(renderer + " " + mapBlock);
        renderer.GetPropertyBlock(mapBlock);

        if (isGet)
            return mapBlock.GetFloat(paramName);
        else {//set
            mapBlock.SetFloat(paramName, value);
            renderer.SetPropertyBlock(mapBlock);
        }
        return -1;
    }

    /// <summary>
    /// enable sprint animation boost if the fish is sprinting, itherwise set the default speed
    /// </summary>
    public void SprintAnimation(bool isSprinting = true) {
        if (isSprinting)
            SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_Frequency", false, matDefSprintSpeed * SPRINT_SPEED_MULTIPLIER);
        else
            SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_Frequency", false, matDefSprintSpeed);
    }

    //[SerializeField] float angleDetct = 13f;
    //[SerializeField] float delta0 = 0.4f;
    public void RotateAnimation(Vector3 fromDir, Vector3 toDir, float deltaTime) {
        if (!isUseRotatingShader)
            return;

        //float ANGLE_DETECTION = 12f/*fish.AngleDetect*//*32f - (float)fish.Level * ANGLE_MIN_MULTIPLIER*/; // decrease the detection angle with increasing fish level
        //const float SMOOTHING_SPEED = 5f;
        //Quaternion currentRotation = rb.rotation;

        // ang betw
        //float angle = Vector3.SignedAngle(fromDir, toDir, Vector3.up);
        //float currentAngleProgress = SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_RotationRad", true);
        //float t = Mathf.Clamp(angle / angleRotDetect, -1f, 1f);
        //float boostedT = Mathf.Sign(t) * Mathf.Sqrt(Mathf.Abs(t));
        //float targetAngleProgress = boostedT * maxShapeRotationVal;

        //if (fish.IsPlayer && angle != 0f)
        //    Debug.Log($"angle: {angle} | current: {currentAngleProgress} | target: {targetAngleProgress}");

        //float smoothT = 1f - Mathf.Exp(-smoothRotateSpeed * deltaTime);
        //float angleProgress = Mathf.Lerp(currentAngleProgress, targetAngleProgress, smoothT);
        float angle = Vector3.SignedAngle(fromDir, toDir, Vector3.up);
        float currentAngleProgress = SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_RotationRad", true);
        float angleProgress = Mathf.LerpUnclamped(0, maxShapeRotationVal, angle / angleRotDetect); // progress from -1 to 1 (LerpUnclamped allows to go beyond the range)
        angleProgress = Mathf.Clamp(angleProgress, -maxShapeRotationVal, maxShapeRotationVal); // clamp the value to avoid exceeding the max rotation value     

        //if (fish.IsPlayer && angle != .0f)
        //    Debug.Log("angle: " + angle + " currentAngProgress: " + currentAngleProgress + " angleProgress:" + angleProgress);                                                                                                                              //angleProgress, ref rotAnimVelocity, enemyAnimDeltaMultiplier, Mathf.Infinity, deltaTime);

        //Math PErfect Smooth: 1f - Mathf.Exp(...)
        float smoothT = 1f - Mathf.Exp(-/*SMOOTHING_SPEED*/smoothRotateSpeed * deltaTime);
        angleProgress = Mathf.Lerp(currentAngleProgress, angleProgress, smoothT);
        SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_RotationRad", false, angleProgress);

        GameObject bubbleShield = effectManager.ShieldObj;
        if (bubbleShield != null && bubbleMaxXoffset != 0 && bubbleShield.activeInHierarchy) {
            //enlargement of the scale of the bubble by width while turning the model 
            Vector3 bp = bubbleShield.transform.localPosition;

            //x pos change from -BUBBLE_INIT_X_OFFSET to +BUBBLE_INIT_X_OFFSETbubbleMaxXoffset
            float newXOffset = Mathf.LerpUnclamped(BUBBLE_INIT_X_OFFSET, bubbleMaxXoffset, angleProgress / maxShapeRotationVal);
            bubbleShield.transform.localPosition = new Vector3(newXOffset, bp.y, bp.z);
        }
    }

    void FishEatAnim(int score) {
        ActivateBiteAnimation();
    }

    public void ActivateBiteAnimation(bool isNeedParticles = true) {
        if (fish.IsVisible) {
            if (animator != null)
                animator.SetTrigger(BITE_TRIGGER_NAME); // play bite animation

            if (isNeedParticles) //for optimization
                fishAddCompMngr.ActivateObjectFromPool(FishComponentType.BiteParticle,
                    editableTransform,
                    biteParticleLocPos,
                    Vector3.one,
                    Quaternion.identity);

            if (fish.IsPlayer)//only player
                audioManager.PlayVFX(VFX_clip.Bite);
        }
    }

    private void OnDisable() {
        if (!isUseRotatingShader)
            return;

        SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_RotationRad", false, 0);
        fish.OnFishEat -= FishEatAnim;
    }
}
