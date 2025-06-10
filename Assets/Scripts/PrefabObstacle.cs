using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabObstacle : MonoBehaviour
{
    public Transform player;
    private bool canSpawn = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canSpawn)
        {
            canSpawn = false;
        }
    }
}
