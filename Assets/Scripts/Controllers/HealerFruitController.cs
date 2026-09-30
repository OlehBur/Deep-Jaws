using UnityEngine;


[RequireComponent(typeof(Collider))]
public class HealerFruitController : MonoBehaviour {
    [SerializeField] Collider fruitCollider;
    [SerializeField] AudioManager audioManager;
    [SerializeField] FishAdditionalCompManager dishAddCompMangr;
    //[SerializeField] AudioSource audioSource;
    //[SerializeField] AudioClip healClip;
    [SerializeField] Light fruitLight;
    [SerializeField] MeshRenderer fruitRender;
    //[SerializeField] GameObject healParticle;
    private Material fruitMaterial; // reference to the material for emission color
    private const float MATERIAL_EMISSION_SCALE = 10f; // scale for emission color
    private const float PROGGRESS_THRESHOLD = 0.95f; // threshold for progress calculation
    private const float PROGGRESS_MULTIPLIER = 1f / (1f - PROGGRESS_THRESHOLD);
    private float lightIntensity;
    private const float GROWTH_SPEED = .02f;
    private const int HEAL_DIVIDER = 4; // how much health fish will get from the fruit
    private Vector3 growthScale;

    private static readonly Vector3[] fishesHealParticleScales = new Vector3[] {
        Vector3.one, //crab only for corect indexing
        new Vector3(.7f, 1f, 1.2f),//sea bass
        new Vector3(.8f, 1.1f, 1.8f),//mackerel
        new Vector3(1.5f, 1f, 3.8f),//moray eel
        new Vector3(.7f, .9f, .8f),//lionfish
        new Vector3(.9f, 1f, 2.2f),//barracuda
        new Vector3(1.1f, 1.3f, 1.8f),//tuna
        new Vector3(2f, 2.5f, 3.4f),//hammerhead
        new Vector3(1f, 1.6f, 2.6f),//tiger shark
        new Vector3(3f, 2.6f, 5f),//white shark
    };


    void Awake() {
        growthScale = transform.localScale;
        lightIntensity = fruitLight.intensity;
        fruitMaterial = fruitRender.material; // get the material from the renderer
        ReGrowth();
    }

    // Update is called once per frame
    void Update() {
        const float MIN_MATERIAL_EMISSION = 0.2f; // minimum emission for the material

        if (transform.localScale.x < growthScale.x) {
            //fast light icreasing after threshold
            float progress = Mathf.Max(0, transform.localScale.x / growthScale.x - PROGGRESS_THRESHOLD) * PROGGRESS_MULTIPLIER; // [0;1] gr after threshold

            transform.localScale += Vector3.one * GROWTH_SPEED * DJTime.DeltaTime;
            fruitLight.intensity = Mathf.Lerp(fruitLight.intensity, lightIntensity, progress);
            fruitMaterial.SetColor("_EmissionColor", Color.blue * Mathf.Max(MIN_MATERIAL_EMISSION, progress) * MATERIAL_EMISSION_SCALE);
        }
        else if (!fruitCollider.enabled)
            fruitCollider.enabled = true; //enable collider when fully grown
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        Transform parentFish = other.attachedRigidbody.transform;

        if (other.isTrigger && parentFish.CompareTag("Player") && GameStaticInfo.TryGetFish(parentFish.gameObject, out Fish fish)/*parentFish.TryGetComponent<Fish>(out Fish fish)*/
            && parentFish.TryGetComponent(out FishAnim animManager)) {

#if UNITY_ANDROID && !UNITY_EDITOR
            const float MIN_HEALTH_MULTIPLIER = 0.3f;//_TO_TRIGGER_MarineMedicine

            if (fish.Health <= fish./*MaxHealth*/DefaultInfo.MaxHealth * MIN_HEALTH_MULTIPLIER)
                GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_algae_are_beneficial_to_health);
#endif

            audioManager.PlayVFX(VFX_clip.Heal);
            //audioSource.PlayOneShot(healClip);
            fish.Heal(fish.DefaultInfo.MaxHealth / HEAL_DIVIDER);
            ReGrowth();
            animManager.ActivateBiteAnimation(false); // activate bite animation on fish

            //healParticle.transform.localScale = other.transform.localScale;
            //healParticle.transform.position = other.transform.position;
            //healParticle.SetActive(true);//spawn particles
            dishAddCompMangr.ActivateObjectFromPool(FishComponentType.PlayerHealParticle,
                other.transform,
                Vector3.zero,
                MulV3(fishesHealParticleScales[(int)fish.Level], other.transform.localScale),
                Quaternion.identity);
            //Instantiate(healParticle, other.transform).transform.localScale = MulV3(fishesHealParticleScales[(int)fish.Level], other.transform.localScale);
        }
    }

    Vector3 MulV3(Vector3 v, Vector3 v1) {
        return new Vector3(v.x * v1.x, v.y * v1.y, v.z * v1.z);
    }

    void ReGrowth() {
        const float INIT_SCALE_MULTIPLIER = 0.25f; // minimum scale for fruit

        transform.localScale = transform.localScale * INIT_SCALE_MULTIPLIER; //reset to quarter size
        fruitLight.intensity = 0f;
        fruitCollider.enabled = false; //disable collider until fully grown again
    }
}
