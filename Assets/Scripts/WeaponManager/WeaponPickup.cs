using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class WeaponPickup : MonoBehaviour
{
    public WeaponType weaponType;
    public float moveSpeed = 2f;
    public float floatingSpeed = 1f;
    public float floatingAmplitude = 0.2f;

    [SerializeField] private GameObject objectOffset;

    private Vector3 originalPosition;
    private bool isPickedUp = false;
    private bool hasSwapped = false;
    private ControllerHand targetHand;
    private Transform targetHandTransform;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if (!isPickedUp)
        {
            float newY = originalPosition.y + Mathf.Sin(Time.time * floatingSpeed) * floatingAmplitude;
            transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
        }
        else if (targetHandTransform != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetHandTransform.position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetHandTransform.rotation, Time.deltaTime * moveSpeed);

            if (!hasSwapped && Vector3.Distance(transform.position, targetHandTransform.position) < 0.1f)
            {
                // Swap the weapon in the correct hand
                HandType handType = targetHand == ControllerHand.Left ? HandType.Left : HandType.Right;
                WeaponManager.Instance.SwapWeapon(weaponType, handType);
                hasSwapped = true;

                if (objectOffset != null)
                {
                    Destroy(objectOffset);
                }

                // Optionally destroy the pickup object
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;
        if (!isPickedUp && ((1 << other.gameObject.layer) & 13) != 0) return;
        Transform rightHand = ControllerLocator.Instance.GetRightHand();
        Transform leftHand = ControllerLocator.Instance.GetLeftHand();

        if (leftHand == null || rightHand == null) return;
        float distLeft = Vector3.Distance(transform.position, leftHand.position);
        float distRight = Vector3.Distance(transform.position, rightHand.position);

        if (distLeft < distRight)
        {
            targetHand = ControllerHand.Left;
            targetHandTransform = leftHand;
        }
        else
        {
            targetHand = ControllerHand.Right;
            targetHandTransform = rightHand;
   
        }

        isPickedUp = true;
        
    }
}
