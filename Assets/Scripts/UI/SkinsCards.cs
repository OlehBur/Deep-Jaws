
using System;
using System.Linq;
using UnityEngine;



[Serializable]
public class SkinsCards {
    public SkinStoreCard mercury;
    public SkinStoreCard chameleon;
    public SkinStoreCard ashflow;
    public SkinStoreCard zebra;
    public SkinStoreCard singularity;
    public SkinStoreCard blinker;
    public SkinStoreCard checkerboard;
    public SkinStoreCard corrosion;
    public SkinStoreCard cosmo;
    public SkinStoreCard cupid;
    public SkinStoreCard fairylights;
    public SkinStoreCard pixy;
    public SkinStoreCard porcelain;
    public SkinStoreCard rainbow;
    public SkinStoreCard sun;
    public SkinStoreCard invert;

    public static readonly Skin[] ALL_SKINS_ITETATORS = Enum.GetValues(typeof(Skin))
        .Cast<Skin>()
        .ToArray();

    public static readonly Skin[] PURCHASE_SKINS_ITERATORS = new Skin[]
    {
        Skin.Mercury,
        Skin.Chameleon,
        Skin.Ashflow
    };
    //Enum.GetValues(typeof(Skin))
    //            .Cast<Skin>()
    //            .Skip(1)//without default
    //            .ToArray();
    public static readonly Skin[] FREE_SKINS_ITERATORS = new Skin[] {
        Skin.Invert,
        Skin.Zebra,
        Skin.Singularity,
        Skin.Blinker,
        Skin.Checkerboard,
        Skin.Corrosion,
        Skin.Cosmo,
        Skin.Cupid,
        Skin.Fairylight,
        Skin.Pixy,
        Skin.Porcelain,
        Skin.Rainbow,
        Skin.Sun,
    };

    public SkinStoreCard GetSkinCard(Skin type) =>
    type switch {
        Skin.Chameleon => chameleon,
        Skin.Mercury => mercury,
        Skin.Ashflow => ashflow,
        Skin.Zebra => zebra,
        Skin.Singularity => singularity,
        Skin.Blinker => blinker,
        Skin.Checkerboard => checkerboard,
        Skin.Corrosion => corrosion,
        Skin.Cosmo => cosmo,
        Skin.Cupid => cupid,
        Skin.Fairylight => fairylights,
        Skin.Pixy => pixy,
        Skin.Porcelain => porcelain,
        Skin.Rainbow => rainbow,
        Skin.Sun => sun,
        Skin.Invert => invert,
        _ => chameleon
    };

    public void InitListeners(IGamePurchaseManager gamePurchaseManager) {
        if (gamePurchaseManager != null) {
            mercury.BuyOnClick.AddListener(gamePurchaseManager.OnBuy_MercurySkin);
            ashflow.BuyOnClick.AddListener(gamePurchaseManager.OnBuy_AshflowSkin);
            chameleon.BuyOnClick.AddListener(gamePurchaseManager.OnBuy_ChameleonSkin);
        }
        else
            Debug.LogWarning("GamePurchase manager Instance is null!");

        zebra.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Zebra);
            zebra.ShowSelectUI();
        });
        singularity.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Singularity);
            singularity.ShowSelectUI();
        });
        blinker.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Blinker);
            blinker.ShowSelectUI();
        });
        checkerboard.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Checkerboard);
            checkerboard.ShowSelectUI();
        });
        corrosion.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Corrosion);
            corrosion.ShowSelectUI();
        });
        cosmo.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Cosmo);
            cosmo.ShowSelectUI();
        });
        cupid.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Cupid);
            cupid.ShowSelectUI();
        });
        fairylights.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Fairylight);
            fairylights.ShowSelectUI();
        });
        pixy.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Pixy);
            pixy.ShowSelectUI();
        });
        porcelain.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Porcelain);
            porcelain.ShowSelectUI();
        });
        rainbow.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Rainbow);
            rainbow.ShowSelectUI();
        });
        sun.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Sun);
            sun.ShowSelectUI();
        });
        invert.BuyOnClick.AddListener(() => {
            gamePurchaseManager.OnBuy_FreeSkin(Skin.Invert);
            invert.ShowSelectUI();
        });
    }
}