using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class SpawnableEntry
{
    public GameObject spawnable;
    [Range(0f, 100f)]
    public float spawnChance = 100f;
}

public class GeneralSpawner : MonoBehaviour
{
    public List<SpawnableEntry> Spawnables;
    public int spawnIndex = 0;
    public enum EnemyType { Melee, Ranged, Sniper }
    public Transform spawnPoint;
    public Transform player;
    private bool canSpawn = true;
    [SerializeField] public EnemyAI enemy;
    [SerializeField] private Animator spawnAnim;
    [SerializeField] public Transform movepoint;
    [SerializeField, HideInInspector] private int _previousSpawnIndex = -1;
    private void Start()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager)
            player = gameManager.PlayerTransform;

        enemy.player = player;
        enemy.pointToMove = movepoint;

        // Keep only the spawnIndex object for now
        for (int i = Spawnables.Count - 1; i >= 0; i--)
        {
            if (i != spawnIndex)
            {
                if (Spawnables[i].spawnable != null)
                {
                    Destroy(Spawnables[i].spawnable);
                    Spawnables.RemoveAt(i);
                }
            }
            else
            {
                if (Spawnables[i].spawnable != null)
                    Spawnables[i].spawnable.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canSpawn)
        {
            spawnAnim.SetTrigger("Spawn");
            canSpawn = false;
        }
    }

    public void ReShowAllSpawnerVisuals()
    {
        for (int i = Spawnables.Count - 1; i >= 0; i--)
        {
            if(i == spawnIndex)
            {
                GameObject spawnable = Spawnables[i].spawnable;
                if (spawnable)
                    spawnable.SetActive(true);
                else return;
                if (spawnable.GetComponent<EnemyAI>() != null)
                {
                    if (enemy != null && enemy.isActiveAndEnabled)
                    {
                        enemy.ReShowVisuals();
                    }
                }
            }
        }
            
    }
    public void Spawn()
    {
        if (Spawnables.Count == 0 || Spawnables[0].spawnable == null)
            return;

        Spawnables[0].spawnable.SetActive(true);
    }

    public void RandomlyPickSpawn()
    {
        if (Spawnables.Count == 0) return;

        float totalChance = 0f;
        foreach (var entry in Spawnables)
        {
            totalChance += entry.spawnChance;
        }

        float randomValue = Random.Range(0, totalChance);
        float cumulative = 0f;
        for (int i = 0; i < Spawnables.Count; i++)
        {
            cumulative += Spawnables[i].spawnChance;
            if (randomValue <= cumulative)
            {
                spawnIndex = i;
                break;
            }
        }

        for (int i = 0; i < Spawnables.Count; i++)
        {
            if (Spawnables[i].spawnable != null)
            {
                Spawnables[i].spawnable.SetActive(i == spawnIndex);
            }
        }

        enemy.PickRandomEnemyType();
    }

    private void OnValidate()
    {
        if (Spawnables == null || Spawnables.Count == 0)
            return;

        spawnIndex = Mathf.Clamp(spawnIndex, 0, Spawnables.Count - 1);

        // Only apply activation if spawnIndex has changed
        if (spawnIndex == _previousSpawnIndex)
            return;

        for (int i = 0; i < Spawnables.Count; i++)
        {
            if (Spawnables[i].spawnable != null)
            {
                Spawnables[i].spawnable.SetActive(i == spawnIndex);
            }
        }

        _previousSpawnIndex = spawnIndex;
    }

    public void OnDespawn()
    {
        Destroy(this.gameObject, 5f);
    }
}
