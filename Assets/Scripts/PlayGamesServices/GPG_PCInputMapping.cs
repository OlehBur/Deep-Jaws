using UnityEngine;
using Google.Play.InputMapping;
using System.Collections.Generic;
//#if PLAY_GAMES_PC
//using Google.Android.Libraries.Play.Games.Inputmapping;
//#endif

public enum GameActionId { SPRINT = 0, MOVE_FORWARD = 1, MOVE_LEFT = 2, MOVE_RIGHT = 3, MOVE_BACKWARD = 4 }
public enum GameGroupId { GAMEPLAY = 1 }


public class GPG_PCInputMapping : MonoBehaviour {
    void Start() {
        //#if PLAY_GAMES_PC
        if (GameManager.IsRunOnPC) {
            //  get the input mapping client from Google Play
            PlayInputMappingClient inputMappingClient = PlayInput.GetInputMappingClient();
            DJ_PlayGamesInputProvider myProvider = new DJ_PlayGamesInputProvider();
            // register it in the Google Play system
            inputMappingClient.SetInputMappingProvider(myProvider);
            Debug.Log("Google Play Input Mapping successfully registered!");
        }
        //#endif
    }
}

public class DJ_PlayGamesInputProvider : PlayInputMappingProvider {
    private PlayInputControls CreateSimpleControls(int androidKeyCode) {
        return PlayInputControls.Create(
            new List<int> { androidKeyCode },
            new List<PlayMouseAction>()
        );
    }

    public PlayInputMap OnProvideInputMap() {
        var moveUp = PlayInputAction.Create("Move Forward", (int)GameActionId.MOVE_FORWARD, CreateSimpleControls(AndroidKeyCode.KEYCODE_W));
        var moveLeft = PlayInputAction.Create("Move Left", (int)GameActionId.MOVE_LEFT, CreateSimpleControls(AndroidKeyCode.KEYCODE_A));
        var moveDown = PlayInputAction.Create("Move Backward", (int)GameActionId.MOVE_BACKWARD, CreateSimpleControls(AndroidKeyCode.KEYCODE_S));
        var moveRight = PlayInputAction.Create("Move Right", (int)GameActionId.MOVE_RIGHT, CreateSimpleControls(AndroidKeyCode.KEYCODE_D));
        var sprint = PlayInputAction.Create("Sprint", (int)GameActionId.SPRINT, CreateSimpleControls(AndroidKeyCode.KEYCODE_SPACE));
        var movementActions = new List<PlayInputAction> { moveUp, moveLeft, moveDown, moveRight, sprint };
        var movementGroup = PlayInputGroup.Create("Movement", movementActions);

        return PlayInputMap.Create(new List<PlayInputGroup> { movementGroup }, PlayMouseSettings.Create(false, false));
    }
}