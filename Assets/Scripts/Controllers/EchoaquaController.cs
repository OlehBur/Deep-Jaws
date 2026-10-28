using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EchoaquaController : MonoBehaviour {
    [SerializeField] Image echoaquaShaderImg;
    [SerializeField] GameObject vibrissaeColliders;
    [SerializeField] GameObject lightsVibrissaeParent;
    [SerializeField] FishHeartbeatsManager heartbeatsManager;
    [SerializeField] PlayerUIController playerUIController;
    [SerializeField] Transform ecqhoMembraneCenter;
    [SerializeField] Animator echoaquaAnimator;

    private const string VIBRASES_ANIMATION_NAME = "Echoaqua_Vibrissaes";
    private const string REVERSE_VIBRASES_ANIMATION_NAME = "Echoaqua_Vibrissaes_Reverse";
    private const string MEMBRANE_ANIMATION_NAME = "Echoaqua_Membrane";
    private const float SCAN_PREVIEW_TIME = 4f;
    private const float COOLDOWN_TIME = SCAN_PREVIEW_TIME + 15f;
    private const float PROGRESS_TRIGGER_FOR_COLLIDER = 0.4f;
    private static readonly int ProgressShaderID = Shader.PropertyToID("_Progress");
    private static readonly int ScanPosNormalizeShaderID = Shader.PropertyToID("_ScanPosNormalize");

    [Inject] private readonly IAudioPlayService _audioManager;
    private readonly WaitForSecondsRealtime _waitForScanPreview = new WaitForSecondsRealtime(SCAN_PREVIEW_TIME);

    private float _cooldownTimer = 0f;
    private Coroutine _echoCoroutine;
    private Material _echoaquaShader;

    private void Start() {
        _echoaquaShader = echoaquaShaderImg.material;
    }

    private void Update() {
        if (!vibrissaeColliders.activeInHierarchy) {
            _cooldownTimer += DJTime.UnscaledDeltaTime;

            if (_cooldownTimer >= COOLDOWN_TIME) {
                _cooldownTimer = 0f;
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
            if (_echoCoroutine != null) {
                StopCoroutine(_echoCoroutine);
                _echoCoroutine = null;
            }

            _echoaquaShader.SetFloat(ProgressShaderID, 0f);
            vibrissaeColliders.SetActive(false);
            lightsVibrissaeParent.SetActive(false);
            _audioManager.PlayVFX(VFX_clip.Echo);

            echoaquaAnimator.Play(MEMBRANE_ANIMATION_NAME, 0);
            echoaquaAnimator.Play(VIBRASES_ANIMATION_NAME, 1);
        }
    }

    public void ActivateEcho() {
        if (_echoCoroutine != null)
            StopCoroutine(_echoCoroutine);

        _echoCoroutine = StartCoroutine(EchoProcessRoutine());
    }

    private IEnumerator EchoProcessRoutine() {
        if (CameraFollower.Instance == null)
            Debug.LogWarning("Camera Follow inst is null");

        heartbeatsManager.SetActivateShaderPreparation(true);
        float currentProgress = 0f;

        // def proggress 0 -> 1
        while (currentProgress < 1f) {
            _echoaquaShader.SetVector(ScanPosNormalizeShaderID, GameStaticInfo.WorldToNormalizeScreenPos(ecqhoMembraneCenter.position, Camera.current)); //in case of movement player
            currentProgress += DJTime.UnscaledDeltaTime;
            //Debug.Log(currentProgress);
            if (!CameraFollower.Instance.IsMapViewActive && currentProgress >= PROGRESS_TRIGGER_FOR_COLLIDER)
                CameraFollower.Instance.IsMapViewActive = true;

            _echoaquaShader.SetFloat(ProgressShaderID, Mathf.Min(currentProgress, 1f));
            yield return null;
        }

        yield return _waitForScanPreview;

        // reset collider & ui activate
        CameraFollower.Instance.IsMapViewActive = false;

        // inverse prog 1 -> 0
        currentProgress = 0f;
        while (currentProgress > 0f) {
            _echoaquaShader.SetVector(ScanPosNormalizeShaderID, GameStaticInfo.WorldToNormalizeScreenPos(ecqhoMembraneCenter.position, Camera.current)); //in case of movement player
            currentProgress -= /*PROGRESS_DELTA_SPEED*/DJTime.UnscaledDeltaTime;
            _echoaquaShader.SetFloat(ProgressShaderID, Mathf.Max(currentProgress, 0f));
            yield return null;
        }

        heartbeatsManager.SetActivateShaderPreparation(false);
        _echoCoroutine = null;
    }
}