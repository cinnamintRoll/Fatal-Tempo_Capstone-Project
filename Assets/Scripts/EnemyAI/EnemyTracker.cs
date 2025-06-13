using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    public static EnemyTracker Instance;

    private List<GameObject> activeEnemies = new List<GameObject>();

    [SerializeField] private int totalEnemiesSpawned = 0;
    [SerializeField] private int totalEnemiesKilled = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            totalEnemiesSpawned++;
        }
    }

    public void UnregisterEnemyKilled(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            totalEnemiesKilled++;
            CheckAllEnemiesDefeated();
        }
    }

    public void UnregisterEnemyDespawned(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            CheckAllEnemiesDefeated();
        }
    }

    private void CheckAllEnemiesDefeated()
    {
        if (totalEnemiesSpawned == totalEnemiesKilled)
        {
            Debug.Log("All enemies defeated!");
            // Trigger win condition or wave complete
        }
    }

    public int GetEnemyCount()
    {
        return activeEnemies.Count;
    }

    public int GetTotalEnemyCount()
    {
        return totalEnemiesSpawned;
    }

    public int GetTotalEnemiesKilled()
    {
        return totalEnemiesKilled;
    }

    public List<GameObject> GetAllEnemies()
    {
        return new List<GameObject>(activeEnemies);
    }
}
