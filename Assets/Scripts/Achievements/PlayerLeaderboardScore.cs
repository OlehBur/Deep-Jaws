using UnityEngine;


public class PlayerLeaderboardScore : MonoBehaviour {
    private SpawnManager spawnManager;
    private Fish player;
    private long score = 0;
    private static int EasyDeaths { get; set; }
    private const string DEATHS_KEY = "Deaths";


    public void Init(SpawnManager spawnManager) {
        this.spawnManager = spawnManager;
        PlayerReconnect(/*spawnManager.pool[SpawnManager.PLAYER_INDEX].GetComponent<Fish>()*/spawnManager.PlayerFish, false);
        spawnManager.OnPlayerRespawn += PlayerReconnect;
        EasyDeaths = PlayerPrefs.GetInt(DEATHS_KEY, 0);
        //Debug.LogWarning("Deaths: " + EasyDeaths);
    }

    void PlayerReconnect(Fish player, bool isDie) {
        if (this.player != null)
            this.player.OnFishEat -= OnScoreAdd;
        this.player = player;
        //else
        //Debug.LogWarning("Player ref: "+player);

        if (isDie) {
            if (GameStaticInfo.Gamemode == GameMode.Easy) {
                EasyDeaths++;
                GPG_Manager.Instance?.OnSavePlayerDeathsToLeaderBoard(EasyDeaths);
            }
            else if (GameStaticInfo.Gamemode == GameMode.Hard) {
                GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), score);
                score = 0; // Reset score when the player is disabled
                           //Debug.Log("LeadBoardScore PlayerReconnect");
            }
        }
        else if (player != null)
            player.OnFishEat += OnScoreAdd;
    }

    public static void LoadSavedData() {
        EasyDeaths = PlayerPrefs.GetInt(DEATHS_KEY, 0);
    }

    public static void SaveLoadedData() {
        PlayerPrefs.SetInt(DEATHS_KEY, EasyDeaths);
    }

    private void OnScoreAdd(int score) {
        this.score += score;
        //Debug.Log($"Player score: {this.score}");
    }

    public void OnPauseMenuOpen() { //save score when the pause menu is opened
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), score);
        score = 0; // Reset score when the player is disabled
        //Debug.Log("LeadBoardScore OnDisable");
    }

    private void OnApplicationQuit() {
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), score);
        score = 0; // Reset score when the player is disabled
        //Debug.Log("OnApplicationQuit");
    }

    private void OnDestroy() {
        PlayerPrefs.SetInt(DEATHS_KEY, EasyDeaths);
        //Debug.LogWarning("Deaths: " + easyDeaths);

        if (player != null)
            player.OnFishEat -= OnScoreAdd;
        spawnManager.OnPlayerRespawn -= PlayerReconnect;
    }

    LederboardKey GetLederboardByGameMode() {
        return GameStaticInfo.Gamemode switch {
            GameMode.Easy => LederboardKey.EasyModeScore,
            GameMode.Medium => LederboardKey.MediumModeScore,
            GameMode.Hard => LederboardKey.HardModeScore,
            _ => LederboardKey.EasyModeScore,
        };
    }
}
