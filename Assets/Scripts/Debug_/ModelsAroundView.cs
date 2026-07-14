using UnityEngine;

public class ModelsAroundView : MonoBehaviour {
    [SerializeField] float rotationSpeed = 30f;
    private float currentAngle = 0f;
    private const float ANGLE_TRIGGER = 180f;

    [SerializeField] Transform parentWithModels;
    private Transform[] models;
    private int modelsCount;
    private int currentModelIndex = 0;




    private void Start() {
        modelsCount = parentWithModels.childCount;
        models = new Transform[modelsCount];
        int index = 0;

        foreach (Transform t in models) {
            models[index] = parentWithModels.GetChild(index);
            ++index;
        }
    }

    // Update is called once per frame
    void Update() {
        parentWithModels.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        currentAngle += rotationSpeed * Time.deltaTime;

        if (currentAngle >= ANGLE_TRIGGER) {
            models[currentModelIndex].gameObject.SetActive(false);
            currentModelIndex = (currentModelIndex + 1 < modelsCount) ? ++currentModelIndex : 0;
            currentAngle = 0f;

            models[currentModelIndex].gameObject.SetActive(true);
        }
    }
}
