using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class SkinsPreviewWnd : WindowBase {
    //[SerializeField] GameObject skinsPreviewWnd;
    [SerializeField] Button exitButton;
    [SerializeField] Button nextSkinButton;

    [Inject] private readonly IAudioPlayService _audioPlayService;
    [Inject] private readonly ISkinsPreviewController _skinsPreview;


    private void Start() {
        exitButton.onClick.RemoveAllListeners();
        nextSkinButton.onClick.RemoveAllListeners();

        exitButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        nextSkinButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));

        exitButton.onClick.AddListener(() => OnClose());
        nextSkinButton.onClick.AddListener(() => _skinsPreview.OnNextSkin());
    }

    protected override void OpenInternal() =>
        _skinsPreview.OnVisualize();

    protected override void CloseInternal() =>
        _skinsPreview.OnStopVisualize();

    public void SetVisualizeSkin(Skin skin) =>
        _skinsPreview.OnSetVisualizeSkin(skin);
}
