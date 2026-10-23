using System;
using System.Collections.Generic;
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


public abstract class Effect {
    /// <summary>
    /// value of effect duration in seconds (must be > 1s, because beacause)
    /// </summary>
    public float Duration { get; private set; }
    public FishEffectType Type { get; private set; }

    public float LeftTime { get; private set; } // remaining time of the effect
    protected Fish targetFish;
    public event Action<FishEffectType> OnEffeñtExpire;
    protected const float NORMAL_SPEED_MULTIPLIER = 1f;

    public bool Enabled { get; set; } = true;


    public void Init(Fish target, float duration) {
        targetFish = target;
        Duration = duration;
        LeftTime = duration;

        //if (type == FishEffectType.FreezeTime)
        //    Debug.Log(LeftTime);

        OnApply();
    }

    public virtual void UpdateTimer() {
        if (DJTime.TimeScale == 0f)//in pause menu effect must be freezed
            return;

        float delta = Type == FishEffectType.FreezeTime ?
            DJTime.UnscaledDeltaTime : DJTime.DeltaTime;

        LeftTime -= delta/*deltaTime*/;

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
    public void DeactivateEffect() {
        //const float DESTROY_DELAY_TIME = 0.05f; // delay to avoid issues with OnDestroy calling before OnExpire
        //isStartDeactivate = true;
        OnExpire();
        OnEffeñtExpire?.Invoke(Type);
    }
}