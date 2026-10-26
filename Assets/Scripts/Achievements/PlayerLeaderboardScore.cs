using System;
using UnityEngine;
using VContainer;


public class PlayerLeaderboardScore : MonoBehaviour {
    [Inject] private readonly IPlayerFishPoolService _playerPoolService;

    private Fish _player;

    public static long Score { get; private set; } = 0;
    private static int EasyDeaths { get; set; }
    public static event Action<long> OnChanged;

    private const string DEATHS_KEY = "Deaths";



    void Start() {
        //PlayerReconnect(_playerPoolService.PlayerCurrentFish, false);
        _playerPoolService.OnPlayerRespawn += PlayerReconnect;
        EasyDeaths = PlayerPrefs.GetInt(DEATHS_KEY, 0);
        //Debug.LogWarning("Deaths: " + EasyDeaths);
    }

    void PlayerReconnect(Fish player, bool isDie) {
        //else
        //Debug.LogWarning("Player ref: "+player);

        if (isDie) {
            if (/*this.player != null &&*/ _player != player) {//not same fish & not first init (is die never = true on first init)
                _player.OnFishEat -= OnScoreAdd;
                _player = player;
                _player.OnFishEat += OnScoreAdd;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (GameStaticInfo.Gamemode == GameMode.Easy) {
                EasyDeaths++;
                GPG_Manager.Instance?.OnSavePlayerDeathsToLeaderBoard(EasyDeaths);
            }
            else if (GameStaticInfo.Gamemode == GameMode.Hard) {
                GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), Score);
                Score = 0; // Reset score when the player is disabled
                           //Debug.Log("LeadBoardScore PlayerReconnect");
            }
#endif
        }
        else {
            if (_player == null) {//first init 
                _player = player;
                _player.OnFishEat += OnScoreAdd;
            }
            else {//new fish (lvl up)   this.player != null
                _player.OnFishEat -= OnScoreAdd;
                _player = player;
                _player.OnFishEat += OnScoreAdd;
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

    public void OnPauseMenuOpen() {//save score when the pause menu is opened
#if UNITY_ANDROID && !UNITY_EDITOR
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), Score);
    //Score = 0; // Reset score when the player is disabled
    //Debug.Log("LeadBoardScore OnDisable");
#endif
    }

    public void OnResetScore() => Score = 0;

    private void OnDisable/*OnApplicationQuit*/() {
#if UNITY_ANDROID && !UNITY_EDITOR
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), Score);
        Score = 0; // Reset score when the player is disabled
        //Debug.Log("OnApplicationQuit");
#endif
    }

    private void OnDestroy() {
        PlayerPrefs.SetInt(DEATHS_KEY, EasyDeaths);
        //Debug.LogWarning("Deaths: " + easyDeaths);

        if (_player != null)
            _player.OnFishEat -= OnScoreAdd;
        if (_playerPoolService != null)
            _playerPoolService.OnPlayerRespawn -= PlayerReconnect;
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
