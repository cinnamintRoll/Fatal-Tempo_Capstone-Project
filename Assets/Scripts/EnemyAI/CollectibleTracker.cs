using System.Collections.Generic;
using UnityEngine;

public class CollectibleTracker : MonoBehaviour
{
    public static CollectibleTracker Instance;

    private List<GameObject> activeCollectibles = new List<GameObject>();

    [SerializeField] private int totalCollectiblesSpawned = 0;
    [SerializeField] private int totalCollectiblesCollected = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterCollectible(GameObject collectible)
    {
        if (!activeCollectibles.Contains(collectible))
        {
            activeCollectibles.Add(collectible);
            totalCollectiblesSpawned++;
        }
    }

    public void UnregisterCollectibleCollected(GameObject collectible)
    {
        if (activeCollectibles.Contains(collectible))
        {
            activeCollectibles.Remove(collectible);
            totalCollectiblesCollected++;
            CheckAllCollectiblesCollected();
        }
    }

    public void UnregisterCollectibleDespawned(GameObject collectible)
    {
        if (activeCollectibles.Contains(collectible))
        {
            activeCollectibles.Remove(collectible);
            CheckAllCollectiblesCollected();
        }
    }

    private void CheckAllCollectiblesCollected()
    {
        if (totalCollectiblesSpawned == totalCollectiblesCollected)
        {
            Debug.Log("All collectibles collected!");
            // Trigger optional event
        }
    }

    public int GetCollectibleCount()
    {
        return activeCollectibles.Count;
    }

    public int GetTotalCollectibleCount()
    {
        return totalCollectiblesSpawned;
    }

    public int GetTotalCollectiblesCollected()
    {
        return totalCollectiblesCollected;
    }

    public List<GameObject> GetAllCollectibles()
    {
        return new List<GameObject>(activeCollectibles);
    }
}
