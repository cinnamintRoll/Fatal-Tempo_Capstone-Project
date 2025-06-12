using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeightInputWindow : MonoBehaviour
{
    [SerializeField] private GameObject WeightMenu;
    [SerializeField] private InputField Input;
    [SerializeField] private GameObject ConfirmMenu;
    [SerializeField] private Text confirmText;
    [SerializeField] private Text errorText;
    [SerializeField] private GameObject LevelSelect;

    private float weight;
    const string PlayerWeightKey = "PlayerWeightKg";
    private const float MinWeight = 20f;  
    private const float MaxWeight = 650f; 

    public void Start()
    {
        if (!PlayerPrefs.HasKey(PlayerWeightKey))
        {
            LevelSelect.SetActive(false);
            WeightMenu.SetActive(true);
        }

        errorText.text = ""; 
    }

    public void EnterWeight()
    {
        HandleInput(Input.text);
    }

    void HandleInput(string text)
    {
        errorText.text = ""; 
        if (string.IsNullOrWhiteSpace(text))
        {
            weight = 0f;
            handleConfirm();
            WeightMenu.SetActive(false);
            return;
        }

        float inputValue;
        if (float.TryParse(text, out inputValue))
        {
            if (inputValue > MaxWeight)
            {
                errorText.text = $"That weight seems too high. Please enter a realistic value (max {MaxWeight}kg).";
                return;
            }
            else if (inputValue < 0f)
            {
                errorText.text = $"Weight can't be negative. Please enter a valid number.";
                return;
            }
            else if (inputValue > 0f && inputValue < MinWeight)
            {
                errorText.text = $"That weight seems too low. Please enter at least {MinWeight}kg, or leave it blank to skip.";
                return;
            }

            weight = inputValue;
            handleConfirm();
            WeightMenu.SetActive(false);
        }
        else
        {
            errorText.text = "Invalid input. Please enter a number or leave blank to skip.";
            Debug.LogWarning("Invalid float input.");
        }
    }


    void handleConfirm()
    {
        ConfirmMenu.gameObject.SetActive(true);

        if (weight > 0f)
        {
            if (confirmText != null)
            {
                confirmText.text = $"Your weight is {weight}kg, is this correct?";
            }
        }
        else if (weight == 0f)
        {
            if (confirmText != null)
            {
                confirmText.text = $"Do you want to skip? The calorie tracker will be less accurate.";
            }
        }
    }

    public void handleCancel()
    {
        ConfirmMenu.SetActive(false);
        WeightMenu.SetActive(true);
        Input.text = "";
        errorText.text = ""; // NEW: Clear error when retrying
    }

    public void handleApplyAndClose()
    {
        PlayerPrefs.SetFloat(PlayerWeightKey, weight);
        PlayerPrefs.Save();

        ConfirmMenu.SetActive(false);
        WeightMenu.SetActive(false);
        LevelSelect.SetActive(true);
        Debug.Log($"Saved player weight: {weight}kg");
    }
}
