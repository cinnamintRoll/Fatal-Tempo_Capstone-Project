using UnityEngine;

public class ControllerLocator : MonoBehaviour
{
    public static ControllerLocator Instance { get; private set; }

    [Header("Assign Controller Transforms")]
    public Transform leftController;
    public Transform rightController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Transform GetLeftHand()
    {
        return leftController;
    }

    public Transform GetRightHand()
    {
        return rightController;
    }

    public Transform GetHand(BNG.ControllerHand hand)
    {
        return hand == BNG.ControllerHand.Left ? leftController : rightController;
    }
}
