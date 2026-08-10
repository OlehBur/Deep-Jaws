

using UnityEngine;
using UnityEngine.Rendering;

public class FishSkinShader {
    /*public */
    readonly LocalKeyword DEFAULT_SKIN_KEYWORD;
    /*public */
    readonly LocalKeyword CHAMELEON_SKIN_KEYWORD;
    /*public */
    readonly LocalKeyword MERCURY_SKIN_KEYWORD;
    /*public */
    readonly LocalKeyword ASHFLOW_SKIN_KEYWORD;
    /*public */
    readonly LocalKeyword ZEBRA_SKIN_KEYWORD;

    private Material runtimeMaterial;

    public FishSkinShader(Renderer fishRenderer) {
        runtimeMaterial = fishRenderer.material;
        Shader shader = runtimeMaterial.shader;

        DEFAULT_SKIN_KEYWORD = new LocalKeyword(shader, "_SKIN_DEFAULT");
        CHAMELEON_SKIN_KEYWORD = new LocalKeyword(shader, "_SKIN_CHAMELEON");
        MERCURY_SKIN_KEYWORD = new LocalKeyword(shader, "_SKIN_MERCURY");
        ASHFLOW_SKIN_KEYWORD = new LocalKeyword(shader, "_SKIN_ASHFLOW");
        ZEBRA_SKIN_KEYWORD = new LocalKeyword(shader, "_SKIN_ZEBRA");
    }

    public void ActivateSkinByType(Skin type) {
        switch (type) {
            case Skin.Default:
            SetDefaulltSkin();
            break;
            case Skin.Chameleon:
            SetChameleonSkin();
            break;
            case Skin.Mercury:
            SetMercurySkin();
            break;
            case Skin.Ashflow:
            SetAshflowSkin();
            break;
            case Skin.Zebra:
            SetZebraSkin();
            break;
        }
    }

    void SetDefaulltSkin() {
        runtimeMaterial.DisableKeyword(CHAMELEON_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(MERCURY_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ZEBRA_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ASHFLOW_SKIN_KEYWORD);

        runtimeMaterial.EnableKeyword(DEFAULT_SKIN_KEYWORD);
    }

    void SetChameleonSkin() {
        runtimeMaterial.DisableKeyword(DEFAULT_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(MERCURY_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ZEBRA_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ASHFLOW_SKIN_KEYWORD);

        runtimeMaterial.EnableKeyword(CHAMELEON_SKIN_KEYWORD);
    }

    void SetMercurySkin() {
        runtimeMaterial.DisableKeyword(DEFAULT_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(CHAMELEON_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ZEBRA_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ASHFLOW_SKIN_KEYWORD);

        runtimeMaterial.EnableKeyword(MERCURY_SKIN_KEYWORD);
    }

    void SetAshflowSkin() {
        runtimeMaterial.DisableKeyword(DEFAULT_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(CHAMELEON_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ZEBRA_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(MERCURY_SKIN_KEYWORD);

        runtimeMaterial.EnableKeyword(ASHFLOW_SKIN_KEYWORD);
    }

    void SetZebraSkin() {
        runtimeMaterial.DisableKeyword(DEFAULT_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(CHAMELEON_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(ASHFLOW_SKIN_KEYWORD);
        runtimeMaterial.DisableKeyword(MERCURY_SKIN_KEYWORD);

        runtimeMaterial.EnableKeyword(ZEBRA_SKIN_KEYWORD);
    }

}