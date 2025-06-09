using UnityEngine;

public class CalorieTrackerManager : MonoBehaviour
{
    public static CalorieTrackerManager Instance { get; private set; }

    [Header("Base Settings")]
    public float playerWeightKg = 70f; // Optional: let user input this
    public float baseCalorieMultiplier = 0.005f;

    [Header("Energy Weights")]
    public float headWeightFactor = 1.2f;
    public float handWeightFactor = 0.8f;

    private Vector3 lastLeftVel;
    private Vector3 lastRightVel;
    private Vector3 lastHeadVel;

    public float totalCalories = 0f;

    const string PlayerWeightKey = "PlayerWeightKg";

    const string TotalCaloriesKey = "TotalPlayerCalories";
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved player weight
        if (PlayerPrefs.HasKey(PlayerWeightKey))
        {
            playerWeightKg = PlayerPrefs.GetFloat(PlayerWeightKey);
        }
        else
        {
            PlayerPrefs.SetFloat(PlayerWeightKey, playerWeightKg); // save default
        }
    }

    public void AddCaloriesFromMovement(Vector3 leftVel, Vector3 rightVel, Vector3 headVel, float deltaTime)
    {
        // Estimate acceleration (change in velocity / time)
        Vector3 leftAcc = (leftVel - lastLeftVel) / deltaTime;
        Vector3 rightAcc = (rightVel - lastRightVel) / deltaTime;
        Vector3 headAcc = (headVel - lastHeadVel) / deltaTime;

        lastLeftVel = leftVel;
        lastRightVel = rightVel;
        lastHeadVel = headVel;

        // Compute weighted movement energy of HMD and controllers
        float weightedEnergy =
            leftAcc.sqrMagnitude * handWeightFactor +
            rightAcc.sqrMagnitude * handWeightFactor +
            headAcc.sqrMagnitude * headWeightFactor;

        // Get an intensity multiplier
        float intensity = GetIntensityMultiplier(weightedEnergy);

        // MET approximation (light = 2.5, moderate = 4.0, vigorous = 6.0)
        float MET = GetMETLevel(intensity);

        // Real calorie estimate formula:
        // Calories/min = (MET * 3.5 * weight in kg) / 200
        float caloriesPerSecond = (MET * 3.5f * playerWeightKg) / 200f / 60f;

        totalCalories += caloriesPerSecond * deltaTime;
    }

    float GetIntensityMultiplier(float energy)
    {
        if (energy < 1f) return 0.5f;
        if (energy < 3f) return 1f;
        if (energy < 6f) return 1.5f;
        return 2f;
    }

    float GetMETLevel(float intensityMultiplier)
    {
        if (intensityMultiplier < 0.75f) return 2.5f;   // Light
        if (intensityMultiplier < 1.25f) return 4.0f;   // Moderate
        return 6.0f;                                    // Vigorous
    }

    public void ResetCalories()
    {
        totalCalories = 0f;
    }

    public float GetCalories()
    {
        return totalCalories;
    }

    public void SaveCalories()
    {
        PlayerPrefs.SetFloat(TotalCaloriesKey, totalCalories);
    }
}
