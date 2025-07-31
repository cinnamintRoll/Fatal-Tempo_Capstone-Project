using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightReset : MonoBehaviour
{
    const string PlayerWeightKey = "PlayerWeightKg";
    
    public void ResetWeight()
    {
        if(PlayerPrefs.HasKey(PlayerWeightKey))
        PlayerPrefs.DeleteKey(PlayerWeightKey);
    }
}
