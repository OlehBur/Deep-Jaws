using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EchoaquaController : MonoBehaviour {
    [SerializeField] Image echoaquaShaderImg;
    [SerializeField] GameObject vibrissaeColliders;
    [SerializeField] GameObject lightsVibrissaeParent;
    [SerializeField] Transform mapPreviewCollider;
    [SerializeField] FishHeartbeatsManager heartbeatsManager;
    [SerializeField] PlayerUIController playerUIController;
    [SerializeField] Transform ecqhoMembraneCenter;
    [SerializeField] AudioManager audioManager;
    [SerializeField] Animator echoaquaAnimator;

    private const string VIBRASES_ANIMATION_NAME = "Echoaqua_Vibrissaes";
    private const string REVERSE_VIBRASES_ANIMATION_NAME = "Echoaqua_Vibrissaes_Reverse";
    private const string MEMBRANE_ANIMATION_NAME = "Echoaqua_Membrane";
    //private const float SHADER_UPDATE_INTERVAL = 0.05f;
    private const float MAP_PREVIEW_COLLIDER_OFFSET_Y = 35f;
        private const float SCAN_PREVIEW_TIME = 4f;
    private const float COOLDOWN_TIME = SCAN_PREVIEW_TIME + 15f;
    //private const float PROGRESS_DELTA_SPEED = SHADER_UPDATE_INTERVAL;
    private const float PROGRESS_TRIGGER_FOR_COLLIDER = 0.4f;
    private static readonly int ProgressShaderID = Shader.PropertyToID("_Progress");
    private static readonly int ScanPosNormalizeShaderID = Shader.PropertyToID("_ScanPosNormalize");
    //private readonly WaitForSecondsRealtime waitForShaderUpdate = new WaitForSecondsRealtime(SHADER_UPDATE_INTERVAL);
    private readonly WaitForSecondsRealtime waitForScanPreview = new WaitForSecondsRealtime(SCAN_PREVIEW_TIME);

    private float mapPreviewColliderInitialY;
    private float cooldownTimer = 0f;
    private Coroutine echoCoroutine;
    private Material echoaquaShader;

    private void Start() {
        mapPreviewColliderInitialY = mapPreviewCollider.position.y;
        mapPreviewCollider.position = new Vector3(
            mapPreviewCollider.position.x,
            mapPreviewColliderInitialY + MAP_PREVIEW_COLLIDER_OFFSET_Y,
            mapPreviewCollider.position.z
        );
        echoaquaShader = echoaquaShaderImg.material;
    }

    private void Update() {
        if (!vibrissaeColliders.activeInHierarchy) {
            cooldownTimer += DJTime.UnscaledDeltaTime;

            if (cooldownTimer >= COOLDOWN_TIME) {
                cooldownTimer = 0f;
                echoaquaAnimator.Play(REVERSE_VIBRASES_ANIMATION_NAME, 1);
                vibrissaeColliders.SetActive(true);
                lightsVibrissaeParent.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        Transform fishParent = other.attachedRigidbody.transform;
        if (!other.isTrigger && fishParent.CompareTag(GameStaticInfo.TAG_PLAYER)) {
            if (echoCoroutine != null) {
                StopCoroutine(echoCoroutine);
                echoCoroutine = null;
            }

            echoaquaShader.SetFloat(ProgressShaderID, 0f);
            vibrissaeColliders.SetActive(false);
            lightsVibrissaeParent.SetActive(false);
            audioManager.PlayVFX(VFX_clip.Echo);

            echoaquaAnimator.Play(MEMBRANE_ANIMATION_NAME, 0);
            echoaquaAnimator.Play(VIBRASES_ANIMATION_NAME, 1);
            //playerUIController.SetPlayableAreaActive(false);
        }
    }

    public void ActivateEcho() {
        if (echoCoroutine != null)
            StopCoroutine(echoCoroutine);

        echoCoroutine = StartCoroutine(EchoProcessRoutine());
    }

    private IEnumerator EchoProcessRoutine() {
        heartbeatsManager.SetActivateShaderPreparation(true);
        float currentProgress = 0f;
        bool isColliderPositionReset = false;

        // def proggress 0 -> 1
        while (currentProgress < 1f) {
            echoaquaShader.SetVector(ScanPosNormalizeShaderID, GameStaticInfo.WorldToNormalizeScreenPos(ecqhoMembraneCenter.position, Camera.current)); //in case of movement player
            currentProgress += /*PROGRESS_DELTA_SPEED*/DJTime.UnscaledDeltaTime;
            //Debug.Log(currentProgress);
            if (!isColliderPositionReset && currentProgress >= PROGRESS_TRIGGER_FOR_COLLIDER) {
                mapPreviewCollider.position = new Vector3(
                    mapPreviewCollider.position.x,
                    mapPreviewColliderInitialY,
                    mapPreviewCollider.position.z
                );
                isColliderPositionReset = true;
            }

            echoaquaShader.SetFloat(ProgressShaderID, Mathf.Min(currentProgress, 1f));
            yield return /*waitForShaderUpdate*/null;
        }

        yield return waitForScanPreview;

        // reset collider & ui activate
        mapPreviewCollider.position = new Vector3(
            mapPreviewCollider.position.x,
            mapPreviewColliderInitialY + MAP_PREVIEW_COLLIDER_OFFSET_Y,
            mapPreviewCollider.position.z
        );
        //playerUIController.SetPlayableAreaActive(true);

        // inverse prog 1 -> 0
        currentProgress = 0f;
        while (currentProgress > 0f) {
            echoaquaShader.SetVector(ScanPosNormalizeShaderID, GameStaticInfo.WorldToNormalizeScreenPos(ecqhoMembraneCenter.position, Camera.current)); //in case of movement player
            currentProgress -= /*PROGRESS_DELTA_SPEED*/DJTime.UnscaledDeltaTime;
            echoaquaShader.SetFloat(ProgressShaderID, Mathf.Max(currentProgress, 0f));
            yield return /*waitForShaderUpdate*/null;
        }

        heartbeatsManager.SetActivateShaderPreparation(false);
        echoCoroutine = null;
    }
}