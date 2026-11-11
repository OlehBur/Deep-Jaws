using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class DemoFinWnd : WindowBase {
    [SerializeField] Button getFullGameButton;

    [Inject] private readonly IAudioPlayService _audioPlayService;

    private const string GAME_FULL_URL = "https://play.google.com/store/apps/details?id=com.bovvcompany.deepjaws&referrer=utm_source%3Ditchio%26utm_medium%3Dpc_demo";


    private void Start() {
        getFullGameButton.onClick.RemoveAllListeners();
        getFullGameButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.SanctuaryResearch));
        getFullGameButton.onClick.AddListener(OpenGooglePlayGamePage);
    }

    void OpenGooglePlayGamePage() =>
        Application.OpenURL(GAME_FULL_URL);    
}
