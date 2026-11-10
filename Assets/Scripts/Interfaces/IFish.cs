using UnityEngine;


public enum EnemyState {
    Walking,
    Escape,
    Attack
}

public interface IFish {
    /// <summary>
    /// Time spent in idle state
    /// </summary>
    //float IdleTime { get; }
    /// <summary>
    /// Time after which fish can stop
    /// </summary>
    //float TimeToIdle { get; }
    EnemyState State { get; }
    //the fish relative to which the reaction occurs
    Fish CurrentTarget { get; }

    void FishProximity(Fish otherFish);
    /// <summary>
    /// returns non normalize direction vector for movement
    /// </summary>
    /// <returns></returns>
    Vector3 Walking();
    Vector3 Escape();
    /// <summary>
    /// unlike walking and escape, does not store the target position, since the victim will definitely run away
    /// </summary>
    /// <returns></returns>
    Vector3 Attack();
}