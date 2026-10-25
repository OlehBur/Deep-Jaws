using System.Collections;
using UnityEngine;
using VContainer;

public class SoundtrackController : MonoBehaviour {
    //[SerializeField] SpawnManager spawnManager;
    [SerializeField] AudioSource soundtrackSource;
    [SerializeField] AudioClip coralRocksSound;
    [SerializeField] AudioClip algaeForestSound;
    [SerializeField] AudioClip deepCoralsSound;

    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;

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

        AudioClip target;
        if (fish.Level >= DEEP_CORALS_TRIGGER)
            target = deepCoralsSound;
        else if (fish.Level >= ALGAE_FOREST_TRIGGER)
            target = algaeForestSound;
        else
            target = coralRocksSound;

        if (target == soundtrackSource.clip)
            return;

        if (_fadeVolumeChangeSoundCoroutine != null)
            StopCoroutine(_fadeVolumeChangeSoundCoroutine);

        _fadeVolumeChangeSoundCoroutine = StartCoroutine(SmoothSoundChange(target));
    }

    IEnumerator SmoothSoundChange(AudioClip target) {
        while (soundtrackSource.volume > 0 && soundtrackSource.clip != null) {//for first call - skip volume fade
            soundtrackSource.volume -= DJTime.UnscaledDeltaTime * SOUND_CHANGE_SPEED_MULT;
            yield return null;
        }

        soundtrackSource.clip = target;
        soundtrackSource.loop = true;
        soundtrackSource.Play();

        while (soundtrackSource.volume < 1) {
            soundtrackSource.volume += DJTime.UnscaledDeltaTime * SOUND_CHANGE_SPEED_MULT;
            yield return null;
        }
    }
}
