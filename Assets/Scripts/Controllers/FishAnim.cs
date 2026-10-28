using UnityEngine;
using VContainer;


[RequireComponent(typeof(Fish))]
public class FishAnim : MonoBehaviour {
    [SerializeField] Fish fish;
    //[SerializeField] float smoothRotateSpeed = 5f;
    //[SerializeField] float angleRotDetect = 12f;
    //[SerializeField] Rigidbody rb;
    [SerializeField] Animator animator; // animator for fish animations (crab has no animator and dartfish)
    //[SerializeField] AnimationClip idleClip;
    [SerializeField] Renderer fishSkinnedMesh; //To get Mesh material
    [SerializeField] Transform editableTransform;
    [SerializeField] EffectManager effectManager;
    //[SerializeField] Vector3 biteParticleLocPos;
    //[SerializeField] float bubbleMaxXoffset = 0; //change the width of the bubble in the rotation (to make the ends of the tail out)
    //[SerializeField] bool isUseRotatingShader = false;

    public Animator FishAnimator { get { return animator; } private set { animator = value; } }
    public Renderer FishSkinnedMesh => fishSkinnedMesh;
    public bool IsInited { get; private set; } = false;

    //private FishAdditionalCompManager fishAddCompMngr;
    private MaterialPropertyBlock matPropBlock;
    private float matDefSprintSpeed; // default sprint speed in the material
    private float maxShapeRotationVal;

    [Inject] private readonly IAudioPlayService _audioManager;
    [Inject] private readonly IVFXPoolService _fishAddCompMngr;

    private const float BUBBLE_INIT_X_OFFSET = 0;
    private const float SPRINT_SPEED_MULTIPLIER = 2f; // multiplier for sprint speed


    public void Init(/*EffectManager effectManager*/) {
        //this.effectManager = effectManager;

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
            matDefSprintSpeed = fish.DefaultInfo.MaxShaderFrequencyVal;//SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_Frequency", true); // get the default sprint speed from the material
            //if (fish.IsPlayer)
            //    Debug.LogWarning(fish.DefaultInfo.MaxShaderFrequencyVal);
        }

        if (!fish.DefaultInfo.IsUseRotShader/*isUseRotatingShader*/)
            return;

        fish.OnFishEat += FishEatAnim;/* Debug.LogWarning("eat fish event");*/

        maxShapeRotationVal = (fishSkinnedMesh == null) ?
            0f : fish.DefaultInfo.MaxShaderRotationVal;//SetGetMaterialWithPropBlock(ref fishSkinnedMesh, ref matPropBlock, "_MaxRotationValue", true);

        if (animator != null && fish.Level != FishLevel.Pufferfish) {
            animator.ResetTrigger(GameStaticInfo.BITE_ANIMATOR_TRIGGER);
            animator.Rebind();  // reset all to def stage (fix open mouth death)

            ////forse attath data from first keyframe to bones
            //if (idleClip != null)
            //    idleClip.SampleAnimation(gameObject, 0f);

            // restart animator            
            animator.Play(GameStaticInfo.FISH_ANIMATOR_IDLE_STATE, 0, 0f);
            animator.Update(0f);//force apply fisrt keyframe of anim
        }
    }

    private static void InitPropBlock(ref MaterialPropertyBlock matPropBlock, ref Renderer skinnedMesh) {
        matPropBlock = new MaterialPropertyBlock();
        SetMaterialProp(ref skinnedMesh, ref matPropBlock, /*"_Frequency"*/GameStaticInfo.FISH_WAVING_FREQUENCY_PROP, skinnedMesh.sharedMaterial.GetFloat(GameStaticInfo.FISH_WAVING_FREQUENCY_PROP));
        SetMaterialProp(ref skinnedMesh, ref matPropBlock, /*"_MaxRotationValue"*/GameStaticInfo.FISH_WAVING_MAX_ROT_PROP, skinnedMesh.sharedMaterial.GetFloat(GameStaticInfo.FISH_WAVING_MAX_ROT_PROP));
        SetMaterialProp(ref skinnedMesh, ref matPropBlock, /*"_RotationRad"*/GameStaticInfo.FISH_WAVING_ROT_PROP, skinnedMesh.sharedMaterial.GetFloat(GameStaticInfo.FISH_WAVING_ROT_PROP));
    }

    public static void SetMaterialProp(ref Renderer renderer, ref MaterialPropertyBlock mapBlock, int paramId, float value = 0) {
        //Debug.LogWarning(renderer + " " + mapBlock);
        renderer.GetPropertyBlock(mapBlock);
        mapBlock.SetFloat(paramId, value);
        renderer.SetPropertyBlock(mapBlock);
    }

    public static float GetMaterialProp(ref Renderer renderer, ref MaterialPropertyBlock mapBlock, int paramId) {
        renderer.GetPropertyBlock(mapBlock);

        return mapBlock.GetFloat(paramId);
    }

    /// <summary>
    /// enable sprint animation boost if the fish is sprinting, itherwise set the default speed
    /// </summary>
    public void SprintAnimation(bool isSprinting = true) {
        if (isSprinting)
            SetMaterialProp(ref fishSkinnedMesh, ref matPropBlock, GameStaticInfo.FISH_WAVING_FREQUENCY_PROP, matDefSprintSpeed * SPRINT_SPEED_MULTIPLIER);
        else
            SetMaterialProp(ref fishSkinnedMesh, ref matPropBlock, GameStaticInfo.FISH_WAVING_FREQUENCY_PROP, matDefSprintSpeed);
    }

    //[SerializeField] float angleDetct = 13f;
    //[SerializeField] float delta0 = 0.4f;
    public void RotateAnimation(Vector3 fromDir, Vector3 toDir, float deltaTime) {
        if (!fish.DefaultInfo.IsUseRotShader/*isUseRotatingShader*/)
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
        float currentAngleProgress = GetMaterialProp(ref fishSkinnedMesh, ref matPropBlock, GameStaticInfo.FISH_WAVING_ROT_PROP);
        float angleProgress = Mathf.LerpUnclamped(0, maxShapeRotationVal, angle / fish.DefaultInfo.AngRotDetect); // progress from -1 to 1 (LerpUnclamped allows to go beyond the range)
        angleProgress = Mathf.Clamp(angleProgress, -maxShapeRotationVal, maxShapeRotationVal); // clamp the value to avoid exceeding the max rotation value     

        //if (fish.IsPlayer && angle != .0f)
        //    Debug.Log("angle: " + angle + " currentAngProgress: " + currentAngleProgress + " angleProgress:" + angleProgress);                                                                                                                              //angleProgress, ref rotAnimVelocity, enemyAnimDeltaMultiplier, Mathf.Infinity, deltaTime);

        //Math PErfect Smooth: 1f - Mathf.Exp(...)
        float smoothT = 1f - Mathf.Exp(-fish.DefaultInfo.SmoothRotSpeed * deltaTime);
        angleProgress = Mathf.Lerp(currentAngleProgress, angleProgress, smoothT);
        SetMaterialProp(ref fishSkinnedMesh, ref matPropBlock, GameStaticInfo.FISH_WAVING_ROT_PROP, angleProgress);

        GameObject bubbleShield = effectManager.ShieldObj;
        if (bubbleShield != null && /*bubbleMaxXoffset*/fish.DefaultInfo.BubbleXoffset != 0 && bubbleShield.activeInHierarchy) {
            //enlargement of the scale of the bubble by width while turning the model 
            Vector3 bp = bubbleShield.transform.localPosition;

            //x pos change from -BUBBLE_INIT_X_OFFSET to +BUBBLE_INIT_X_OFFSETbubbleMaxXoffset
            float newXOffset = Mathf.LerpUnclamped(BUBBLE_INIT_X_OFFSET, /*bubbleMaxXoffset*/fish.DefaultInfo.BubbleXoffset, angleProgress / maxShapeRotationVal);
            bubbleShield.transform.localPosition = new Vector3(newXOffset, bp.y, bp.z);
        }
    }

    void FishEatAnim(int score) {
        ActivateBiteAnimation();
    }

    public void ActivateBiteAnimation(bool isNeedParticles = true) {
        if (fish.IsVisible && fish.IsActive) {
            if (animator != null)
                animator.SetTrigger(GameStaticInfo.BITE_ANIMATOR_TRIGGER); // play bite animation

            if (isNeedParticles) //for optimization
                _fishAddCompMngr.ActivateObjectFromPool(FishVFX.BiteParticle,
                    editableTransform,
                    /*biteParticleLocPos*/fish.DefaultInfo.BiteParticleLocPos,
                    Vector3.one,
                    Quaternion.identity);

            if (fish.IsPlayer)//only player
                _audioManager.PlayVFX(VFX_clip.Bite);
        }
    }

    private void OnDisable() {
        if (!/*isUseRotatingShader*/fish.DefaultInfo.IsUseRotShader)
            return;

        SetMaterialProp(ref fishSkinnedMesh, ref matPropBlock, GameStaticInfo.FISH_WAVING_ROT_PROP, 0f);
        fish.OnFishEat -= FishEatAnim;
    }
}
