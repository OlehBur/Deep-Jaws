using System;
using UnityEngine;


public class PlayerLeaderboardScore : MonoBehaviour {
    private SpawnManager spawnManager;
    private Fish player;

    public static long Score { get; private set; } = 0;
    public static event Action<long> OnChanged;

    private static int EasyDeaths { get; set; }
    private const string DEATHS_KEY = "Deaths";


    public void Init(SpawnManager spawnManager) {
        this.spawnManager = spawnManager;
        PlayerReconnect(spawnManager.PlayerFish, false);
        spawnManager.OnPlayerRespawn += PlayerReconnect;
        EasyDeaths = PlayerPrefs.GetInt(DEATHS_KEY, 0);
        //Debug.LogWarning("Deaths: " + EasyDeaths);
    }

    void PlayerReconnect(Fish player, bool isDie) {
        //else
        //Debug.LogWarning("Player ref: "+player);

        if (isDie) {
            if (/*this.player != null &&*/ this.player != player) {//not same fish & not first init (is die never = true on first init)
                this.player.OnFishEat -= OnScoreAdd;
                this.player = player;
                this.player.OnFishEat += OnScoreAdd;
            }

            if (GameStaticInfo.Gamemode == GameMode.Easy) {
                EasyDeaths++;
                GPG_Manager.Instance?.OnSavePlayerDeathsToLeaderBoard(EasyDeaths);
            }
            else if (GameStaticInfo.Gamemode == GameMode.Hard) {
                GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), Score);
                Score = 0; // Reset score when the player is disabled
                           //Debug.Log("LeadBoardScore PlayerReconnect");
            }
        }
        else {
            if (this.player == null) {//first init 
                this.player = player;
                this.player.OnFishEat += OnScoreAdd;
            }
            else {//new fish (lvl up)   this.player != null
                this.player.OnFishEat -= OnScoreAdd;
                this.player = player;
                this.player.OnFishEat += OnScoreAdd;
            }
        }
    }

    public static void LoadSavedData() =>
        EasyDeaths = PlayerPrefs.GetInt(DEATHS_KEY, 0);


    public static void SaveLoadedData() =>
        PlayerPrefs.SetInt(DEATHS_KEY, EasyDeaths);


    private void OnScoreAdd(int score) {
        Score += score;
        //Debug.Log($"Player score: {this.score}");

        OnChanged?.Invoke(Score);
    }

    public void OnPauseMenuOpen() => //save score when the pause menu is opened
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), Score);
    //Score = 0; // Reset score when the player is disabled
    //Debug.Log("LeadBoardScore OnDisable");

    public void OnResetScore() => Score = 0;

    private void OnDisable/*OnApplicationQuit*/() {
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), Score);
        Score = 0; // Reset score when the player is disabled
        //Debug.Log("OnApplicationQuit");
    }

    private void OnDestroy() {
        PlayerPrefs.SetInt(DEATHS_KEY, EasyDeaths);
        //Debug.LogWarning("Deaths: " + easyDeaths);

        if (player != null)
            player.OnFishEat -= OnScoreAdd;
        spawnManager.OnPlayerRespawn -= PlayerReconnect;
    }

    /*LederboardKey*/
    string GetLederboardByGameMode() {
        return GameStaticInfo.Gamemode switch {
            GameMode.Easy => GPGSIds.leaderboard_harbor_breeze_appetite,
            GameMode.Medium => GPGSIds.leaderboard_ocean_voyage_appetite,
            GameMode.Hard => GPGSIds.leaderboard_deadly_maelstrom_appetite,
            _ => GPGSIds.leaderboard_harbor_breeze_appetite,
        };
    }
}
