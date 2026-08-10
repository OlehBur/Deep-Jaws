using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StoreAnimator : MonoBehaviour {
    [SerializeField] Transform[] transforms;
   
    private BlinkAnim[] blinkAnims;


    void Start() {
        InitBlinks();
    }

    private void Update() {
        foreach (BlinkAnim blAnim in blinkAnims)
            blAnim.Update();
    }

    public void InitBlinks() {
        int cnt = transforms.Length;

        blinkAnims = new BlinkAnim[cnt];
        for (int i = 0; i < cnt; i++)
            blinkAnims[i] = new BlinkAnim(transforms[i]);
    }
}

public class BlinkAnim {
    private float scaleSpeed = .6f;
    private float maxScale = 1.2f;

    private readonly float startScale;
    private Transform objTransform;
    private float seed;

    private const float MIN_SCALE_MULT = .1f;

    public BlinkAnim(Transform transform) {
        this.objTransform = transform;
        startScale = transform.localScale.x;

        seed = UnityEngine.Random.Range(0f, 100f);
    }

    public void Update() {
        if (objTransform.gameObject.activeInHierarchy) {
            float noiseScale = Mathf.PerlinNoise(seed, Time.time * scaleSpeed) + MIN_SCALE_MULT;
            objTransform.localScale = startScale * noiseScale * maxScale * Vector3.one;
        }
    }
}
