using log4net.Filter;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;


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
    BubblePop,
    UI_ButtonClick,
    SanctuaryResearch
}

public enum Music_clip {
    None,
    MainMenu,
    Sanctuary,
    Game_CoralRocks,
    Game_AlgaeForest,
    Game_DeepCorals
}


public interface IAudioPlayService {
    float MasterVolume { get; set; }
    float MusicVolume { get; set; }
    float SFXVolume { get; set; }
    float MusicSourceVolume { get; set; }
    float PlayerSourceVolume { get; set; }
    Music_clip CurrentMusicClip { get; }

    void PlayVFX(VFX_clip clipType);
    void PlayMusic(Music_clip clipType);

    void StopPlayerSprintAudioSource();
    void PlayPlayerSprintAudioSource();
}


/// <summary>
/// read and write audio settings, save and load them, update audio mixer parameters
/// </summary>
public class AudioPlayManager : MonoBehaviour, IAudioPlayService {
    [field: SerializeField] public AudioSource MusicAudioSource { get; private set; }
    [field: SerializeField] public AudioSource VFXAudioSource { get; private set; }
    [field: SerializeField] public AudioSource PlayerAudioSource { get; private set; }

    [Inject] private readonly AudioClipsInfo _clipsInfo;
    [Inject] private readonly AudioMixer _audioMixer;
    [Inject] private readonly ISceneLoader _sceneLoader;
    [Inject] IAudioSettingsLoader _audioSettingsLoader;

    public Music_clip CurrentMusicClip { get; private set; }
    public float MusicSourceVolume {
        get {
            return MusicAudioSource.volume;
        }
        set {
            MusicAudioSource.volume = value;
        }
    }
    public float PlayerSourceVolume {
        get {
            return PlayerAudioSource.volume;
        }
        set {
            PlayerAudioSource.volume = value;
        }
    }

    void Start() {
        LoadVolumeSettings();
        InitPlayerAudioSource();

        _sceneLoader.OnNewSceneLoaded += SceneChangeHandler;
        SceneChangeHandler(GameStaticInfo.MAINMENU_SCENE_NAME);//by default main menu        
    }

    void InitPlayerAudioSource() {
        if (PlayerAudioSource.clip == null)
            Debug.LogWarning("Player AudioSource should have a current sound clip by default");
        PlayerAudioSource.Stop();
        PlayerAudioSource.volume = 0f;
    }

    void LoadVolumeSettings() {
        AudioVolumes av = _audioSettingsLoader.Load();

        MasterVolume = av.MasterVolume;
        SFXVolume = av.SFXVolume;
        MusicVolume = av.MusicVolume;
    }

    void OnDestroy() {
        if (_sceneLoader != null)
            _sceneLoader.OnNewSceneLoaded -= SceneChangeHandler;
    }

    void SceneChangeHandler(string sceneName) {
        switch (sceneName) {
            case GameStaticInfo.GAME_SCENE_NAME:
            PlayMusic(Music_clip.Game_CoralRocks);
            break;
            case GameStaticInfo.MAINMENU_SCENE_NAME:
            PlayMusic(Music_clip.MainMenu);
            break;
            case GameStaticInfo.LOADING_SCENE_NAME:
            //PlayMusic(ClipsInfo.Game_CoralRocks_Clip);
            break;
            case GameStaticInfo.SANCTUARY_SCENE_NAME:
            PlayMusic(Music_clip.Sanctuary);
            break;
        }

        MusicAudioSource.volume = 1.0f;
        VFXAudioSource.volume = 1.0f;
        PlayerAudioSource.volume = 1.0f;
    }

    public void PlayVFX(VFX_clip clipType) {
        AudioClip audioClip = clipType switch {
            VFX_clip.FreezeTime => _clipsInfo.PearlSoundClip,
            VFX_clip.Bite => _clipsInfo.FishBiteClip,
            //VFX_clip.Current => _clipsInfo.FishCurrentClip,
            VFX_clip.Hydra => _clipsInfo.HydraBiteClip,
            VFX_clip.Jellyfish => _clipsInfo.HydraBiteClip,
            VFX_clip.Ship => _clipsInfo.ShipHornClip,
            VFX_clip.Cuttlefish => _clipsInfo.CuttlefishClip,
            VFX_clip.Pufferfish => _clipsInfo.PufferfishClip,
            VFX_clip.Damage => _clipsInfo.Damage,
            VFX_clip.Heal => _clipsInfo.Heal,
            VFX_clip.Echo => _clipsInfo.EchoAqua,
            VFX_clip.BubblePop => _clipsInfo.BubblePop,
            VFX_clip.UI_ButtonClick => _clipsInfo.UI_buttonClick,
            VFX_clip.SanctuaryResearch => _clipsInfo.SanctuaryResearch,
            _ => _clipsInfo.Damage
        };

        if (VFXAudioSource != null)
            VFXAudioSource.PlayOneShot(audioClip);
    }

    public void PlayMusic(Music_clip clipType) {
        if (MusicAudioSource != null) {
            AudioClip audioClip = clipType switch {
                Music_clip.MainMenu => _clipsInfo.MainMenu_Clip,
                Music_clip.Sanctuary => _clipsInfo.Sanctuary_Clip,
                Music_clip.Game_CoralRocks => _clipsInfo.Game_CoralRocks_Clip,
                Music_clip.Game_AlgaeForest => _clipsInfo.Game_AlgaeForest_Clip,
                Music_clip.Game_DeepCorals => _clipsInfo.Game_DeepCorals_Clip,
                _ => _clipsInfo.Game_CoralRocks_Clip
            };

            CurrentMusicClip = clipType;

            MusicAudioSource.clip = audioClip;
            MusicAudioSource.loop = true;
            MusicAudioSource.Play();
        }
    }
    public void StopPlayerSprintAudioSource() =>
        PlayerAudioSource.Stop();

    public void PlayPlayerSprintAudioSource() =>
        PlayerAudioSource.Play();

    public float MasterVolume {
        get {
            _audioMixer.GetFloat(GameStaticInfo.AUDIO_MASTER_VOLUME_PARAM, out float value);
            return value;
        }
        set {
            _audioMixer.SetFloat(GameStaticInfo.AUDIO_MASTER_VOLUME_PARAM, value);
        }
    }

    public float MusicVolume {
        get {
            _audioMixer.GetFloat(GameStaticInfo.AUDIO_MUSIC_VOLUME_PARAM, out float value);
            return value;
        }
        set {
            _audioMixer.SetFloat(GameStaticInfo.AUDIO_MUSIC_VOLUME_PARAM, value);
        }
    }

    public float SFXVolume {
        get {
            _audioMixer.GetFloat(GameStaticInfo.AUDIO_VFX_VOLUME_PARAM, out float value);
            return value;
        }
        set {
            _audioMixer.SetFloat(GameStaticInfo.AUDIO_VFX_VOLUME_PARAM, value);
        }
    }



}
