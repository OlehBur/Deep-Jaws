using System.Collections.Generic;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshPro))]
public class FishInfoVisualizer : MonoBehaviour {
    [SerializeField] Fish fish;
    [SerializeField] TextMeshPro textMesh;
    [SerializeField] EffectManager effectManager;

    // Update is called once per frame
    void Update() {
        textMesh.text = $"LVL: {fish.Level.ToString()}, Prg: {fish.Progress.ToString("F1")}\n Eff: {GetEffectsInfo(effectManager.ActiveEffects)}";
    }

    string GetEffectsInfo(Dictionary<FishEffectType, Effect> d) {
        string info = "";

        foreach (FishEffectType type in EffectManager.AllEffectTypes)
            if (effectManager.IsEffectActive(type))
                info += type.ToString() + " ";
        //foreach (FishEffectType t in d.Keys)
        //    if (d.TryGetValue(t, out Effect v) && v != null)
        //info += t.ToString() + " ";

        return info;
    }
}
