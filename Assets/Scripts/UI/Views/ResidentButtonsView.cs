using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class ResidentButtonsView : MonoBehaviour {
    [field: SerializeField] public Button[] residentsButtons { get; private set; }
    [field: SerializeField] public Image[] residentsImages { get; private set; }


    [Inject] private readonly FishSpriteInfo _fishSpriteInfo;


    public void InitListeners(SanctuaryVisualizeWnd sanctuaryVisualizeUI) {
        for (int i = 0; i < residentsButtons.Length; i++) {//init residents buttons
            int redisDentInd = i;
            residentsButtons[i].onClick.RemoveAllListeners();
            residentsButtons[i].onClick.AddListener(() => sanctuaryVisualizeUI.OnOpenAlmanacUI(redisDentInd));
        }
    }

    public FishLevel GetFishResidentByIndex(int index, BiomeWrapper selectedBiome) {
        //if (index < 0 || index >= selectedBiome.Info.Requirements.Length)
        //    return FishLevel.WhiteShark;
        FishLevel actualResident = selectedBiome.Info.Requirements[0];
        if (index == 0)
            return actualResident;

        for (int r = 1, actual = 0; r < selectedBiome.Info.Requirements.Length; r++) {
            if (actualResident != selectedBiome.Info.Requirements[r]) {
                actualResident = selectedBiome.Info.Requirements[r];
                ++actual;

                if (actual == index)
                    break;
            }
        }

        return actualResident;
    }

    public void UpdateResidentImagesPreview(BiomeWrapper selectedBiome) {// only diff residents shows
        int lastSpriteIndex = 0;
        int residentInd = 0;
        residentsImages[lastSpriteIndex].sprite = _fishSpriteInfo.GetFishSprite(selectedBiome.Info.Requirements[residentInd]);
        residentsButtons[lastSpriteIndex].gameObject.SetActive(true);
        
        for (residentInd = 1; residentInd < selectedBiome.Level; residentInd++) {
            FishLevel newFish = selectedBiome.Info.Requirements[residentInd];
            FishLevel previousFish = selectedBiome.Info.Requirements[residentInd - 1];

            if (newFish != previousFish)
                ++lastSpriteIndex;

            residentsImages[lastSpriteIndex].sprite = _fishSpriteInfo.GetFishSprite(newFish);
            residentsButtons[lastSpriteIndex].gameObject.SetActive(true);
        }

        for (int i = lastSpriteIndex + 1; i < residentsImages.Length; i++) {
            residentsImages[i].sprite = null;
            residentsButtons[i].gameObject.SetActive(false);
        }
    }

    public void DisableAllSlots() {
        foreach (Button button in residentsButtons)//disable residents buttons
            button.gameObject.SetActive(false);
    }
}
