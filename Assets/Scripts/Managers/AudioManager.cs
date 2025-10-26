using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public enum VFX_clip {
    FreezeTime,
    Bite,
    //Current,
    Hydra,
    Jellyfish,
    Ship,
    Cuttlefish,
    Pufferfish,
    Damage,
    Heal,
    Echo,
    BubblePop
}

/// <summary>
/// read and write audio settings, save and load them, update audio mixer parameters
/// </summary>
public class AudioManager : MonoBehaviour {
    [SerializeField] AudioMixer audioMixer; // Reference to the AudioMixer asset
    [SerializeField] Slider masterSlider;// Slider for master volume control
    [SerializeField] Slider musicSlider;// Slider for music volume control
    [SerializeField] Slider sfxSlider;// Slider for SFX volume control
    private const float MAX_MASTER_DB = 1f;
    private const float MAX_MUSIC_DB = -26f;
    private const float MAX_SFX_DB = -10f;
    private const float MIN_DB = -80f;


    [SerializeField] AudioSource vfxGameAudioSource;
    public AudioSource playerRevolumeAudioSource;
    [SerializeField] AudioClip pearlSoundClip;
    [SerializeField] AudioClip fishBiteClip;
    //[SerializeField] AudioClip fishCurrentClip;
    [SerializeField] AudioClip hydraBiteClip;
    [SerializeField] AudioClip shipHornClip;
    [SerializeField] AudioClip cuttlefishClip;
    [SerializeField] AudioClip pufferfishClip;
    [SerializeField] AudioClip damage;
    [SerializeField] AudioClip heal;
    [SerializeField] AudioClip echoAqua;
    [SerializeField] AudioClip bubblePop;


    //save audio data
    private const string MASTER_KEY = "audio_master";
    private const string MUSIC_KEY = "audio_music";
    private const string SFX_KEY = "audio_sfx";

    public void PlayVFX(VFX_clip clipType) {
        AudioClip audioClip = clipType switch {
            VFX_clip.FreezeTime => pearlSoundClip,
            VFX_clip.Bite => fishBiteClip,
            //VFX_clip.Current => fishCurrentClip,
            VFX_clip.Hydra => hydraBiteClip,
            VFX_clip.Jellyfish => hydraBiteClip,
            VFX_clip.Ship => shipHornClip,
            VFX_clip.Cuttlefish => cuttlefishClip,
            VFX_clip.Pufferfish => pufferfishClip,
            VFX_clip.Damage => damage,
            VFX_clip.Heal => heal,
            VFX_clip.Echo => echoAqua,
            VFX_clip.BubblePop => bubblePop,
            _ => damage
        };

        vfxGameAudioSource.PlayOneShot(audioClip);
    }


    public float MasterVolume {
        get {
            audioMixer.GetFloat("MasterVol", out float value);
            return value;
        }
        private set {
            audioMixer.SetFloat("MasterVol", value);
        }
    }

    public float MusicVolume {
        get {
            audioMixer.GetFloat("MusicVol", out float value);
            return value;
        }
        private set {
            audioMixer.SetFloat("MusicVol", value);
        }
    }

    public float SFXVolume {
        get {
            audioMixer.GetFloat("SFXVol", out float value);
            return value;
        }
        private set {
            audioMixer.SetFloat("SFXVol", value);
        }
    }

    private float SliderToDB(float sliderValue, float maxDB) {
        const float CURVE_FACTOR = .2f; // Adjust this factor to change the curve steepness
        float curvedValue = Mathf.Pow(sliderValue, CURVE_FACTOR);

        return Mathf.Lerp(MIN_DB, maxDB, curvedValue);
    }

    //save audio data in normalize format
    public void SaveAudioData() {
        PlayerPrefs.SetFloat(MASTER_KEY, masterSlider.value);
        PlayerPrefs.SetFloat(MUSIC_KEY, musicSlider.value);
        PlayerPrefs.SetFloat(SFX_KEY, sfxSlider.value);
    }

    void LoadAudoData() {
        masterSlider.value = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        musicSlider.value = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        //UpdateVolumesBySliders();
    }

    //load audio data on start
    void OnEnable() {
        masterSlider.onValueChanged.AddListener(value => MasterVolume = SliderToDB(value, MAX_MASTER_DB));
        musicSlider.onValueChanged.AddListener(value =>
        {
            MusicVolume = SliderToDB(value, MAX_MUSIC_DB);

            if (value == 0f)
                GPG_Manager.OnTriggerAchievement(AchievementKey.GiveItBack);
        });
        sfxSlider.onValueChanged.AddListener(value => SFXVolume = SliderToDB(value, MAX_SFX_DB));
    }

    private void Start() {
        LoadAudoData();// In OnEnable and Awake, the mixer does not react to changes because for some reason it is still inactive in Unity.

    }
    //void UpdateVolumesBySliders() {
    //    MasterVolume = SliderToDB(masterSlider.value, MAX_MASTER_DB);
    //    Debug.Log(MasterVolume);
    //    MusicVolume = SliderToDB(musicSlider.value, MAX_MUSIC_DB);
    //    SFXVolume = SliderToDB(sfxSlider.value, MAX_SFX_DB);
    //}


    //save audio data on exit
    private void OnApplicationQuit() {
        SaveAudioData();
    }
    private void OnDisable() {
        SaveAudioData();
    }
}
