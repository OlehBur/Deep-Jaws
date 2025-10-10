using UnityEngine;

public class ShieldBubbleControl : MonoBehaviour
{
    private const float SPEED = 4f;
    private float maxHeight = 15f;
    private const float MIN_BUBBLE_SCALE = 3f;
    private const float MAX_BUBBLE_SCALE = 6f;

    private void OnEnable() {
        transform.localScale = Vector3.one * Random.Range(MIN_BUBBLE_SCALE, MAX_BUBBLE_SCALE);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + new Vector3(0, 1, 0) * Time.deltaTime * SPEED;

        if(transform.position.y > maxHeight)
            gameObject.SetActive(false);
    }

    private void OnTriggerStay(Collider other) {
        if (other.transform.parent.TryGetComponent(out EffectManager fem)) 
            gameObject.SetActive(false);
    }
}
