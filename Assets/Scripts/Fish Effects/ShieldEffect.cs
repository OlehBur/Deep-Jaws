using UnityEngine;

public class ShieldEffect : Effect {
    public GameObject ShieldObj {get; private set; }
    private FishAdditionalCompManager fishAddComp;
    private AudioManager audioManager;
    private Vector3 shieldLocalPos;
    private Vector3 shieldLocalScale;
    private Transform shieldParent;

    public ShieldEffect(AudioManager audioManager, FishAdditionalCompManager fishAddComp, Transform parent, Vector3 shieldLocPos, Vector3 shieldLocScale) {
        this.audioManager = audioManager;
        this.fishAddComp = fishAddComp;
        shieldLocalPos = shieldLocPos;
        shieldLocalScale = shieldLocScale;
        shieldParent = parent;
        //shieldObj = shield;
        type = FishEffectType.Shield;
    }

    protected override void OnApply() {
        //shieldObj?.SetActive(true);
        if (fishAddComp != null)
            ShieldObj = fishAddComp.ActivateObjectFromPool(FishComponentType.ShieldBubble, shieldParent, shieldLocalPos, shieldLocalScale, Quaternion.identity);
        audioManager?.PlayVFX(VFX_clip.BubblePop);
        //Debug.Log($"{target.name} has applied Shield Effect for {duration} s.");
    }

    protected override void OnExpire() {
        //Debug.Log($"{target.name} has expired Shield Effect after {duration} s.");
        audioManager?.PlayVFX(VFX_clip.BubblePop);
        if (ShieldObj != null)
            ShieldObj?.SetActive(false);
    }
}
