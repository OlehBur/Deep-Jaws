using UnityEngine;

public class TestColliderMovement : MonoBehaviour {
    Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        const float speed = 5f; // speed of movement
        Vector3 targetPos = Vector3.forward * speed * DJTime.FixedDeltaTime;
        rb.MovePosition(rb.position + targetPos);
    }

    //void Update() {
    //    const float speed = 5f;
    //    transform.position += Vector3.forward * speed * DJTime.DeltaTime;
    //}
}
