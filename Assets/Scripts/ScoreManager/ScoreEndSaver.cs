using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreEndSaver : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerMask.value) != 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveSongScore();
            }
        }
        
    }
}
