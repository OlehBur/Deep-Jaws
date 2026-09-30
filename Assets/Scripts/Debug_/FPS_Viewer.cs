using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class FPS_Viewer : MonoBehaviour {
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] bool ExtendedInfo = true;

    private int frameCount = 0;
    private float accumTime = 0f;
    private float timeLeft;
    private float updateTxtInterval = .5f;


    void Start() {
        timeLeft = updateTxtInterval;
    }

    void Update() {
        float deltaTime = DJTime.UnscaledDeltaTime;

        timeLeft -= deltaTime;
        accumTime += deltaTime;
        frameCount++;

        if (timeLeft <= 0f) {
            float fps = frameCount / accumTime;

            if (ExtendedInfo) {
                long gcMemory = System.GC.GetTotalMemory(false) / (1024 * 1024); //in MB
                fpsText.text = $"FPS: {fps:F0}\nGC: {gcMemory} MB\nTime: {Time.time:F1}s";
            }
            else {
                fpsText.text = $"{fps:F0}";
            }

            // reset
            timeLeft = updateTxtInterval;
            accumTime = 0f;
            frameCount = 0;
        }
    }
}
