using UnityEngine;

public class EchoaquaController : MonoBehaviour {
    [SerializeField] Material echoaquaShader;
    [SerializeField] /*Collider*/GameObject vibrissaeColliders/*echoaquaTriggerCollider*/;
    [SerializeField] GameObject lightsVibrissaeParent;
    [SerializeField] /*Collider*/Transform mapPreviewCollider/*echoaquaSolidCollider*/;

    [SerializeField] FishHeartbeatsManager heartbeatsManager;
    [SerializeField] PlayerUIController playerUIController;
    [SerializeField] Transform ecqhoMembraneCenter;

    [SerializeField] AudioManager audioManager;
    //[SerializeField] AudioSource echoaquaAudioSource;
    //[SerializeField] AudioClip echoquaClip;

    [SerializeField] Animator echoaquaAnimator;

    private const string VIBRASES_ANIMATION_NAME = "Echoaqua_Vibrissaes";
    private const string REVERSE_VIBRASES_ANIMATION_NAME = "Echoaqua_Vibrissaes_Reverse";
    private const string MEMBRANE_ANIMATION_NAME = "Echoaqua_Membrane";

    private const float SHADER_UPDATE_INTERVAL = .05f;
    private const float MAP_PREVIEW_COLLIDER_OFFSET_Y = 35f;

    private const float COOLDOWN_TIME = SCAN_PREVIEW_TIME + 15f;
    private const float SCAN_PREVIEW_TIME = 3f;

    private float mapPreviewColliderInitialY;
    private float cooldownTimer = 0;
    private bool isReverseProgress = false;

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        mapPreviewColliderInitialY = mapPreviewCollider.position.y;
        mapPreviewCollider.position = new Vector3(mapPreviewCollider.position.x, mapPreviewColliderInitialY + MAP_PREVIEW_COLLIDER_OFFSET_Y, mapPreviewCollider.position.z);
    }

    // Update is called once per frame
    void Update() {
        if (!vibrissaeColliders.activeInHierarchy) {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= COOLDOWN_TIME) {
                cooldownTimer = 0f;
                echoaquaAnimator.Play(REVERSE_VIBRASES_ANIMATION_NAME, 1);
                vibrissaeColliders.SetActive(true);
                lightsVibrissaeParent.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Debug.Log(other.transform.parent);
        if (!other.isTrigger && other.transform.parent != null && other.transform.parent.CompareTag(GameStaticInfo.TAG_PLAYER)) {
            echoaquaShader.SetFloat("_Progress", 0f);//reset previous 
            vibrissaeColliders.SetActive(false);
            lightsVibrissaeParent.SetActive(false);
            audioManager.PlayVFX(VFX_clip.Echo);
            //echoaquaAudioSource.PlayOneShot(echoquaClip);
            echoaquaAnimator.Play(MEMBRANE_ANIMATION_NAME, 0);
            echoaquaAnimator.Play(VIBRASES_ANIMATION_NAME, 1);
            playerUIController.SetPlayableAreaActive(false);
        }
    }

    //called by animation event (int MEMBRANE_ANIMATION)
    public void ActivateEcho() {
        //playerUIController.SetPlayableAreaActive(false);
        echoaquaShader.SetVector("_ScanPosNormalize", /*ecqhoMembraneCenter.position*/GameStaticInfo.WorldToNormalizeScreenPos(ecqhoMembraneCenter.position));
        //Debug.Log(GameStaticInfo.WorldToNormalizeScreenPos(ecqhoMembraneCenter.position));
        isReverseProgress = false;
        Invoke(nameof(DisableSolidTrigger), SCAN_PREVIEW_TIME);

        //echoaquaShader.SetFloat("_Progress", 1f);
        heartbeatsManager.SetActivateShaderPreparation(true);
        InvokeRepeating(nameof(UpdateShaderProgress), SHADER_UPDATE_INTERVAL, SHADER_UPDATE_INTERVAL);
    }

    void DisableSolidTrigger() {
        mapPreviewCollider.position = new Vector3(mapPreviewCollider.position.x, mapPreviewColliderInitialY + MAP_PREVIEW_COLLIDER_OFFSET_Y, mapPreviewCollider.position.z);
        //echoaquaSolidCollider.enabled = false;
        playerUIController.SetPlayableAreaActive(true);
        isReverseProgress = true;
        InvokeRepeating(nameof(UpdateShaderProgress), SHADER_UPDATE_INTERVAL, SHADER_UPDATE_INTERVAL);
    }

    void UpdateShaderProgress() {
        const float PROGRESS_DELTA_SPEED = .06f;
        const float PROGRESS_TRIGGER_FOR_COLLIDER = .4f;
        float currentProg = echoaquaShader.GetFloat("_Progress");

        if (isReverseProgress) {//ending animation
            if (currentProg <= 0) {
                heartbeatsManager.SetActivateShaderPreparation(false);
                CancelInvoke(nameof(UpdateShaderProgress));
                return;
            }
            currentProg -= PROGRESS_DELTA_SPEED;
        }
        else {
            if (currentProg >= PROGRESS_TRIGGER_FOR_COLLIDER)
                mapPreviewCollider.position = new Vector3(mapPreviewCollider.position.x, mapPreviewColliderInitialY, mapPreviewCollider.position.z);
            //echoaquaSolidCollider.enabled = true;
            if (currentProg >= 1) {
                CancelInvoke(nameof(UpdateShaderProgress));
                return;
            }
            currentProg += PROGRESS_DELTA_SPEED;

        }

        echoaquaShader.SetFloat("_Progress", currentProg);
    }

}
