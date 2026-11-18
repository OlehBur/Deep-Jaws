using UnityEngine;

public class GlobalEventSystemController : MonoBehaviour {
    private static GlobalEventSystemController instance;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}