using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ImageHsvAnim : MonoBehaviour {
    [SerializeField] Image image;

    private float progress = 0f;

    private const float DEFAULT_COLOR_S = .5f;
    private const float DEFAULT_COLOR_V = 1f;


    // Update is called once per frame
    void Update() {
        image.color = Color.HSVToRGB(progress, DEFAULT_COLOR_S, DEFAULT_COLOR_V);

        progress += DJTime.UnscaledDeltaTime;
        if (progress > 1f)
            progress = 0f;
    }
}
