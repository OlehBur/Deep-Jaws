using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipsInfo", menuName = "Scriptable Objects/AudioClipsInfo")]
public class AudioClipsInfo : ScriptableObject {
    //music clips
    [field: SerializeField] public AudioClip MainMenu_Clip { get; set; }
    [field: SerializeField] public AudioClip Sanctuary_Clip { get; set; }
    [field: SerializeField] public AudioClip Game_CoralRocks_Clip { get; set; }
    [field: SerializeField] public AudioClip Game_AlgaeForest_Clip { get; set; }
    [field: SerializeField] public AudioClip Game_DeepCorals_Clip { get; set; }

    //vfx clips
    [field: SerializeField] public AudioClip PearlSoundClip { get; set; }
    [field: SerializeField] public AudioClip FishBiteClip { get; set; }
    //[field:SerializeField] public AudioClip fishCurrentClip{get;set;}
    [field: SerializeField] public AudioClip HydraBiteClip { get; set; }
    [field: SerializeField] public AudioClip ShipHornClip { get; set; }
    [field: SerializeField] public AudioClip CuttlefishClip { get; set; }
    [field: SerializeField] public AudioClip PufferfishClip { get; set; }
    [field: SerializeField] public AudioClip Damage { get; set; }
    [field: SerializeField] public AudioClip Heal { get; set; }
    [field: SerializeField] public AudioClip EchoAqua { get; set; }
    [field: SerializeField] public AudioClip BubblePop { get; set; }
    [field: SerializeField] public AudioClip UI_buttonClick { get; set; }
}
