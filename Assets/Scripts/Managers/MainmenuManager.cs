using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.CompilerServices;
using VContainer; //  for Uri.EscapeDataString



/// <summary>
/// відповідає за активніть графічного інтерфейсу пов'язану з грою
/// responsible for the activity of the graphical interface related to the game
/// </summary>
public class MainmenuManager : MonoBehaviour {
    //[SerializeField] GraphicManager graphicManager;


    //[Header("MainMenu Scene")]
    //[SerializeField] SceneLoader sceneLoader;


    

    //[SerializeField] SceneLoader faderScene;


    //[SerializeField] GameObject guideBookWnd;



    //[SerializeField] LocalizedString settingsMaxFpsLocalized;

    //[Header("Game Scene")]
    //only for game scene





    //private bool _isWorldDataExist = false;
   
    //private GPG_Manager gpg_manager;

    //[Inject] private readonly IPlayerFishPoolService _playerSpawnService;
    //[Inject] private readonly IAudioPlayService _audioManager;



    //private const string LOCALE_PREF_KEY = "LocaleIndex";
    //private const string FPS_KEY = "FPS";


    //private void Awake() {
    //    //if (gamemodePauseTitles != null)
    //    //    GameModeTitlesInit();



    //}

    //private void Start() {
        //_isWorldDataExist = ProgressSaver.IsWorldDataExist;

        //if (faderScene != null)        // Ensure the fader scene is not destroyed on load
        //    faderScene?.gameObject.SetActive(true);

        //if (continueButton != null) {// Initialize the continue button visibility based on saved data
        //    continueButton?.SetActive(_isWorldDataExist);
        //    GameStaticInfo.Gamemode = _isWorldDataExist ?
        //        ProgressSaver.LoadWorldData().gameMode : GameMode.None;
        //}

        //if (GameManager.IsInternetNotAvailable)
        //    FirstNoConnectionOpen();
    //}


    //private void OnEnable() {
        //if (_playerSpawnService != null) {
        //    _playerSpawnService.OnFishDie += ActivateRespawnWnd;
        //    _playerSpawnService.OnPlayerLVLup += PlayerLvlUpHandler;
        //}

//#if UNITY_ANDROID
//        if (kingOfDeepsObj != null && GPG_Manager.Instance != null)
//            GPG_Manager.Instance.OnLoadKingOfDeeps += KingOfDeepsUpdate;
//#endif
    //}

    //private void OnDisable() {
        //if (_playerSpawnService != null) {
        //    _playerSpawnService.OnFishDie -= ActivateRespawnWnd;
        //    _playerSpawnService.OnPlayerLVLup -= PlayerLvlUpHandler;
        //}

//#if UNITY_ANDROID
//        if (kingOfDeepsObj != null && GPG_Manager.Instance != null)
//            GPG_Manager.Instance.OnLoadKingOfDeeps -= KingOfDeepsUpdate;
//#endif
    //}


}