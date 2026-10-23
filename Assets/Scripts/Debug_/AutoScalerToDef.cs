using UnityEngine;

/// <summary>
/// increases the scale of the object to the specified value starting from zero and disables 
/// the script when the initial scale is reached, if the component is disabled then the initial value will be restored
/// </summary>
public class AutoScalerToDef : MonoBehaviour {
    const float SCALE_EPSILON = 0.05f; // Threshold to consider the scale as "reached"
    private Vector3 defaultScale = new Vector3(1, 0, 1);
    private float speed = 3f;

    public void OnEnable() {
        defaultScale = /*transform.localScale*/Vector3.one;
        transform.localScale = Vector3.zero;
    }

    void Update() {
        transform.localScale = Vector3.Lerp(transform.localScale, defaultScale, speed * DJTime.DeltaTime);

        if (V3Eq(defaultScale, transform.localScale))
        enabled = false; // Disable this script when the scale reaches the default scale
    }

    private void OnDisable() {
        transform.localScale = defaultScale;
    }

    bool V3Eq(Vector3 v1, Vector3 v2) {
        return Mathf.Abs(v1.x - v2.x) < SCALE_EPSILON &&
               Mathf.Abs(v1.y - v2.y) < SCALE_EPSILON &&
               Mathf.Abs(v1.z - v2.z) < SCALE_EPSILON;
    }
}
