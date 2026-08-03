using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType {
    MainMenu,
    Game,
    Loading,
    Null,
    Sancturary
}

public class SceneLoader : MonoBehaviour {
    public static SceneType NextScene { get; set; } = SceneType.MainMenu;

    [SerializeField] private Image image;
    [SerializeField] private bool doFadeInOnStart = true; //for load scene always false 

    private bool isTransitioning = false;
    private SceneType targetScene;

    private const float TRANSPARENT_ALPHA = 0f;
    private const float OPAQUE_ALPHA = 1f;
    private const float FADE_SPEED = 1.5f;

    private void Awake() {
        isTransitioning = false;
    }

    private void Start() {
        //if (isTransitioning)
        //    return;

        if (doFadeInOnStart) 
            StartCoroutine(FadeInRoutine());
        else             //for loading scene
            LoadSceneWithoutFade(NextScene);
    }

    //fr black to transp
    private IEnumerator FadeInRoutine() {
        gameObject.SetActive(true);
        Color color = image.color;
        color.a = OPAQUE_ALPHA;
        image.color = color;

        while (image.color.a > TRANSPARENT_ALPHA) {
            color.a = Mathf.MoveTowards(color.a, TRANSPARENT_ALPHA, Time.unscaledDeltaTime * FADE_SPEED);
            image.color = color;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void OnLoad_GameScene() => LoadScene(SceneType.Game);
    public void OnLoad_SanctuaryScene() => LoadScene(SceneType.Sancturary);
    public void OnLoad_MainMenuScene() => LoadScene(SceneType.MainMenu);

    public void LoadScene(SceneType sceneType) {
        if (isTransitioning)
            return;

        isTransitioning = true;
        gameObject.SetActive(true);

        targetScene = sceneType;
        NextScene = targetScene;//save for load scene

        StartCoroutine(FadeOutAndLoadRoutine(SceneType.Loading));
    }

    //onl for load scene
    private void LoadSceneWithoutFade(SceneType sceneType) {
        if (isTransitioning)
            return;
        isTransitioning = true;

        Color color = image.color;//already black screen
        color.a = OPAQUE_ALPHA;
        image.color = color;
        gameObject.SetActive(true);

        StartCoroutine(LoadAsyncRoutine(sceneType));
    }

    // transp to black + new scene load
    private IEnumerator FadeOutAndLoadRoutine(SceneType sceneToLoad) {
        Color color = image.color;
        color.a = TRANSPARENT_ALPHA;
        image.color = color;

        while (image.color.a < OPAQUE_ALPHA) {
            color.a = Mathf.MoveTowards(color.a, OPAQUE_ALPHA, Time.unscaledDeltaTime * FADE_SPEED);
            image.color = color;
            yield return null;
        }

        yield return LoadAsyncRoutine(sceneToLoad);
    }

    private IEnumerator LoadAsyncRoutine(SceneType sceneToLoad) {
        string sceneName = sceneToLoad switch {
            SceneType.MainMenu => GameStaticInfo.MAINMENU_SCENE_NAME,
            SceneType.Game => GameStaticInfo.GAME_SCENE_NAME,
            SceneType.Loading => GameStaticInfo.LOADING_SCENE_NAME,
            SceneType.Sancturary => GameStaticInfo.SANCTUARY_SCENE_NAME,
            _ => GameStaticInfo.MAINMENU_SCENE_NAME
        };

        AsyncOperation asyncLoad = (sceneToLoad == SceneType.Game) ?
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive) :
            SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone || (sceneToLoad == SceneType.Game && !GameManager.IsSceneReady))
            yield return null;

        if (sceneToLoad == SceneType.Game) {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            SceneManager.UnloadSceneAsync(GameStaticInfo.LOADING_SCENE_NAME);
        }

        isTransitioning = false; 
    }
}