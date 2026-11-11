
using System;
using System.Linq;
using UnityEngine;



[Serializable]
public class SkinsCards {
    public StoreCardView mercury;
    public StoreCardView chameleon;
    public StoreCardView ashflow;
    public StoreCardView zebra;
    public StoreCardView singularity;
    public StoreCardView blinker;
    public StoreCardView checkerboard;
    public StoreCardView corrosion;
    public StoreCardView cosmo;
    public StoreCardView cupid;
    public StoreCardView fairylights;
    public StoreCardView pixy;
    public StoreCardView porcelain;
    public StoreCardView rainbow;
    public StoreCardView sun;
    public StoreCardView invert;

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

    public StoreCardView GetSkinCard(Skin type) =>
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

    public void InitDI(UILocalizationInfo localizationInfo) {
        foreach (Skin s in ALL_SKINS_ITETATORS)
            GetSkinCard(s).InitDI(localizationInfo);
    }

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