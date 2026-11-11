using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public enum SceneType {
    MainMenu,
    Game,
    Loading,
    Sancturary
}

public interface ISceneLoader {
    event Action<string> OnNewSceneLoaded;

    void OnLoad_GameScene();
    void OnLoad_SanctuaryScene();
    void OnLoad_MainMenuScene();
    void LoadScene(SceneType sceneType);
}

public class SceneLoader : MonoBehaviour, ISceneLoader {
    [SerializeField] private GameObject fadeCanvasObject;
    [SerializeField] private Image fadeImage;

    [Inject] private readonly IAlertWindowsManager _alertWindowsManager;

    public event Action<string> OnNewSceneLoaded;

    private bool _isTransitioning = false;

    private const float TRANSPARENT_ALPHA = 0f;
    private const float OPAQUE_ALPHA = 1f;
    private const float FADE_SPEED = 1.5f;


    private void Awake() {
        // hide on game start
        if (fadeCanvasObject != null)
            fadeCanvasObject.SetActive(false);
    }

    public void OnLoad_GameScene() => LoadScene(SceneType.Game);
    public void OnLoad_SanctuaryScene() => LoadScene(SceneType.Sancturary);
    public void OnLoad_MainMenuScene() => LoadScene(SceneType.MainMenu);

    public void LoadScene(SceneType sceneType) {
        if (_isTransitioning)
            return;

        StartCoroutine(TransitionRoutine(sceneType));
    }

    private IEnumerator TransitionRoutine(SceneType sceneToLoad) {
        _isTransitioning = true;

        if (fadeCanvasObject != null)
            fadeCanvasObject.SetActive(true);

        // to black
        yield return FadeImageAlpha(TRANSPARENT_ALPHA, OPAQUE_ALPHA);
        
        _alertWindowsManager.ActivateLoadingPanelWnd(true);

        AudioListener.volume = 0f;
        string sceneName = GetSceneName(sceneToLoad);
        //async scene load 
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) {
            yield return null;
        }

        AudioListener.volume = 1f;
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone) {
            yield return null;
        }

        OnNewSceneLoaded?.Invoke(sceneName);

        if (sceneToLoad == SceneType.Game) {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            Scene loadingScene = SceneManager.GetSceneByName(GameStaticInfo.LOADING_SCENE_NAME);
            if (loadingScene.isLoaded)
                SceneManager.UnloadSceneAsync(loadingScene);
        }

        _alertWindowsManager.ActivateLoadingPanelWnd(false);

        //to transparent
        yield return FadeImageAlpha(OPAQUE_ALPHA, TRANSPARENT_ALPHA);

        if (fadeCanvasObject != null)
            fadeCanvasObject.SetActive(false);

        _isTransitioning = false;
    }

    private IEnumerator FadeImageAlpha(float startAlpha, float targetAlpha) {
        if (fadeImage == null)
            yield break;

        Color color = fadeImage.color;
        color.a = startAlpha;
        fadeImage.color = color;

        while (!Mathf.Approximately(fadeImage.color.a, targetAlpha)) {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, DJTime.UnscaledDeltaTime * FADE_SPEED);
            fadeImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
    }

    private string GetSceneName(SceneType sceneType) {
        return sceneType switch {
            SceneType.MainMenu => GameStaticInfo.MAINMENU_SCENE_NAME,
            SceneType.Game => GameStaticInfo.GAME_SCENE_NAME,
            SceneType.Loading => GameStaticInfo.LOADING_SCENE_NAME,
            SceneType.Sancturary => GameStaticInfo.SANCTUARY_SCENE_NAME,
            _ => GameStaticInfo.MAINMENU_SCENE_NAME
        };
    }
}