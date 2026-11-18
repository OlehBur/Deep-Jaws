
using System;

public interface ISceneLoader {
    event Action<string> OnNewSceneLoaded;

    void OnLoad_GameScene();
    void OnLoad_SanctuaryScene();
    void OnLoad_MainMenuScene();
    void LoadScene(SceneType sceneType);
}