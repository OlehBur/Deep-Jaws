using UnityEngine;

public class SnowAnimation : MonoBehaviour {
    //private const float SIDE_SPEED = 2f;
    private const float DOWN_SPEED = 5f;
    private RectTransform parent;

    private void Start() {
        parent = transform as RectTransform;
    }


    void Update() {
        if (gameObject.activeInHierarchy) {
            Vector2 buffPos;
            int dir;
            float hw;
            float hh;
            float parentWidth = parent.rect.width;
            float parentHeight = parent.rect.height;

            foreach (RectTransform rs in transform) {
                buffPos = rs.anchoredPosition;
                hw = rs.rect.width * .5f;
                hh = rs.rect.height * .5f;

                if (buffPos.y < -hh
                || buffPos.x > /*Screen.width*/parentWidth + hw
                || buffPos.x < -hw) {

                    buffPos.x = Random.Range(hw, /*Screen.width*/parentWidth - hw);
                    buffPos.y = /*Screen.height*/parentHeight + hh;
                    rs.anchoredPosition = buffPos;
                }

                dir = Random.Range(0, 5);
                if (dir == 0)
                    buffPos.x--;
                else if (dir == 1)
                    buffPos.x++;
                buffPos.y -= Random.value * DOWN_SPEED;

                rs.anchoredPosition = buffPos;
            }
        }
    }
}
