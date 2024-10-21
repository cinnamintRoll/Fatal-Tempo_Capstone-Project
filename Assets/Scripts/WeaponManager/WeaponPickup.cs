using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Tooltip("The type of weapon this pickup represents.")]
    public WeaponType weaponType;

    [Tooltip("The position where the weapon will float to (usually the player's weapon position).")]
    public Transform targetPosition;

    [Tooltip("Speed at which the weapon floats to the player.")]
    public float moveSpeed = 2f;

    [Tooltip("Speed of the idle floating effect (how fast it moves up and down).")]
    public float floatingSpeed = 1f;

    [Tooltip("Amplitude of the floating effect (how far it moves up and down).")]
    public float floatingAmplitude = 0.2f;
    private bool isPickedUp = false;  // To check if the player has triggered the pickup
    [SerializeField] private GameObject objectOffset;
    private Vector3 originalPosition;  // Original position for floating animation

    void Start()
    {
        originalPosition = transform.position;  // Store the starting position for the floating effect
    }

    void Update()
    {
        if (!isPickedUp)
        {
            // Apply floating animation (simple up and down movement)
            float newY = originalPosition.y + Mathf.Sin(Time.time * floatingSpeed) * floatingAmplitude;
            transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
        }
        else
        {
            // Get the target weapon GameObject and its transform
            GameObject targetWeapon = WeaponManager.Instance.GetWeaponGameObject(weaponType);
            if (targetWeapon != null)
            {
                targetPosition = targetWeapon.transform;

                // Move the weapon towards the player's weapon position
                transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, moveSpeed * Time.deltaTime);

                // Smoothly rotate the pickup to match the target weapon's rotation
                transform.rotation = Quaternion.Lerp(transform.rotation, targetPosition.rotation, Time.deltaTime * moveSpeed);

                // When the weapon reaches the target, swap the weapon in the WeaponManager
                if (Vector3.Distance(transform.position, targetPosition.position) < 0.1f)
                {
                    WeaponManager.Instance.SwapWeapon(weaponType);  // Call the WeaponManager to swap the weapon
                    Destroy(objectOffset);  // Destroy the floating weapon pickup object once the weapon is swapped
                }
            }
        }
    }


    // This method will be triggered when the player touches the floating weapon (using VR hand or collider)
    private void OnTriggerEnter(Collider other)
    {
        // Check if the layer of the other object is the "Hand" layer
        if (((1 << other.gameObject.layer) & 13) != 0)
        {
            isPickedUp = true;  // Start the floating toward player behavior
        }
    }
}
