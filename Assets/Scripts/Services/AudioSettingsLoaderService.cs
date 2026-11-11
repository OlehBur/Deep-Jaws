


public interface IAudioSettingsLoader {
    AudioVolumes Load();
}

public struct AudioVolumes {
    public float MasterVolume;
    public float MusicVolume;
    public float SFXVolume;
}

public class AudioSettingsLoader : IAudioSettingsLoader {
    public AudioVolumes Load() {
        AudioVolumes av;
        SettingsData data =
            ProgressSaver.IsSettingsDataExist
            ? ProgressSaver.LoadSettingsData()
            : new SettingsData();

        av.MasterVolume =
            GameStaticInfo.SliderToDB(
                data.volume_master,
                GameStaticInfo.MAX_MASTER_DB);

        av.MusicVolume =
            GameStaticInfo.SliderToDB(
                data.volume_music,
                GameStaticInfo.MAX_MUSIC_DB);

        av.SFXVolume =
            GameStaticInfo.SliderToDB(
                data.volume_sfx,
                GameStaticInfo.MAX_SFX_DB);

        return av;
    }
}