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