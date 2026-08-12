using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(fileName = "FishSkinManager", menuName = "Scriptable Objects/FishSkinManager")]
public class FishSkinManager : ScriptableObject {
    [Header("Default Skins")]
    [SerializeField] Material seabass;
    [SerializeField] Material mackerel;
    [SerializeField] Material moray;
    [SerializeField] Material lionfish;
    [SerializeField] Material barracuda;
    [SerializeField] Material tuna;
    [SerializeField] Material hammer;
    [SerializeField] Material tiger;
    [SerializeField] Material white;

    [Header("Store Skins")]
    [SerializeField] Material mercury;
    [SerializeField] Material chameleon;
    [SerializeField] Material ashflow;
    [SerializeField] Material zebra;
    [SerializeField] Material singularity;
    [SerializeField] Material blinker;
    [SerializeField] Material checkerboard;
    [SerializeField] Material corrosion;
    [SerializeField] Material cosmo;
    [SerializeField] Material cupid;
    [SerializeField] Material fairylights;
    [SerializeField] Material pixy;
    [SerializeField] Material porcelain;
    [SerializeField] Material rainbow;
    [SerializeField] Material sun;

    private Material runtimeMaterial;

    public void InitSkin(Renderer fishRenderer, Skin type, FishLevel fish) {
        runtimeMaterial = fishRenderer.material;
        Shader shader = runtimeMaterial.shader;

        Material levelMat = GetDefaultSkin(fish);
        Material skinTemplate = GetSkin(type);

        // new runtime skin mat 
        runtimeMaterial = new Material(skinTemplate);
        //runtimeMaterial.CopyPropertiesFromMaterial(levelMat);        //all base (movement) data transfer
        SafeCopyPropertiesFrom(runtimeMaterial, levelMat);
        fishRenderer.material = runtimeMaterial;
    }

    /// <summary>
    /// Safely copies only those parameters that exist in both materials,
    /// without disrupting the render queue (RenderQueue) or shader keys.
    /// </summary>
    public void SafeCopyPropertiesFrom(/*this*/ Material target, Material source) {
        if (source == null || target == null)
            return;

        Shader targetShader = target.shader;
        int propertyCount = targetShader.GetPropertyCount();

        for (int i = 0; i < propertyCount; i++) {
            int nameID = targetShader.GetPropertyNameId(i);

            // check if has same prop
            if (!source.HasProperty(nameID))
                continue;

            ShaderPropertyType type = targetShader.GetPropertyType(i);

            switch (type) {
                case ShaderPropertyType.Color:
                target.SetColor(nameID, source.GetColor(nameID));
                break;

                case ShaderPropertyType.Vector:
                target.SetVector(nameID, source.GetVector(nameID));
                break;

                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                target.SetFloat(nameID, source.GetFloat(nameID));
                break;

                case ShaderPropertyType.Texture:
                Texture srcTex = source.GetTexture(nameID);
                // if != null - keep txr
                if (srcTex != null) {
                    target.SetTexture(nameID, srcTex);
                    target.SetTextureOffset(nameID, source.GetTextureOffset(nameID));
                    target.SetTextureScale(nameID, source.GetTextureScale(nameID));
                }
                break;
            }
        }
    }

    Material GetDefaultSkin(FishLevel level) =>
        level switch {
            FishLevel.SeaBass => seabass,
            FishLevel.Mackerel => mackerel,
            FishLevel.Moray => moray,
            FishLevel.Lionfish => lionfish,
            FishLevel.Barracuda => barracuda,
            FishLevel.Tuna => tuna,
            FishLevel.HammerShark => hammer,
            FishLevel.TigerShark => tiger,
            FishLevel.WhiteShark => white,
            _ => null,
        };

    Material GetSkin(Skin type) =>
        type switch {
            Skin.Mercury => mercury,
            Skin.Chameleon => chameleon,
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
            _ => null
        };

}
