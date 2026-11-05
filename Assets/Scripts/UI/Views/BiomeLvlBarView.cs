using UnityEngine;

public class BiomeLvlBarView : MonoBehaviour
{
    [SerializeField] RectTransform[] stars;

    private readonly float[][] STAR_X_POS_MULT = new float[3][] {
         new float[] { -1f, 0f, 1f },//3 star
         new float[] { -1.5f, - 0.5f, 0.5f, 1.5f },//4 stars
         new float[] { -2f, -1f, 0f, 1f, 2f }//5 stars
    };
    private const int MIN_STARS = 3;


    public void UpdateStarsVisualize(int currentLevel, int maxLevel) {
        for (int i = 0; i < stars.Length; i++) {
            if (i < maxLevel) {
                stars[i].gameObject.SetActive(true);
                stars[i].anchoredPosition = new Vector2(STAR_X_POS_MULT[maxLevel - MIN_STARS][i] * stars[i].rect.width, stars[i].anchoredPosition.y);
                stars[i].GetChild(0).gameObject.SetActive(i < currentLevel);//fill star if current level >= index
            }
            else
                stars[i].gameObject.SetActive(false);
        }
    }
}
