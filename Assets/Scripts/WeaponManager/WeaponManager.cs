using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Fist,
    Gun,
    Sword
}

public enum HandType
{
    Left,
    Right
}

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Weapon Parent Transforms")]
    [SerializeField] private Transform leftHandWeaponParent;
    [SerializeField] private Transform rightHandWeaponParent;

    private Dictionary<WeaponType, GameObject> leftHandWeaponMap = new();
    private Dictionary<WeaponType, GameObject> rightHandWeaponMap = new();

    [SerializeField] private WeaponType currentLeftWeapon = WeaponType.Fist;
    [SerializeField] private WeaponType currentRightWeapon = WeaponType.Fist;
    private WeaponType lastLeftWeapon = WeaponType.Fist;
    private WeaponType lastRightWeapon = WeaponType.Fist;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swapWeaponSound;

    private bool hasStarted = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        AutoMapWeapons(leftHandWeaponParent, leftHandWeaponMap);
        AutoMapWeapons(rightHandWeaponParent, rightHandWeaponMap);

        // Disable all weapons initially
        foreach (var weapon in leftHandWeaponMap.Values)
            if (weapon) weapon.SetActive(false);
        foreach (var weapon in rightHandWeaponMap.Values)
            if (weapon) weapon.SetActive(false);

        SwapWeapon(currentLeftWeapon, HandType.Left, true);
        SwapWeapon(currentRightWeapon, HandType.Right, true);
    }

    private void AutoMapWeapons(Transform parent, Dictionary<WeaponType, GameObject> map)
    {
        map.Clear();
        foreach (Transform child in parent)
        {
            if (System.Enum.TryParse(child.name, out WeaponType type))
            {
                map[type] = child.gameObject;
            }
            else
            {
                Debug.LogWarning($"Weapon '{child.name}' does not match any WeaponType enum.");
            }
        }
    }

    public void SwapWeapon(WeaponType newWeapon, HandType hand, bool ignoreOtherHandReset = false)
    {
        var currentMap = hand == HandType.Left ? leftHandWeaponMap : rightHandWeaponMap;
        var currentWeapon = hand == HandType.Left ? currentLeftWeapon : currentRightWeapon;

        if (hand == HandType.Left)
            lastLeftWeapon = currentLeftWeapon;
        else
            lastRightWeapon = currentRightWeapon;
        
        if (!ignoreOtherHandReset)
        {
            if (hand == HandType.Left)
            {
                SwapWeapon(WeaponType.Fist, HandType.Right, true);
            }
            else if (hand == HandType.Right)
            {
                SwapWeapon(WeaponType.Fist, HandType.Left, true);
            }
        }

        if (currentMap.TryGetValue(currentWeapon, out var oldWeapon) && oldWeapon != null)
            oldWeapon.SetActive(false);

        if (currentMap.TryGetValue(newWeapon, out var newWeaponGO) && newWeaponGO != null)
            newWeaponGO.SetActive(true);

        if (hasStarted)
            PlayWeaponSwapSound(hand);
        else
            hasStarted = true;

        if (hand == HandType.Left)
            currentLeftWeapon = newWeapon;
        else
            currentRightWeapon = newWeapon;
    }

    private void PlayWeaponSwapSound(HandType controlhand)
    {
        if (audioSource != null && swapWeaponSound != null)
        {
            // Set the audio source's position and rotation to match the correct controller
            if (controlhand == HandType.Left)
            {
                audioSource.transform.position = ControllerLocator.Instance.GetLeftHand().position;
            }
            else if (controlhand == HandType.Right)
            {
                audioSource.transform.position = ControllerLocator.Instance.GetRightHand().position;
            }

            audioSource.clip = swapWeaponSound;
            audioSource.Play();
        }
    }

    public void SwapBack()
    {
       SwapWeapon(lastLeftWeapon, HandType.Left, true);
       SwapWeapon(lastRightWeapon, HandType.Right, true);
        Debug.Log($"setting Left Weapon:{lastLeftWeapon}");
        Debug.Log($"Setting Right Weapon:{lastRightWeapon}");
    }

    public GameObject GetWeaponGameObject(WeaponType weaponType, HandType hand)
    {
        var map = hand == HandType.Left ? leftHandWeaponMap : rightHandWeaponMap;
        return map.ContainsKey(weaponType) ? map[weaponType] : null;
    }

    public void ResetToFist()
    {
            lastLeftWeapon = currentLeftWeapon;
            lastRightWeapon = currentRightWeapon;
        Debug.Log($"Last Left Weapon:{lastLeftWeapon}");
        Debug.Log($"Last Right Weapon:{lastRightWeapon}");
        SwapWeapon(WeaponType.Fist, HandType.Left, true);
        SwapWeapon(WeaponType.Fist, HandType.Right, true);

    }
}
