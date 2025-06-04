using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSliceSegment : MonoBehaviour
{
    private PlayerHealth PlayerHealth;
    // Start is called before the first frame update
    void OnEnable()
    {
        PlayerHealth = PlayerHealth.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if(PlayerHealth)
        PlayerHealth.KillEnemy(this.transform.position);
    }
}
