using System.Collections;
using UnityEngine;

public class SoundtrackController : MonoBehaviour {
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] AudioSource soundtrackSource;
    [SerializeField] AudioClip coralRocksSound;
    [SerializeField] AudioClip algaeForestSound;
    [SerializeField] AudioClip deepCoralsSound;

    private Coroutine fadeVolumeChangeSoundCoroutine = null;
    //private AudioClip targetClip;

    private const FishLevel ALGAE_FOREST_TRIGGER = FishLevel.Moray;
    private const FishLevel DEEP_CORALS_TRIGGER = FishLevel.HammerShark;
    private const float SOUND_CHANGE_SPEED_MULT = 1 / 6f; //s


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        spawnManager.OnPlayerRespawn += ActivateNewTargerSong;
        if (spawnManager.PlayerCurrentFish != null)//initial start mus
            ActivateNewTargerSong(spawnManager.PlayerCurrentFish, false);
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

        if (fadeVolumeChangeSoundCoroutine != null)
            StopCoroutine(fadeVolumeChangeSoundCoroutine);

        fadeVolumeChangeSoundCoroutine = StartCoroutine(SmoothSoundChange(target));
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
