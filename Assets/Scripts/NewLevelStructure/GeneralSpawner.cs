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

        for (int i = Spawnables.Count - 1; i >= 0; i--)
        {
            if (i != spawnIndex)
            {
                DestroyImmediate(Spawnables[i],true);
                Spawnables.RemoveAt(i);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canSpawn)
            spawnAnim.SetTrigger("Spawn");
            canSpawn = false;
            //Spawn();
        }
    }


    public void Spawn()
    {

            GameObject toSpawn = Spawnables[0];
            if (toSpawn != null)
            {
                toSpawn.transform.position = spawnPoint.position;
                toSpawn.transform.rotation = spawnPoint.rotation;
                toSpawn.SetActive(true);
            }
    }

    public void RandomlyPickSpawn()
    {
        if (Spawnables.Count == 0) return;
        spawnIndex = Random.Range(0, Spawnables.Count);
        enemy.PickRandomEnemyType();
    }
}
