using UnityEngine;

public class SnowAnimation : MonoBehaviour {
    private const float SIDE_SPEED = 50f;
    private const float DOWN_SPEED = 200f;

    private RectTransform parent;
    private float parentWidth;
    private float parentHeight;


    private void Start() {
        parent = transform as RectTransform;
        parentWidth = parent.rect.width;
        parentHeight = parent.rect.height;
    }

    void Update() {
        if (gameObject.activeInHierarchy) {
            Vector2 buffPos;
            int dir;
            float flakeWidthHalf;

            foreach (RectTransform rs in transform) {
                buffPos = rs.anchoredPosition;
                flakeWidthHalf = rs.rect.width * .5f * rs.localScale.x;

                if (buffPos.y < -flakeWidthHalf
                || buffPos.x > parentWidth + flakeWidthHalf
                || buffPos.x < -flakeWidthHalf) {

                    buffPos.x = Random.Range(flakeWidthHalf, parentWidth - flakeWidthHalf);
                    buffPos.y = parentHeight + flakeWidthHalf;
                    rs.anchoredPosition = buffPos;
                }

                dir = Random.Range(0, 5);
                if (dir == 0)
                    buffPos.x -= DJTime.UnscaledDeltaTime * SIDE_SPEED;
                else if (dir == 1)
                    buffPos.x += DJTime.UnscaledDeltaTime * SIDE_SPEED;
                buffPos.y -= Random.value * DOWN_SPEED * DJTime.UnscaledDeltaTime;

                rs.anchoredPosition = buffPos;
            }
        }
    }
}
