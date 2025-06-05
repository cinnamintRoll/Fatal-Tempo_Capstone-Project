using UnityEngine;
using UnityEngine.XR;
using BNG;
using TMPro;

public class BasicCalorieTracker : MonoBehaviour
{
    public float updateRate = 0.1f;
    public float calorieMultiplier = 0.005f;
    public float totalCalories = 0f;

    public TextMeshProUGUI caloriesText;

    private float timer = 0f;
    private Vector3 lastHeadLocalPos;

    void Start()
    {
        lastHeadLocalPos = GetHMDLocalPosition();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            timer = 0f;
            TrackCalories();
        }
    }

    void TrackCalories()
    {
        Vector3 leftVel = InputBridge.Instance.GetControllerVelocity(ControllerHand.Left);
        Vector3 rightVel = InputBridge.Instance.GetControllerVelocity(ControllerHand.Right);

        Vector3 currentHeadLocalPos = GetHMDLocalPosition();
        Vector3 headVel = (currentHeadLocalPos - lastHeadLocalPos) / updateRate;
        lastHeadLocalPos = currentHeadLocalPos;

        Debug.Log($"Left: {leftVel} Right: {rightVel} Head: {headVel}");

        float movementEnergy = leftVel.sqrMagnitude + rightVel.sqrMagnitude + headVel.sqrMagnitude;
        totalCalories += movementEnergy * calorieMultiplier * updateRate;

        UpdateCalorieDisplay();
    }

    void UpdateCalorieDisplay()
    {
        if (caloriesText != null)
        {
            caloriesText.text = $"{totalCalories:F2}";
        }
    }

    Vector3 GetHMDLocalPosition()
    {
        Vector3 localPosition;
        InputDevice headDevice = GetHMD();
        headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
        return localPosition;
    }

    InputDevice GetHMD()
    {
        return InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
    }

    public float GetCaloriesBurned()
    {
        return totalCalories;
    }
}
