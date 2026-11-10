using UnityEngine;
using VContainer;


public interface ISkinsPreviewController {
    void OnNextSkin();
    void OnVisualize();
    void OnSetVisualizeSkin(Skin skin);
    void OnStopVisualize();
}

public class SkinsPreviewController : MonoBehaviour, ISkinsPreviewController {
    [SerializeField] SkinnedMeshRenderer chameleon_Seabass;
    [SerializeField] SkinnedMeshRenderer ashflow_Mackarel;
    [SerializeField] SkinnedMeshRenderer blinker_Lionfish;
    [SerializeField] SkinnedMeshRenderer zebra_Barracuda;
    [SerializeField] SkinnedMeshRenderer mercury_Tuna;
    [SerializeField] SkinnedMeshRenderer checkerboard_Seabass;
    [SerializeField] SkinnedMeshRenderer corrosion_Mackarel;
    [SerializeField] SkinnedMeshRenderer fairylight_Lionfish;
    [SerializeField] SkinnedMeshRenderer cupid_Barracuda;
    [SerializeField] SkinnedMeshRenderer singularity_Tuna;
    [SerializeField] SkinnedMeshRenderer cosmo_Seabass;
    [SerializeField] SkinnedMeshRenderer pixy_Mackarel;
    [SerializeField] SkinnedMeshRenderer sun_Lionfish;
    [SerializeField] SkinnedMeshRenderer porcelain_Barracuda;
    [SerializeField] SkinnedMeshRenderer rainbow_Tuna;
    [SerializeField] SkinnedMeshRenderer invert_Seabass;

    [Inject] private readonly FishSkinInfo fishSkinManager;

    private int _selectedSkinOnPreview = -1;

    private const float SKINS_PREVIEW_ROTATION_SPEED = 8f;


    private void Start() {
        InitSkins();
    }

    private void Update() {
        if (/*skinsPreview.*/gameObject.activeInHierarchy)
            /*skinsPreview*/
            transform.Rotate(Vector3.up, DJTime.UnscaledDeltaTime * SKINS_PREVIEW_ROTATION_SPEED);
    }

    void InitSkins() {
        fishSkinManager.InitSkin(chameleon_Seabass, Skin.Chameleon, FishLevel.SeaBass);
        fishSkinManager.InitSkin(mercury_Tuna, Skin.Mercury, FishLevel.Tuna);
        fishSkinManager.InitSkin(ashflow_Mackarel, Skin.Ashflow, FishLevel.Mackerel);
        fishSkinManager.InitSkin(zebra_Barracuda, Skin.Zebra, FishLevel.Barracuda);
        fishSkinManager.InitSkin(singularity_Tuna, Skin.Singularity, FishLevel.Tuna);
        fishSkinManager.InitSkin(blinker_Lionfish, Skin.Blinker, FishLevel.Lionfish);
        fishSkinManager.InitSkin(checkerboard_Seabass, Skin.Checkerboard, FishLevel.SeaBass);
        fishSkinManager.InitSkin(corrosion_Mackarel, Skin.Corrosion, FishLevel.Mackerel);
        fishSkinManager.InitSkin(cosmo_Seabass, Skin.Cosmo, FishLevel.SeaBass);
        fishSkinManager.InitSkin(cupid_Barracuda, Skin.Cupid, FishLevel.Barracuda);
        fishSkinManager.InitSkin(fairylight_Lionfish, Skin.Fairylight, FishLevel.Lionfish);
        fishSkinManager.InitSkin(pixy_Mackarel, Skin.Pixy, FishLevel.Mackerel);
        fishSkinManager.InitSkin(porcelain_Barracuda, Skin.Porcelain, FishLevel.Barracuda);
        fishSkinManager.InitSkin(rainbow_Tuna, Skin.Rainbow, FishLevel.Tuna);
        fishSkinManager.InitSkin(sun_Lionfish, Skin.Sun, FishLevel.Lionfish);
        fishSkinManager.InitSkin(invert_Seabass, Skin.Invert, FishLevel.SeaBass);
    }


    public void OnNextSkin() {
        foreach (Transform t in /*skinsPreview.*/transform)
            t.gameObject.SetActive(false);

        ++_selectedSkinOnPreview;

        _selectedSkinOnPreview = /*skinsPreview*/transform.childCount > _selectedSkinOnPreview ?
            _selectedSkinOnPreview :
            0;

        /*skinsPreview*/
        transform.GetChild(_selectedSkinOnPreview).gameObject.SetActive(true);
    }

    public void OnVisualize() {
        if (_selectedSkinOnPreview == -1) {//if skin dont selected yet, then select first skin
            foreach (Transform t in /*skinsPreview.*/transform)
                t.gameObject.SetActive(false);

            _selectedSkinOnPreview = 0;
            transform.GetChild(_selectedSkinOnPreview).gameObject.SetActive(true);
            
            Debug.LogWarning("Skin preview: skin not selected yet, so first skin selected by default");
        }

        /*skinsPreview.*/
        gameObject.SetActive(true);

    }

    public void OnSetVisualizeSkin(Skin skin) {
        Renderer neededRenderer = skin switch {
            Skin.Chameleon => chameleon_Seabass,
            Skin.Mercury => mercury_Tuna,
            Skin.Ashflow => ashflow_Mackarel,
            Skin.Zebra => zebra_Barracuda,
            Skin.Singularity => singularity_Tuna,
            Skin.Blinker => blinker_Lionfish,
            Skin.Checkerboard => checkerboard_Seabass,
            Skin.Corrosion => corrosion_Mackarel,
            Skin.Cosmo => cosmo_Seabass,
            Skin.Cupid => cupid_Barracuda,
            Skin.Fairylight => fairylight_Lionfish,
            Skin.Pixy => pixy_Mackarel,
            Skin.Porcelain => porcelain_Barracuda,
            Skin.Rainbow => rainbow_Tuna,
            Skin.Sun => sun_Lionfish,
            Skin.Invert => invert_Seabass,
            _ => chameleon_Seabass
        };

        foreach (Transform t in /*skinsPreview*/transform)
            t.gameObject.SetActive(false);

        GameObject skinObject = neededRenderer.gameObject;
        skinObject.SetActive(true);
        _selectedSkinOnPreview = skinObject.transform.GetSiblingIndex();
    }

    public void OnStopVisualize() => gameObject.SetActive(false);
}
