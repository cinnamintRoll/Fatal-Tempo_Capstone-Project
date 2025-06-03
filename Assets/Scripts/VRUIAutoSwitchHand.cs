using BNG;
using UnityEngine;

[RequireComponent(typeof(VRUISystem))]
public class VRUIAutoSwitchHand : MonoBehaviour
{
    private VRUISystem uiSystem;

    [SerializeField]
    private ControllerBinding leftTriggerBinding = ControllerBinding.LeftTriggerDown;
    [SerializeField]
    private ControllerBinding rightTriggerBinding = ControllerBinding.RightTriggerDown;

    private void Awake()
    {
        uiSystem = GetComponent<VRUISystem>();
    }

    private void Update()
    {
        if (uiSystem.LeftPointerTransform == null || uiSystem.RightPointerTransform == null)
            return;

        if (InputBridge.Instance.GetControllerBindingValue(leftTriggerBinding))
        {
            uiSystem.SelectedHand = ControllerHand.Left;
            uiSystem.UpdateControllerHand(ControllerHand.Left);
            uiSystem.UpdateControllerInput(leftTriggerBinding); // Update ControllerBinding
            uiSystem.ClearAll();
        }
        else if (InputBridge.Instance.GetControllerBindingValue(rightTriggerBinding))
        {
            uiSystem.SelectedHand = ControllerHand.Right;
            uiSystem.UpdateControllerHand(ControllerHand.Right);
            uiSystem.UpdateControllerInput(rightTriggerBinding); // Update ControllerBinding
            uiSystem.ClearAll();
        }
    }


}