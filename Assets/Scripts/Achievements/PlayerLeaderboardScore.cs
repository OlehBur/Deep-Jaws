using UnityEngine;


public class PlayerLeaderboardScore : MonoBehaviour {
    private SpawnManager spawnManager;
    private Fish player;
    private long score = 0;
    private int deaths = 0;

    public void Init(SpawnManager spawnManager) {
        this.spawnManager = spawnManager;
        PlayerReconnect(/*spawnManager.pool[SpawnManager.PLAYER_INDEX].GetComponent<Fish>()*/spawnManager.PlayerFish, false);
        spawnManager.OnPlayerRespawn += PlayerReconnect;
    }

    void PlayerReconnect(Fish player, bool isDie) {
        this.player = player;

        if (player != null)
            player.OnFishEat -= OnScoreAdd;
        //else
        //Debug.LogWarning("Player ref: "+player);

        player.OnFishEat += OnScoreAdd;

        if (isDie) {
            deaths++;

            if (GameStaticInfo.Gamemode == GameMode.Hard) {
                GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), score, deaths);
                score = 0; // Reset score when the player is disabled
                           //Debug.Log("LeadBoardScore PlayerReconnect");
            }
        }
    }

    private void OnScoreAdd(int score) {
        this.score += score;
        //Debug.Log($"Player score: {this.score}");
    }

    public void OnPauseMenuOpen() { //save score when the pause menu is opened
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), score, deaths);
        score = 0; // Reset score when the player is disabled
        //Debug.Log("LeadBoardScore OnDisable");
    }

    private void OnApplicationQuit() {
        GPG_Manager.Instance?.OnSavePlayerScoreToLeaderBoard(GetLederboardByGameMode(), score, deaths);
        score = 0; // Reset score when the player is disabled
        //Debug.Log("OnApplicationQuit");
    }

    private void OnDestroy() {
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
