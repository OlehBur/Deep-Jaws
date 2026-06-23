using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class AccountsButtons {
    public Button buyNoAd;
    public Button buyPremium;
    public TextMeshProUGUI noAdTxt;
    public TextMeshProUGUI premiumTxt;
}

[Serializable]
public class SkinsButtons {
    public Button buyMercury;
    public Button selectMercury;
    public TextMeshProUGUI mercurySelectTxt;
    public Button buyChameleon;
    public Button selectChameleon;
    public TextMeshProUGUI chameleonSelectTxt;
    public Button buyAshflow;
    public Button selectAshflow;
    public TextMeshProUGUI ashflowSelectTxt;
}

public class StoreAnimator : MonoBehaviour {
    [SerializeField] Transform[] transforms;
    [SerializeField] AccountsButtons accountsButtons;
    [SerializeField] SkinsButtons skinsButtons;

    private BlinkAnim[] blinkAnims;


    void Start() {
        GamePurchaseManager.Instance.OnUpdatePurchasedSkins += UpdateSkinsSection;
        GamePurchaseManager.Instance.OnUpdateAccountTittles += UpdateAccountSection;
        InitBlinks();
    }

    void UpdateSkinsSection(PurchasedSkins skins) {

    }

    void UpdateAccountSection(AccountTitles accountTitles) {

    }

    private void Update() {
        //if (blinkAnims[0] != null)
        //for (int i = 0; i < blinkAnims.Length; i++)
        //    blinkAnims[i].Update();
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

        seed = Random.Range(0f, 100f);
    }

    public void Update() {
        if (objTransform.gameObject.activeInHierarchy) {
            float noiseScale = Mathf.PerlinNoise(seed, Time.time * scaleSpeed) + MIN_SCALE_MULT;
            objTransform.localScale = startScale * noiseScale * maxScale * Vector3.one;
        }
    }
}
