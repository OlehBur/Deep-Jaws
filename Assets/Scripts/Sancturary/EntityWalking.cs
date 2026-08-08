using UnityEngine;

public class EntityWalking : MonoBehaviour {
    [SerializeField] Transform movePoints;
    [SerializeField] float moveSpeed = 5f;//m\s
    //[SerializeField] float shaderRotSpeed = 3f;//m\s
    [SerializeField]    float rotateSpeed = 180f;//deg per sec

    //[SerializeField] private float smoothTime = 1.74f;
    [SerializeField] bool isHasFishShader;
    [SerializeField] bool isOnlyHorizontalRotation = false;
    [SerializeField] Renderer fishRenderer;

    //private Vector3 currentVelocity;
    private int currentPointIndex = 0;
    private Vector3 targetPoint;
    private Vector3 directionToTarget;
    //private Quaternion targetRotation;
    private MaterialPropertyBlock matPropBlock;
    //private float shaderCurrentSmoothRot = 0f;
    private float SHADER_MAX_ROT_VAL = 0f;

    private const float MAX_LOOK_ROTATION_DETECT = 90f;
    private const float TARGET_EPSILON = 0.1f;


    private void Awake() {
        targetPoint = movePoints.GetChild(currentPointIndex).position;

        if (isHasFishShader) {
            matPropBlock = new MaterialPropertyBlock();
            float defaultWaveRotation = fishRenderer.sharedMaterial.GetFloat(GameStaticInfo.FISH_WAVING_ROT_PROP);
            SHADER_MAX_ROT_VAL = fishRenderer.sharedMaterial.GetFloat(GameStaticInfo.FISH_WAVING_MAX_ROT_PROP);

            FishAnim.SetMaterialProp(ref fishRenderer, ref matPropBlock,
                GameStaticInfo.FISH_WAVING_ROT_PROP, defaultWaveRotation);
            //FishAnim.SetMaterialProp(ref renderer, ref matPropBlock,
            //    GameStaticInfo.FISH_WAVING_MAX_ROT_PROP, SHADER_MAX_ROT_VAL);
        }
    }

    private void Update() {
        if (movePoints == null)
            return;

        //dir to targ
        directionToTarget = targetPoint - transform.position;
        if (isOnlyHorizontalRotation)
            directionToTarget.y = 0f;//onl horiz

        Movement();
        Rotation();
        ShaderRotation();
    }

    void Movement() {
        //smooth move
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint,
            moveSpeed * Time.deltaTime
        );
        //transform.position = Vector3.SmoothDamp(
        //    transform.position,
        //    targetPoint,
        //    ref currentVelocity,
        //    smoothTime);
    }

    void Rotation() {
        if (directionToTarget.sqrMagnitude > TARGET_EPSILON) {//check if not same point
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
            // checl ang (0 - look on targ, 180 - otherwise)
        }
        else {
            currentPointIndex++;
            currentPointIndex = currentPointIndex < movePoints.childCount ?
                currentPointIndex : 0;

            targetPoint = movePoints.GetChild(currentPointIndex).position;
        }

    }

    void ShaderRotation() {
        if (isHasFishShader) {
            //currentAngleToTarget = Quaternion.Angle(transform.rotation, targetRotation);
            float currentAngleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
            float rotationSteepnessNormalized = currentAngleToTarget / MAX_LOOK_ROTATION_DETECT;
            //smooth rotate
            //shaderCurrentSmoothRot = Mathf.MoveTowards(shaderCurrentSmoothRot, rotationSteepnessNormalized * SHADER_MAX_ROT_VAL, shaderRotSpeed * Time.deltaTime);

            FishAnim.SetMaterialProp(ref fishRenderer, ref matPropBlock, GameStaticInfo.FISH_WAVING_ROT_PROP, /*shaderCurrentSmoothRot*/rotationSteepnessNormalized * SHADER_MAX_ROT_VAL);
            //Debug.Log(rotationSteepnessNormalized);
        }
    }
}



