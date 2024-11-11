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

        Vector3 targetforward = this.transform.forward;
        Vector3 cameraForward = _objectToTeleport.transform.forward;

        float angle = Vector3.SignedAngle(cameraForward, targetforward, Vector3.up);

        

        _objectToTeleport.transform.position = transform.position;
        _objectToTeleport.transform.rotation = transform.rotation;
    }
}
