using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BiomeUIWrapper {
    public ResidentButtonsView residentButtonsView;
    public TextMeshProUGUI biomeTittle;
    public GameObject biomeUnlockInfo;
    public BiomeLvlBarView biomeLvlBar;
    public TextMeshProUGUI productionPerMinuteTxt;
    public Button reseachButton;
    public TextMeshProUGUI researchButtonTxt;
    public Button boostResearchButton;
    public ProgressBar researchProgressBar;
    public BiomeRequirementView requirementView;
}
