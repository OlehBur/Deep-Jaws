using UnityEngine;

public class ShaderTexuteBaker : MonoBehaviour {
    [SerializeField] string textureName;
    [SerializeField] Material shaderMat;
    [SerializeField] Vector2Int textureSize;
    [SerializeField] bool tapToBake;
    private RenderTexture renderTex;
    private bool prevFlag;
    private int imgCounter = 0;



    void OnValidate() {
        if (prevFlag != tapToBake) {
            prevFlag = tapToBake;

            renderTex = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGB32);
            renderTex.Create();


            Graphics.Blit(null, renderTex, shaderMat, 0);

            Texture2D bakedTexture = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTex;
            bakedTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            bakedTexture.Apply();
            RenderTexture.active = null;

            byte[] bytes = bakedTexture.EncodeToPNG();
            string name = textureName != null ? textureName : shaderMat.name + "_" + imgCounter++;
            System.IO.File.WriteAllBytes(Application.dataPath + "/Textures/Baked/" + name + ".png", bytes);

            Debug.Log("Texture " + shaderMat.name + "_" + imgCounter + " baked.");
            //Color debugColor = bakedTexture.GetPixel(0, 0);
            //Debug.Log("Pixel 0,0 = " + debugColor);
        }
    }

}
