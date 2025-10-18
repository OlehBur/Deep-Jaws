using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// для зміни кольору маскування каракатиці в залежності від середовища та випускання отруйного чорнила під час небезпеки
/// to change the color of the cuttlefish's camouflage depending on the environment and to release poisonous ink when in danger
/// </summary>
public class СuttlefishController : MonoBehaviour {
    [SerializeField] Fish fish;
    [SerializeField] MeshRenderer mesh;
    private MaterialPropertyBlock matPropBlock;
    //[SerializeField] AudioSource audioSource;
    private AudioManager audioManager;
    //private Material shaderMat;
    private const float SHADER_UPDATE_INTERVAL = .1f;
    private const float OVERLAP_RADIUS = 20f;
    private int OVERLAP_LAYER_MASK;
    private Collider[] hitObjects = new Collider[10];
    private Color prev_targetColor = Color.black;
    private Color targetColor = Color.black;
    private bool isAllowOverlapping = true;
    //private bool isNewColor = true;
    //private bool isReverseProgress = false;
    private bool isNeedReverseProgress = false;

    //[SerializeField] GameObject poisonInkPrefab;
    private FishAdditionalCompManager fishAddComp;
    private const float POISON_COOLDOWN = 10f;
    private float curentPoisonCooldown = 0f;
    private static readonly WaitForSeconds SHADER_UPD_WAIT = new WaitForSeconds(SHADER_UPDATE_INTERVAL);

    private static readonly Dictionary<string, Color> tagColors = new() {
    { "Gray", Color.gray },
    { "Green", Color.green },
    { "Pink", new Color(1f, 0f, 0.9190922f, 1f) },
    { "Orange", new Color(1f, 0.6761332f, 0.3066038f, 1f) },
    { "Red", Color.red },
    { "Yellow", Color.yellow },
    { "Blue", Color.blue },
    { "Turquoise", new Color(0f, 1f, .9052942f, 1f) },
    { "Violete", new Color(.4672455f, 0f, 1f, 1f) }
};


    public void Init(AudioManager audioManager, FishAdditionalCompManager fishAddComp) {
        this.audioManager = audioManager;
        this.fishAddComp = fishAddComp;
    }

    private void Awake() {
        //shaderMat = mesh.material;
        InitPropBlock(ref mesh, ref matPropBlock);
        //shaderMat.SetFloat("_Progress", 0);
        OVERLAP_LAYER_MASK = LayerMask.GetMask("Default", "Obstacle");
    }

    static void InitPropBlock(ref MeshRenderer renderer, ref MaterialPropertyBlock matBlock) {
        matBlock = new MaterialPropertyBlock();
        SetMatPropBlock(ref renderer, ref matBlock, "_Progress", 0);
        SetMatPropBlock(ref renderer, ref matBlock, "_Color", Color.black);
        //fishSkinnedMesh.material.SetFloat("_RotationRad", 0f);// test
    }

    static float GetMatPropBlock(ref MeshRenderer renderer, ref MaterialPropertyBlock mapBlock, string paramName) {
        renderer.GetPropertyBlock(mapBlock);
        return mapBlock.GetFloat(paramName);
    }

    static void SetMatPropBlock(ref MeshRenderer renderer, ref MaterialPropertyBlock mapBlock, string paramName, float value = 0) {
        renderer.GetPropertyBlock(mapBlock);

        //return mapBlock.GetFloat(paramName);
        mapBlock.SetFloat(paramName, value);
        renderer.SetPropertyBlock(mapBlock);
    }

    static void SetMatPropBlock(ref MeshRenderer renderer, ref MaterialPropertyBlock mapBlock, string paramName, Color color) {
        renderer.GetPropertyBlock(mapBlock);

        mapBlock.SetColor(paramName, color);
        renderer.SetPropertyBlock(mapBlock);
    }

    void FixedUpdate() {
        if (fish.IsActive && isAllowOverlapping)
            Overlapping();

        curentPoisonCooldown = (curentPoisonCooldown - Time.fixedDeltaTime < 0) ? 0 : curentPoisonCooldown - Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.isTrigger)
            TriggerInkBehaviour(other);
    }

    private void OnTriggerStay(Collider other) {
        if (!other.isTrigger)
            TriggerInkBehaviour(other);
    }

    void TriggerInkBehaviour(Collider other) {
        Transform parentFish = other.transform.parent;

        if (curentPoisonCooldown == 0 && fish.IsVisible && parentFish != null && GameStaticInfo.TryGetFish(parentFish.gameObject, out Fish enemyFish)/*other.transform.parent.TryGetComponent<Fish>(out Fish enemyFish)*/
    && enemyFish.Level > GameStaticInfo.PEACEFUL_FISH_LVL/*enemyFish.IsLegitimateTarget(fish)*/) {
            audioManager.PlayVFX(VFX_clip.Cuttlefish);
            //audioSource.PlayOneShot(audioSource.clip);
            curentPoisonCooldown = POISON_COOLDOWN;

            fishAddComp.ActivateObjectFromPool(FishComponentType.InkParticles, transform.parent, transform.localPosition, Vector3.one, transform.rotation);
            //Instantiate(poisonInkPrefab, transform.position, transform.rotation);
        }
    }

    void Overlapping() {
        int count = Physics.OverlapSphereNonAlloc(transform.position, OVERLAP_RADIUS, hitObjects, OVERLAP_LAYER_MASK);

        Collider nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < count; i++) {
            float currDist = (hitObjects[i].transform.position - transform.position).sqrMagnitude;
            if (currDist < minDist) {
                minDist = currDist;
                nearest = hitObjects[i];
            }
        }

        if (nearest != null) {
            //Debug.Log("Nearest: " + nearest.name); //The Component.tag property allocates managed memory.
            //string tag = nearest.tag;

            //targetColor = tag switch {
            //    "Gray" => Color.gray,
            //    "Green" => Color.green,
            //    "Pink" => new Color(1f, 0f, 0.9190922f, 1f),
            //    "Orange" => new Color(1f, 0.6761332f, 0.3066038f, 1f),
            //    "Red" => Color.red,
            //    "Yellow" => Color.yellow,
            //    "Blue" => Color.blue,
            //    "Turquoise" => new Color(0f, 1f, .9052942f, 1f),
            //    "Violete" => new Color(.4672455f, 0f, 1f, 1f),
            //    _ => Color.black
            //};

            foreach (KeyValuePair<string, Color> tagColor in tagColors) {
                if (nearest.CompareTag(tagColor.Key)) {
                    targetColor = tagColor.Value;
                    break;
                }
            }

        }
        else
            targetColor = Color.black;//reset camouflage

        if (prev_targetColor != targetColor) {
            //Debug.Log("Color: " + nearest.tag);
            isNeedReverseProgress = GetMatPropBlock(ref mesh, ref matPropBlock, "_Progress") >= 1; //shaderMat.GetFloat("_Progress") >= 1;
            //    isReverseProgress = true;

            StartCoroutine(CamouflageProgress(isNeedReverseProgress, targetColor == Color.black));
            //InvokeRepeating(nameof(CamouflageProgress), 0f, SHADER_UPDATE_INTERVAL);
            isAllowOverlapping = false;
            //isNewColor = true;
        }

        prev_targetColor = targetColor;
    }

    IEnumerator CamouflageProgress(bool isNeedReverse, bool isJustReset) {
        float progress = GetMatPropBlock(ref mesh, ref matPropBlock, "_Progress");//shaderMat.GetFloat("_Progress");

        while (isNeedReverse) {
            SetMatPropBlock(ref mesh, ref matPropBlock, "_Progress", Mathf.Clamp01(progress - SHADER_UPDATE_INTERVAL));
            //shaderMat.SetFloat("_Progress", Mathf.Clamp01(progress - SHADER_UPDATE_INTERVAL));
            progress = GetMatPropBlock(ref mesh, ref matPropBlock, "_Progress");// shaderMat.GetFloat("_Progress");
            if (progress <= 0f)
                break;

            yield return SHADER_UPD_WAIT/*new WaitForSeconds(SHADER_UPDATE_INTERVAL)*/;
        }

        if (isJustReset) {
            isAllowOverlapping = true;
            yield break;
        }

        SetMatPropBlock(ref mesh, ref matPropBlock, "_Color", targetColor);
        //shaderMat.SetColor("_Color", targetColor);
        while (progress < 1f) {
            SetMatPropBlock(ref mesh, ref matPropBlock, "_Progress", Mathf.Clamp01(progress + SHADER_UPDATE_INTERVAL));
            //shaderMat.SetFloat("_Progress", Mathf.Clamp01(progress + SHADER_UPDATE_INTERVAL));
            progress = GetMatPropBlock(ref mesh, ref matPropBlock, "_Progress");//shaderMat.GetFloat("_Progress");
            yield return SHADER_UPD_WAIT/*new WaitForSeconds(SHADER_UPDATE_INTERVAL)*/;
        }

        isAllowOverlapping = true;

        //if (!isNewColor && (progress >= 1 && !isReverseProgress) || (progress <= 0 && isReverseProgress)) {
        //    CancelInvoke(nameof(CamouflageProgress));
        //    isAllowOverlapping = true;
        //    isReverseProgress = !isReverseProgress;
        //    yield return null;
        //}

        //if (isReverseProgress) {
        //    shaderMat.SetFloat("_Progress", Mathf.Clamp01(progress - SHADER_UPDATE_INTERVAL));
        //}
        //else if (isNewColor) {
        //    shaderMat.SetColor("_Color", targetColor);
        //    shaderMat.SetFloat("_Progress", Mathf.Clamp01(progress + SHADER_UPDATE_INTERVAL));

        //    if (progress + SHADER_UPDATE_INTERVAL >= 1f)
        //        isNewColor = false;
        //}

        //Debug.Log("prog: " + shaderMat.GetFloat("_Progress"));

    }

    private void OnDrawGizmos() {
        Gizmos.color = targetColor;
        Gizmos.DrawWireSphere(transform.position, OVERLAP_RADIUS);
    }
}
