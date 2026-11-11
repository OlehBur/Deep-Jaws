using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;


// helper class for safe language loading 
public class LocalizationStartupRunner : MonoBehaviour {//genius of Unity - is that it only works with MonoBeh (((
    private IEnumerator Start() {
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist
            ? ProgressSaver.LoadSettingsData()
            : new SettingsData();

        // wait for Unity Localization in frame mode (yield return instead of await)
        yield return LocalizationSettings.InitializationOperation;

        int localeIndex = savedSettings.language;
        if (localeIndex >= 0 && localeIndex < LocalizationSettings.AvailableLocales.Locales.Count) {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
            Debug.Log($"[LocalizationStartup] Successfully loaded locale index: {localeIndex}");
        }

        // self-destruct so as not to litter the memory
        Destroy(gameObject);
    }
}