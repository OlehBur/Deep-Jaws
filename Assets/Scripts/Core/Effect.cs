using System;
using UnityEngine;

public enum FishEffectType {//pos or neg (+/-)
    None = -1,
    Shield,             //+
    CurrentSpeed,       //+
    Stun,               //-
    StunInitiator,      //+
    SprintSpeed,        //+
    FreezeTime,         //+
    Poison,             //-
    Blindness           //-
}

/// <summary>
/// no(fish effect Component)
/// </summary>
public abstract class Effect /*: MonoBehaviour*/ {
    /// <summary>
    /// value of effect duration in seconds (must be > 1s, because beacause)
    /// </summary>
    public float Duration { get; private set; }
    protected FishEffectType type;
    //protected bool isStartDeactivate = false;

    public float LeftTime { get; private set; } // remaining time of the effect
    protected Fish target;
    public event Action<FishEffectType> OnEffeñtExpire;
    protected const float NORMAL_SPEED_MULTIPLIER = 1f;
    //private bool enabled = true;
    public bool Enabled { get; set; } = true;
    //public bool Enabled {
    //    get { return enabled; }
    //    set {
    //        enabled = value;

    //        if (!value)
    //            DeactivateEffect();
    //    }
    //}


    public void Init(Fish target, float duration) {
        this.target = target;
        Duration = duration;
        LeftTime = duration;

        //if (type == FishEffectType.FreezeTime)
        //    Debug.Log(LeftTime);

        OnApply();
    }

    public virtual void UpdateTimer() {
        if (Time.timeScale == 0f)//in pause menu effect must be freezed
            return;

        LeftTime -= Time.unscaledDeltaTime/*deltaTime*/;

        if (LeftTime <= 0f || Enabled == false) {
            Enabled = false;
            DeactivateEffect();
        }
    }

    public void Refresh(float duration) {
        //if (LeftTime < 0f)
        //    OnApply();

        Duration = duration;
        LeftTime = /*Duration*/duration;
    }

    protected abstract void OnApply();
    protected abstract void OnExpire();

    //private void OnDestroy() {
    //    //Debug.LogWarning("Effect OnDestroy called");
    //    if (!isStartDeactivate) // if the player death or etc. is close before effect duratuin time , then just call DeactivateEffect
    //        DeactivateEffect();
    //}

    /// <summary>
    /// dont use by user, only after onenable = false set
    /// </summary>
    private void DeactivateEffect() {
        //const float DESTROY_DELAY_TIME = 0.05f; // delay to avoid issues with OnDestroy calling before OnExpire
        //isStartDeactivate = true;
        OnExpire();
        OnEffeñtExpire?.Invoke(type);
    }
}