using System.Collections;
using UnityEngine;
using VContainer;

public class SoundtrackController : MonoBehaviour {
    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;
    [Inject] private readonly IAudioPlayService _audioPlayService;

    private Coroutine _fadeVolumeChangeSoundCoroutine = null;

    private const FishLevel ALGAE_FOREST_TRIGGER = FishLevel.Moray;
    private const FishLevel DEEP_CORALS_TRIGGER = FishLevel.HammerShark;
    private const float SOUND_CHANGE_SPEED_MULT = 1 / 6f; //s


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _playerFishPoolService.OnPlayerRespawn += ActivateNewTargerSong;

        if (_playerFishPoolService.PlayerCurrentFish != null)//initial start mus
            ActivateNewTargerSong(_playerFishPoolService.PlayerCurrentFish, false);
    }

    void ActivateNewTargerSong(Fish fish, bool isDeath) {
        Music_clip targetClip;

        if (fish.Level >= DEEP_CORALS_TRIGGER)
            targetClip = Music_clip.Game_DeepCorals;
        else if (fish.Level >= ALGAE_FOREST_TRIGGER)
            targetClip = Music_clip.Game_AlgaeForest;
        else
            targetClip = Music_clip.Game_CoralRocks;
        if (targetClip == _audioPlayService.CurrentMusicClip)
            return;

        if (_fadeVolumeChangeSoundCoroutine != null)
            StopCoroutine(_fadeVolumeChangeSoundCoroutine);

        _fadeVolumeChangeSoundCoroutine = StartCoroutine(SmoothSoundChange(targetClip));
    }

    IEnumerator SmoothSoundChange(Music_clip target) {
        while (_audioPlayService.MusicSourceVolume > 0 && _audioPlayService.CurrentMusicClip != Music_clip.None) {//for first call - skip volume fade
            _audioPlayService.MusicSourceVolume -= DJTime.UnscaledDeltaTime * SOUND_CHANGE_SPEED_MULT;
            yield return null;
        }

        _audioPlayService.PlayMusic(target);

        while (_audioPlayService.MusicSourceVolume < 1) {
            _audioPlayService.MusicSourceVolume += DJTime.UnscaledDeltaTime * SOUND_CHANGE_SPEED_MULT;
            yield return null;
        }
    }
}
