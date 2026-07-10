using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LicenseValidator : MonoBehaviour {
    [Header("UI Panels")]
    //[SerializeField] private GameObject firstInitLicensePanel;
    [SerializeField] private GameObject licenseFailedPanel;

    [Header("Game Start Logic")]
    [SerializeField] private GameObject mainMenuContainer;


    private void Awake/*Start*/() {
        ValidateGame();
    }

    public void ValidateGame() {
        mainMenuContainer.SetActive(false);
        //firstInitLicensePanel.SetActive(false);
        licenseFailedPanel.SetActive(false);

        if (ProgressSaver.IsDataExist()) {
            SaveData data = ProgressSaver.Load();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        isFromPlayStore = true;
        //Debug.Log("<color=yellow>[LicenseValidator]</color> Bypassed check: Running in Unity Editor.");
#else
    // Production
    string installer = Application.installerName;
    isGameFromPlayStore = (installer == "com.android.vending");
    Debug.Log($"<color=yellow>[LicenseValidator]</color> Installer checking: {installer}");
#endif

        if (isFromPlayStore)
            GrantLicense(true);
        else {
            //if game installed not fr Play Market
            Debug.LogWarning("[LicenseValidator] License check failed! Invalid installer.");
            licenseFailedPanel.SetActive(true);
        }
    }

    private void GrantLicense(bool isNeedSaveData = false) {
        if (isNeedSaveData) {
            SaveData currentData = ProgressSaver.IsDataExist() ? ProgressSaver.Load() : new SaveData();
            currentData.isLicensed = true;
            ProgressSaver.Save(currentData);
            Debug.Log("License granted and saved!");
        }

        //firstInitLicensePanel.SetActive(false);
        licenseFailedPanel.SetActive(false);
        if (mainMenuContainer != null)
            mainMenuContainer.SetActive(true);
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