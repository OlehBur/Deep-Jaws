using TMPro;
using UnityEngine;


public class BiomeRequirementView : MonoBehaviour {
    [SerializeField] GameObject obj;
    [SerializeField] TextMeshProUGUI victimValue;


    public void SetVictimName(string victimName) => victimValue.text = victimName;

    public void SetActive(bool active) => obj.SetActive(active);
}

