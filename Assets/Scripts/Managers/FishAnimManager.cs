using UnityEngine;


[RequireComponent(typeof(Fish))]
public class FishAnimManager : MonoBehaviour {
    [SerializeField] Fish fish;
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator; // animator for fish animations (crab has no animator)
    public Animator FishAnimator { get { return animator; } private set { animator = value; } }
    const string BITE_TRIGGER_NAME = "bite"; // name of the trigger for bite animation
    //[SerializeField] GameObject biteParticleSystem = null; // particle system for bite animation (crab has no particle system)'
    private FishAdditionalCompManager fishAddCompMngr;
    [SerializeField] Transform editableTransform;
    [SerializeField] Vector3 biteParticleLocPos;
    private AudioManager audioManager;
    //[SerializeField] AudioSource biteAudioSource; // audio source for bite sound effect
    [SerializeField] bool isUseRotatingShader = false;

    [SerializeField] SkinnedMeshRenderer fishSkinnedMesh; //To get Mesh material
    //[SerializeField] MeshRenderer fishMesh; //To get Mesh material
    private MaterialPropertyBlock matPropBlock;
    const float SPRINT_SPEED_MULTIPLIER = 2f; // multiplier for sprint speed
    private float matDefSprintSpeed; // default sprint speed in the material
    //private Material fishMat;
    private float maxShapeRotationVal;
    private float rotAnimVelocity = 0f;

    //[SerializeField] GameObject bubbleShield = null;
    private EffectManager effectManager;
    [SerializeField] float bubbleMaxXoffset = 0; //change the width of the bubble in the rotation (to make the ends of the tail out)
    private const float BUBBLE_INIT_X_OFFSET = 0;

    //for shape rotation
    //private Quaternion prevRotation;



    public void Init(AudioManager audioManager, EffectManager effectManager, FishAdditionalCompManager fishAddCompMngr/*Animator animator*/) {
        this.audioManager = audioManager;
        this.effectManager = effectManager;
        this.fishAddCompMngr = fishAddCompMngr;
        //this.animator = animator;
    }

    void OnEnable() {
        //if (fishMesh != null)
        //    fishMat = fishMesh.material;
        //else if (fishSkinnedMesh != null)
        //    fishMat = fishSkinnedMesh.material;

        //init set prop block

        if (fishSkinnedMesh == null) {
            matDefSprintSpeed = 1f;
        }
        else {
            InitPropBlock();
            matDefSprintSpeed = SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_Frequency", true); // get the default sprint speed from the material
        }

        if (!isUseRotatingShader)
            return;

        fish.OnFishEat += FishEatAnim;/* Debug.LogWarning("eat fish event");*/

        //prevRotation = rb.rotation;
        ////bubbleInitXoffset = bubbleShield.transform.localScale.x;


        maxShapeRotationVal = (fishSkinnedMesh == null) ?
            0f : SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_MaxRotationValue", true);//fishMat?.GetFloat("_MaxRotationValue") ?? 0;
        //biteAudioSource.enabled = fish.IsPlayer; // enable audio source only for player fish
    }

    private void InitPropBlock() {
        matPropBlock = new MaterialPropertyBlock();
        SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_Frequency", false, fishSkinnedMesh.sharedMaterial.GetFloat("_Frequency"));
        SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_MaxRotationValue", false, fishSkinnedMesh.sharedMaterial.GetFloat("_MaxRotationValue"));
        SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_RotationRad", false, fishSkinnedMesh.sharedMaterial.GetFloat("_RotationRad"));
        //fishSkinnedMesh.material.SetFloat("_RotationRad", 0f);// test
    }

    public static float SetGetMaterialWithPropBlock(SkinnedMeshRenderer renderer, ref MaterialPropertyBlock mapBlock, string paramName, bool isGet, float value = 0) {
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
            SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_Frequency", false, matDefSprintSpeed * SPRINT_SPEED_MULTIPLIER);
        //fishMat.SetFloat("_Frequency", matDefSprintSpeed * SPRINT_SPEED_MULTIPLIER/* + sprintAudioSource.volume * matDefSprintSpeed*/); //double (max) the speed of the fish material effect
        else
            SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_Frequency", false, matDefSprintSpeed);
        //fishMat.SetFloat("_Frequency", matDefSprintSpeed);
    }

    public void RotateAnimation(Vector3 fromDir, Vector3 toDir, float deltaTime, float enemyAnimDeltaMultiplier = .08f) {
        if (!isUseRotatingShader)
            return;

        const float ANGLE_DETECTION = 45f;
        Quaternion currentRotation = rb.rotation;

        // ang betw
        //Vector3 fromDir = current * Vector3.forward;
        //Vector3 toDir = next * Vector3.forward;

        float angle = Vector3.SignedAngle(fromDir, toDir, Vector3.up);
        //float speed = Mathf.Abs(angle) / deltaTime;  //curr angul speed
        //Debug.Log("Obj rot! Speed: " + speed + "°/s");
        //Debug.Log("ang: " + angle);
        float angleProgress = Mathf.LerpUnclamped(0, maxShapeRotationVal, angle / ANGLE_DETECTION); // progress from -1 to 1 (LerpUnclamped allows to go beyond the range)
        angleProgress = Mathf.Clamp(angleProgress, -maxShapeRotationVal, maxShapeRotationVal); // clamp the value to avoid exceeding the max rotation value
        angleProgress = Mathf.SmoothDamp(SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_RotationRad", true)/*fishMat.GetFloat("_RotationRad")*/,
            angleProgress, ref rotAnimVelocity, enemyAnimDeltaMultiplier, Mathf.Infinity, deltaTime);

        SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_RotationRad", false, angleProgress);
        //fishMat.SetFloat("_RotationRad", /*currentRotationVal*/angleProgress);

        //prevRotation = currentRotation;
        GameObject bubbleShield = effectManager.ShieldObj;
        if (bubbleShield != null && bubbleMaxXoffset != 0 && bubbleShield.activeInHierarchy) {
            //enlargement of the scale of the bubble by width while turning the model 
            Vector3 bp = bubbleShield.transform.localPosition;

            //x pos change from -BUBBLE_INIT_X_OFFSET to +BUBBLE_INIT_X_OFFSETbubbleMaxXoffset
            float newXOffset = Mathf.LerpUnclamped(BUBBLE_INIT_X_OFFSET, bubbleMaxXoffset, angleProgress / maxShapeRotationVal);
            bubbleShield.transform.localPosition = new Vector3(newXOffset, bp.y, bp.z);
            //}
        }
    }

    void FishEatAnim(int score) {
        ActivateBiteAnimation();
    }

    public void ActivateBiteAnimation(bool isNeedParticles = true) {
        //if(animator.isActiveAndEnabled)
        FishAnimator?.SetTrigger(BITE_TRIGGER_NAME); // play bite animation
        //biteParticleSystem.SetActive(true); //play particle on awake

        if (isNeedParticles && fish.IsVisible/*IsPlayer*/ /*Vector3.Distance(transform.position, Camera.main.transform.position) <= biteAudioSource.maxDistance*/) {//for optimization
            //if (biteAudioSource.enabled)
            //    biteAudioSource.PlayOneShot(biteAudioSource.clip); // play bite sound effect
            audioManager.PlayVFX(VFX_clip.Bite);
            fishAddCompMngr.ActivateObjectFromPool(FishComponentType.BiteParticle,
                editableTransform,
                biteParticleLocPos, 
                Vector3.one,
                Quaternion.identity);
            //Instantiate(biteParticleSystem, biteParticleSystem.transform.position, biteParticleSystem.transform.rotation).SetActive(true); // instantiate bite particle system at fish position
        }
    }

    private void OnDisable() {
        if (!isUseRotatingShader)
            return;

        SetGetMaterialWithPropBlock(fishSkinnedMesh, ref matPropBlock, "_RotationRad", false, 0);
        //fishMat.SetFloat("_RotationRad", 0);
        fish.OnFishEat -= FishEatAnim;
    }
}
