using System.Collections;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class FPS_Viewer : MonoBehaviour {
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] bool ExtendedInfo = true;
    //private const float UPD_INTERVAL = 1f; // Update interval in seconds
    //private static readonly WaitForSecondsRealtime UPD_WAIT = new WaitForSecondsRealtime(UPD_INTERVAL);
    //private float currentTime = 0f;
    private int frameCount = 0;
    private float accumTime = 0f;
    private float timeLeft;
    private float updateTxtInterval = .5f;

    //private const int DEFAULT_SCREEN_FRAMERATE = 60;

    //void OnEnable() {
    //    StartCoroutine(LogPerformance());
    //}

    //private void OnDisable() {
    //    StopAllCoroutines();
    //}

    //IEnumerator LogPerformance() {
    //    while (true) {
    //        yield return UPD_WAIT;

    //        int fps = (int)(frameCount / currentTime);
    //        if (ExtendedInfo) {
    //            long gcMemory = System.GC.GetTotalMemory(false) / 1024;
    //            fpsText.text = $"FPS: {fps/*:F1*/}, \nGC: {gcMemory}KB, \nTime: {Time.time:F1}s";
    //        }
    //        else
    //            fpsText.text = fps.ToString();

    //        currentTime = 0f;
    //        frameCount = 0;
    //    }
    //}

    //void Update() {
    //    currentTime += Time.unscaledDeltaTime;
    //    frameCount++;
    //}


    //void Awake() {
    //    QualitySettings.vSyncCount = 0;
    //    //Application.targetFrameRate = 60;

    //    int screenHz = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
    //    if (screenHz <= 0) 
    //        screenHz = DEFAULT_SCREEN_FRAMERATE;
    //    Application.targetFrameRate = screenHz;
    //}

    void Start() {
        timeLeft = updateTxtInterval;
    }

    void Update() {
        // Використовуємо unscaledDeltaTime, щоб FPS рахувався правильно навіть при Time.timeScale = 0
        float deltaTime = Time.unscaledDeltaTime;

        timeLeft -= deltaTime;
        accumTime += deltaTime;
        frameCount++;

        // Оновлюємо значення лише раз на заданий інтервал
        if (timeLeft <= 0f) {
            float fps = frameCount / accumTime;

            if (ExtendedInfo) {
                long gcMemory = System.GC.GetTotalMemory(false) / (1024 * 1024); // Перевів у MB для зручності
                fpsText.text = $"FPS: {fps:F0}\nGC: {gcMemory} MB\nTime: {Time.time:F1}s";
            }
            else {
                fpsText.text = $"{fps:F0}";
            }

            // Скидаємо лічильники
            timeLeft = updateTxtInterval;
            accumTime = 0f;
            frameCount = 0;
        }
    }
}
