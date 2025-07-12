using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject _objectToTeleport;

    private void Start()
    {
        if (_objectToTeleport != null)
        {
            TeleportObject();
        }
        else
        {
            Debug.LogWarning("No object assigned to teleport.");
        }
    }

    private void TeleportObject()
    {
        _objectToTeleport.transform.position = transform.position;
        _objectToTeleport.transform.rotation = transform.rotation;
    }
}
