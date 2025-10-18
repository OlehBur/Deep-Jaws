using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType {
    MainMenu,
    Game,
    Loading,
    Null
}

public static class SceneFlagHolder {
    public static SceneType nextScene { get; set; } = SceneType.MainMenu;
}

public class FaderScene : MonoBehaviour {
    [SerializeField] Image image;
    [SerializeField] SceneType loadScene;
    [SerializeField] bool isNeedInitFade = true;

    private const float TRANSPARANT_ALPHA = 0f;
    private const float UNTRANSPARANT_ALPHA = 1f;
    private const float FADING_SPEED = .025f;
    private const float FADE_EPSILON = .01f;
    private bool isLoadSceneStarted = false;
    //private static FaderScene instance { get; set; }


    //void Awake() {
    //    //instance = this;
    //    //DontDestroyOnLoad(gameObject);

    //    //SceneManager.sceneLoaded += OnSceneLoaded;

    //}

    //At the first start of the scene, the active and black object that gradually
    //becomes transparent, after which, when re -switched on, it loads the scene selected through Bul and darkens
    void Update() {
        if (!isLoadSceneStarted)
            if (isNeedInitFade) {//from black to transparent
                image.color = new Color(0f, 0f, 0f, Mathf.Lerp(image.color.a, TRANSPARANT_ALPHA, Time.unscaledTime * FADING_SPEED));

                if (image.color.a < FADE_EPSILON) {
                    gameObject.SetActive(false);
                    isNeedInitFade = false;
                }
            }
            else {
                image.color = new Color(0f, 0f, 0f, Mathf.Lerp(image.color.a, UNTRANSPARANT_ALPHA, Time.unscaledTime * FADING_SPEED));

                if (Mathf.Abs(UNTRANSPARANT_ALPHA - image.color.a) < FADE_EPSILON) {
                    StartCoroutine(Load());
                    isLoadSceneStarted = true;
                }

            }
    }

    //void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    //    if (isInitFade)
    //        return;

    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //    Destroy(gameObject);
    //}

    //void OnDestroy() {
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    IEnumerator Load() {
        AudioListener.volume = 0f; // disable all sounds

        //save type of the scene to load if it is not a null intermediate scene
        if (loadScene == SceneType.Null)
            loadScene = SceneFlagHolder.nextScene;
        else {
            SceneFlagHolder.nextScene = loadScene;
            loadScene = SceneType.Loading;
        }

        string sceneName = loadScene switch {
            SceneType.MainMenu => MainmenuManager.MAINMENU_SCENE_NAME,
            SceneType.Game => MainmenuManager.GAME_SCENE_NAME,
            SceneType.Loading => MainmenuManager.LOADING_SCENE_NAME,
            _ => MainmenuManager.MAINMENU_SCENE_NAME
        };

        AsyncOperation asyncLoad = (loadScene == SceneType.Game) ?
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive) :
            SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (/*!asyncLoad.isDone || */(loadScene == SceneType.Game && !GameManager.IsSceneReady)) {
            yield return null;
        }

        if (loadScene == SceneType.Game) {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            SceneManager.UnloadSceneAsync(MainmenuManager.LOADING_SCENE_NAME);
        }
        AudioListener.volume = 1f; // enable
    }

}
