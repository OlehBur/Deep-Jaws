using GooglePlayGames.BasicApi.SavedGame;
using System.Collections;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class FPS_Viewer : MonoBehaviour {
    [SerializeField] TextMeshProUGUI fpsText;
    private const float UPD_INTERVAL = 1f; // Update interval in seconds
    private static readonly WaitForSeconds UPD_WAIT = new WaitForSeconds(UPD_INTERVAL);
    private float currentTime = 0f;
    private int frameCount = 0;


    void Start() {
        StartCoroutine(LogPerformance());
    }

    IEnumerator LogPerformance() {
        while (true) {
            yield return UPD_WAIT/*new WaitForSeconds(updateInterval)*/;
            float fps = frameCount / currentTime;
            long gcMemory = System.GC.GetTotalMemory(false) / 1024;
            fpsText.text = $"FPS: {fps:F1}, \nGC: {gcMemory}KB, \nTime: {Time.time:F1}s";
            currentTime = 0f;
            frameCount = 0;
        }
    }

    void Update() {
        currentTime += Time.unscaledDeltaTime;
        frameCount++;
    }

    //private void Awake() {
    //    fpsText = GetComponent<TextMeshProUGUI>();
    //}

    //void Update() {
    //    currentTime += Time.unscaledDeltaTime;

    //    if (currentTime > updateInterval) {
    //        int fps = (int)(1f / Time.unscaledDeltaTime);

    //        fpsText.text = $"FPS: {fps.ToString()}"; // display FPS
    //        currentTime = 0f; // reset timer
    //    }
    //}
}
