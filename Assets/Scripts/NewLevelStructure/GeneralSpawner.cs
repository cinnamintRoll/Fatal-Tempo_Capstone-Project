using UnityEngine;
using System.Collections.Generic;

public class GeneralSpawner : MonoBehaviour
{
    public List<GameObject> Spawnables;
    public int spawnIndex = 0;
    public enum EnemyType { Melee, Ranged, Sniper }
    public Transform spawnPoint;
    public Transform player;
    private bool canSpawn = true;
    [SerializeField] private EnemyAI enemy;
    [SerializeField] private Animator spawnAnim;
    [SerializeField] private Transform movepoint;

    private void Start()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager)
            player = gameManager.PlayerTransform;
        enemy.player = player;
        enemy.pointToMove = movepoint;
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canSpawn)
            spawnAnim.SetTrigger("Spawn");
            //Spawn();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canSpawn = false;
        }
    }

    public void Spawn()
    {
        if (spawnIndex >= 0 && spawnIndex < Spawnables.Count)
        {
            GameObject toSpawn = Spawnables[spawnIndex];
            if (toSpawn != null)
            {
                toSpawn.transform.position = spawnPoint.position;
                toSpawn.transform.rotation = spawnPoint.rotation;
                toSpawn.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Invalid spawn index or missing spawnable in list.");
        }
    }

    public void RandomlyPickSpawn()
    {
        if (Spawnables.Count == 0) return;
        spawnIndex = Random.Range(0, Spawnables.Count);
        enemy.PickRandomEnemyType();
    }
}
