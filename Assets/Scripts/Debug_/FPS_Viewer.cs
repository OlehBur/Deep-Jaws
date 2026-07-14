using System.Collections;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class FPS_Viewer : MonoBehaviour {
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] bool ExtendedInfo = true;
    private const float UPD_INTERVAL = 1f; // Update interval in seconds
    private static readonly WaitForSecondsRealtime UPD_WAIT = new WaitForSecondsRealtime(UPD_INTERVAL);
    private float currentTime = 0f;
    private int frameCount = 0;


    void OnEnable() {
        StartCoroutine(LogPerformance());
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    IEnumerator LogPerformance() {
        while (true) {
            yield return UPD_WAIT;

            int fps = (int)(frameCount / currentTime);
            if (ExtendedInfo) {
                long gcMemory = System.GC.GetTotalMemory(false) / 1024;
                fpsText.text = $"FPS: {fps/*:F1*/}, \nGC: {gcMemory}KB, \nTime: {Time.time:F1}s";
            }
            else
                fpsText.text = fps.ToString();

            currentTime = 0f;
            frameCount = 0;
        }
    }

    void Update() {
        currentTime += Time.unscaledDeltaTime;
        frameCount++;
    }
}
