using System;
using UnityEngine;
using VContainer;


public interface ILicenseValidator {
    void ValidateGame();
    event Action<bool> OnLicenseCheckCompleted;
}

public class LicenseValidator : MonoBehaviour, ILicenseValidator {
    [Inject] private readonly IAlertWindowsManager _alertWindowsManager;
    //[Inject] private readonly IMainMenuNavigation _mainMenu;

    public event Action<bool> OnLicenseCheckCompleted;

    private void Start() {
        ValidateGame();
    }

    public void ValidateGame() {
        //if (_mainMenu != null)
        //    _mainMenu.ActivateMainMenuContainer(false);
        OnLicenseCheckCompleted?.Invoke(false);
        _alertWindowsManager.ActivateLicenseFailedWnd(false);

        if (ProgressSaver.IsAccountDataExist) {
            AccountData data = ProgressSaver.LoadAccountData();
            if (data != null && data.isLicensed) {
                Debug.Log("License already verified in save file.");
                GrantLicense();
                return;
            }
        }

        //if (Application.internetReachability == NetworkReachability.NotReachable) {
        //    Debug.Log("No internet for initial license check.");
        //    firstInitLicensePanel.SetActive(true);
        //    return;
        //}

        PerformLicenseCheck();
    }

    private void PerformLicenseCheck() {
        //string installer = Application.installerName;
        bool isFromPlayStore = false;// = (installer == "com.android.vending");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_STANDALONE_WIN
        isFromPlayStore = true;
        //Debug.Log("<color=yellow>[LicenseValidator]</color> Bypassed check: Running in Unity Editor.");
#else
    // Production
    string installer = Application.installerName;
    isFromPlayStore = (installer == "com.android.vending");
    Debug.Log($"<color=yellow>[LicenseValidator]</color> Installer checking: {installer}");
#endif

        if (isFromPlayStore)
            GrantLicense(true);
        else {
            //if game installed not fr Play Market
            Debug.LogWarning("[LicenseValidator] License check failed! Invalid installer.");
            _alertWindowsManager.ActivateLicenseFailedWnd(true);
        }
    }

    private void GrantLicense(bool isNeedSaveData = false) {
        if (isNeedSaveData) {
            AccountData currentData = ProgressSaver.IsAccountDataExist ?
                ProgressSaver.LoadAccountData() : new AccountData();

            currentData.isLicensed = true;
            ProgressSaver.SaveData(currentData);
            Debug.Log("License granted and saved!");
        }

        //firstInitLicensePanel.SetActive(false);
        _alertWindowsManager.ActivateLicenseFailedWnd(false);
        //if (_mainMenu != null)
        //    _mainMenu.ActivateMainMenuContainer(true);
        OnLicenseCheckCompleted?.Invoke(true);
    }


    public void OnRetryInternetButtonClicked() {
        ValidateGame();
    }

    public void OnRetryLicenseButtonClicked() {
        ValidateGame();
    }

    public void OnQuitGameButtonClicked() {
        Application.Quit();
    }
}

/* 
//  Google Play Integrity API Method (with plugin com.google.play.integrity)
// async call to Google Play
var integrityManager = new IntegrityManager();
// ... token request logic ...
*/

//using Google.Play.Integrity;
//using System.Collections;
//using UnityEngine;

//public void RequestIntegrityCheck() {
//    StartCoroutine(CheckIntegrityRoutine());
//}

//private IEnumerator CheckIntegrityRoutine() {
//    var integrityManager = new IntegrityManager();

//    //create nonce on server and send it to the client.
//    //generated on serv
//    string nonce = "rand_str_token_BASE64";

//    var tokenRequest = new IntegrityTokenRequest(nonce);

//    // check token with internet connection
//    var requestFlowOperation = integrityManager.RequestIntegrityToken(tokenRequest);

//    // wait for the operation to complete
//    yield return requestFlowOperation;

//    if (requestFlowOperation.Error != IntegrityErrorCode.NoError) {
//        Debug.LogError("Integrity check failed with error: " + requestFlowOperation.Error);
//        //show license error wnd 
//        yield break;
//    }

//    // get the encrypted token
//    string encryptedToken = requestFlowOperation.GetResult().Token;

//    Debug.Log("Token received: " + encryptedToken);

//    // need to send this 'encryptedToken' to your server for decryption.
//}