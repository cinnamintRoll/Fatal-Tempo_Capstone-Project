#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using BNG;

[System.Serializable]
public struct WeaponVisualEntry
{
    public WeaponType weaponType;
    public GameObject visualPrefab;
}

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Visuals (Assign gameobject per weapon type)")]
    public List<WeaponVisualEntry> weaponVisuals = new List<WeaponVisualEntry>();

    public enum PickupMode { Single, Dual }
    [Header("Pickup Mode")]
    public PickupMode pickupMode = PickupMode.Single;

    [Header("Single pickup")]
    public WeaponType singleWeaponType;
    [Header("Dual pickup")]
    public WeaponType dualLeftWeaponType;
    public WeaponType dualRightWeaponType;

    [Header("Pickup Settings")]
    public float moveSpeed = 2f;
    public float floatingSpeed = 1f;
    public float floatingAmplitude = 0.2f;

    private Vector3 originalPosition;
    private bool isPickedUp = false;

    [SerializeField]
    private GameObject leftVisual;
    [SerializeField]
    private GameObject rightVisual;
    private Transform leftTarget;
    private Transform rightTarget;

    private bool leftHasSwapped = false;
    private bool rightHasSwapped = false;
    [SerializeField] private Vector3 DuplicateSpawn = new Vector3(0f, 0.046f, -0.045f);


    void Start()
    {
        originalPosition = transform.position;

    }

    void Update()
    {

        if (!isPickedUp)
        {
            // Floating animation
            float newY = originalPosition.y + Mathf.Sin(Time.time * floatingSpeed) * floatingAmplitude;
            transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
        }
        else
        {
            // Handle left visual
            if (leftVisual != null && leftTarget != null && !leftHasSwapped)
            {
                leftVisual.transform.position = Vector3.MoveTowards(leftVisual.transform.position, leftTarget.position, moveSpeed * Time.deltaTime);
                leftVisual.transform.rotation = Quaternion.Lerp(leftVisual.transform.rotation, leftTarget.rotation, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(leftVisual.transform.position, leftTarget.position) < 0.1f)
                {
                    WeaponManager.Instance.SwapWeapon(
                        pickupMode == PickupMode.Single ? singleWeaponType : dualLeftWeaponType,
                        HandType.Left, true);
                    leftHasSwapped = true;
                    leftVisual.SetActive(false);
                    leftVisual = null;
                }
            }

            // Handle right visual
            if (rightVisual != null && rightTarget != null && !rightHasSwapped)
            {
                rightVisual.transform.position = Vector3.MoveTowards(rightVisual.transform.position, rightTarget.position, moveSpeed * Time.deltaTime);
                rightVisual.transform.rotation = Quaternion.Lerp(rightVisual.transform.rotation, rightTarget.rotation, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(rightVisual.transform.position, rightTarget.position) < 0.1f)
                {
                    WeaponManager.Instance.SwapWeapon(dualRightWeaponType, HandType.Right, true);
                    rightHasSwapped = true;
                    rightVisual.SetActive(false);
                    rightVisual = null;
                }
            }

            // When all swaps done, destroy this pickup
            if ((pickupMode == PickupMode.Single && leftHasSwapped) ||
                (pickupMode == PickupMode.Dual && leftHasSwapped && rightHasSwapped))
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        // Ignore certain layers (you had &13, I assume these layers are to ignore)
        if (((1 << other.gameObject.layer) & 13) != 0) return;

        Transform leftHand = ControllerLocator.Instance.GetLeftHand();
        Transform rightHand = ControllerLocator.Instance.GetRightHand();

        if (leftHand == null || rightHand == null) return;

        isPickedUp = true;

        if (pickupMode == PickupMode.Single)
        {
            float distLeft = Vector3.Distance(transform.position, leftHand.position);
            float distRight = Vector3.Distance(transform.position, rightHand.position);

            Transform targetHand = distLeft < distRight ? leftHand : rightHand;

            GameObject visual = GetVisualObject(singleWeaponType);
            if (visual != null)
            {
                visual.transform.position = transform.position;
                visual.SetActive(true);
                leftVisual = visual;
                leftTarget = targetHand;
            }
        }
        else if (pickupMode == PickupMode.Dual)
        {
            GameObject left = leftVisual;
            GameObject right = rightVisual;

            if (left != null)
            {
                left.transform.position = transform.position;
                left.SetActive(true);
                leftVisual = left;
                leftTarget = leftHand;
            }
            else
            {
                Debug.LogWarning("could not find left");
            }

            if (right != null)
            {
                right.transform.position = transform.position;
                right.SetActive(true);
                rightVisual = right;
                rightTarget = rightHand;
            }
            else
            {
                Debug.LogWarning("could not find right");
            }

        }
    }

    private GameObject GetVisualObject(WeaponType weaponType)
    {
        foreach (var entry in weaponVisuals)
        {
            if (entry.weaponType == weaponType)
                return entry.visualPrefab;
        }
        return null;
    }

#if UNITY_EDITOR
    private GameObject rightDuplicate;
    [SerializeField, HideInInspector] private WeaponType lastSingleWeaponType;
    [SerializeField, HideInInspector] private WeaponType lastDualLeftWeaponType;
    [SerializeField, HideInInspector] private WeaponType lastDualRightWeaponType;
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;

                bool weaponTypeChanged = false;

                if (pickupMode == PickupMode.Single)
                {
                    if (singleWeaponType == lastSingleWeaponType)
                        return;

                    weaponTypeChanged = true;
                    lastSingleWeaponType = singleWeaponType;
                }
                else if (pickupMode == PickupMode.Dual)
                {
                    if (dualLeftWeaponType == lastDualLeftWeaponType && dualRightWeaponType == lastDualRightWeaponType)
                        return;

                    weaponTypeChanged = true;
                    lastDualLeftWeaponType = dualLeftWeaponType;
                    lastDualRightWeaponType = dualRightWeaponType;
                }

                // Update visual references
                leftVisual = pickupMode == PickupMode.Single ?
                    GetVisualObject(singleWeaponType) :
                    GetVisualObject(dualLeftWeaponType);

                rightVisual = pickupMode == PickupMode.Dual ?
                    GetVisualObject(dualRightWeaponType) : null;

                // Handle duplicate visuals
                if (pickupMode == PickupMode.Dual && rightVisual != null && leftVisual != null)
                {
                    if (leftVisual == rightVisual)
                    {
                        if (rightDuplicate == null)
                        {
                            rightDuplicate = Instantiate(rightVisual, transform);
                            rightDuplicate.name = rightVisual.name + "_rightDuplicate";
                        }
                        rightVisual = rightDuplicate;
                        EditorUtility.SetDirty(this);
                    }
                    else if (rightDuplicate != null)
                    {
                        DestroyImmediate(rightDuplicate);
                        rightDuplicate = null;
                    }
                }
                else if (rightDuplicate != null)
                {
                    DestroyImmediate(rightDuplicate);
                    rightDuplicate = null;
                }

                // Activate/deactivate visual prefabs
                foreach (var entry in weaponVisuals)
                {
                    if (entry.visualPrefab == null) continue;

                    bool shouldBeActive = pickupMode switch
                    {
                        PickupMode.Single => entry.weaponType == singleWeaponType,
                        PickupMode.Dual => entry.weaponType == dualLeftWeaponType || entry.weaponType == dualRightWeaponType,
                        _ => false
                    };

                    if (entry.visualPrefab.activeSelf != shouldBeActive)
                    {
                        entry.visualPrefab.SetActive(shouldBeActive);
                        EditorUtility.SetDirty(entry.visualPrefab);
                    }
                }

                // Reset transforms only if changed
                if (weaponTypeChanged)
                {
                    if (leftVisual != null)
                    {
                        leftVisual.transform.localPosition = Vector3.zero;
                        leftVisual.transform.localRotation = Quaternion.identity;
                    }
                    if (rightVisual != null)
                    {
                        rightVisual.transform.localPosition = DuplicateSpawn;
                        rightVisual.transform.localRotation = Quaternion.identity;
                    }
                }
            };
        }
#endif
    }

}