
using UnityEngine;



[CreateAssetMenu(fileName = "FishSpritesInfo", menuName = "Scriptable Objects/FishSpritesInfo")]
public class FishSpriteInfo : ScriptableObject {
    [field: SerializeField] public Sprite Dartfish_img { get; private set; }
    [field: SerializeField] public Sprite Crab_img { get; private set; }
    [field: SerializeField] public Sprite Seabass_img { get; private set; }
    [field: SerializeField] public Sprite Mackerel_img { get; private set; }
    [field: SerializeField] public Sprite Moray_img { get; private set; }
    [field: SerializeField] public Sprite Lionfish_img { get; private set; }
    [field: SerializeField] public Sprite Barracuda_img { get; private set; }
    [field: SerializeField] public Sprite Tuna_img { get; private set; }
    [field: SerializeField] public Sprite Hammer_img { get; private set; }
    [field: SerializeField] public Sprite Tiger_img { get; private set; }
    [field: SerializeField] public Sprite White_img { get; private set; }
    [field: SerializeField] public Sprite Cuttle_img { get; private set; }
    [field: SerializeField] public Sprite Puffer_img { get; private set; }

    public Sprite GetFishSprite(FishLevel fishType) {
        return fishType switch {
            FishLevel.Dartfish => Dartfish_img,
            FishLevel.Crab => Crab_img,
            FishLevel.SeaBass => Seabass_img,
            FishLevel.Mackerel => Mackerel_img,
            FishLevel.Moray => Moray_img,
            FishLevel.Lionfish => Lionfish_img,
            FishLevel.Barracuda => Barracuda_img,
            FishLevel.Tuna => Tuna_img,
            FishLevel.HammerShark => Hammer_img,
            FishLevel.TigerShark => Tiger_img,
            FishLevel.WhiteShark => White_img,
            FishLevel.Cuttlefish => Cuttle_img,
            FishLevel.Pufferfish => Puffer_img,
            _ => null
        };
    }
}
