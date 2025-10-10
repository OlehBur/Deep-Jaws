using UnityEngine;

public class LookOnCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Camera.main.transform.forward != Vector3.zero)
            // Align the text to face the camera
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
