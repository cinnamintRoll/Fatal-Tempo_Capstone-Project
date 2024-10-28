using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VRJoystickButtonSelector : MonoBehaviour
{
    [SerializeField] private GameObject LowPowerScreen;
    public BNG.JoystickControl joystick; // Reference to the custom JoystickControl script
    public EventSystem eventSystem; // Event system that manages UI interactions
    public float inputDelay = 0.2f; // Delay to avoid rapid navigation for buttons/sliders
    private float lastInputTime;
    [SerializeField] private Button StartingButton;
    public float sliderStep = 0.1f; // Amount to move the slider
    public float scrollSpeed = 0.02f; // Speed of scrollbar scrolling
    private Vector2 lastJoystickVector; // Track the previous joystick input
    public bool singleSelectMode = true; // Toggle for single select mode for buttons and sliders

    // UnityEvent triggered when the selection moves
    [System.Serializable]
    public class SelectionMoveEvent : UnityEvent<GameObject, MoveDirection> { }

    // Event to be assigned in the inspector or code
    public SelectionMoveEvent OnSelectionMoved;

    private void OnEnable()
    {
        DestinationButton(StartingButton);
        lastJoystickVector = Vector2.zero; // Initialize with no input
    }

    private void Update()
    {
        // Only move if enough time has passed since the last input for buttons and sliders
        if (Time.time - lastInputTime < inputDelay)
            return;

        // Get the current UI selection
        GameObject currentSelected = eventSystem.currentSelectedGameObject;
        if (!LowPowerScreen.activeInHierarchy)
        {
            if (currentSelected != null)
            {
                // Use the LeverVector from JoystickControl
                Vector2 joystickVector = joystick.LeverVector;

                // Get the SingleSelectModeComponent from the currently selected UI element
                SingleSelectModeComponent singleSelectComponent = currentSelected.GetComponent<SingleSelectModeComponent>();

                bool isSingleSelectMode = singleSelectComponent != null ? singleSelectComponent.singleSelectMode : true;

                // If the current element is a scrollbar, handle continuous scrolling
                Scrollbar selectedScrollbar = currentSelected.GetComponent<Scrollbar>();
                if (selectedScrollbar != null)
                {
                    HandleContinuousScrollbar(selectedScrollbar, joystickVector);
                    lastInputTime = Time.time; // Reset input delay only for buttons/sliders
                }
                else
                {
                    // Handle single-select mode or continuous-select for other elements like sliders and buttons
                    if (isSingleSelectMode)
                    {
                        // Trigger selection only when crossing the threshold, not continuously
                        if (ShouldSelect(joystickVector, lastJoystickVector))
                        {
                            HandleSelection(currentSelected, joystickVector);
                            lastInputTime = Time.time;
                        }
                    }
                    else
                    {
                        // Navigate continuously
                        HandleSelection(currentSelected, joystickVector);
                        lastInputTime = Time.time;
                    }
                }

                // Update the lastJoystickVector for the next comparison
                lastJoystickVector = joystickVector;
            }
        }
    }

    // Check if joystick has crossed the movement threshold
    private bool ShouldSelect(Vector2 currentVector, Vector2 lastVector)
    {
        // If the joystick has crossed the threshold in any direction
        return (currentVector.y > 0.5f && lastVector.y <= 0.5f) ||
               (currentVector.y < -0.5f && lastVector.y >= -0.5f) ||
               (currentVector.x > 0.5f && lastVector.x <= 0.5f) ||
               (currentVector.x < -0.5f && lastVector.x >= -0.5f);
    }

    // Handles the actual selection based on the joystick vector for buttons and sliders
    private void HandleSelection(GameObject currentSelected, Vector2 joystickVector)
    {
        // Check if the currently selected UI element is a Slider
        Slider selectedSlider = currentSelected.GetComponent<Slider>();
        Scrollbar selectedScrollbar = currentSelected.GetComponent<Scrollbar>();

        if (selectedSlider != null)
        {
            // If it's a slider, modify its value based on joystick input
            if (joystickVector.x > 0.5f)
            {
                IncrementSlider(selectedSlider, sliderStep); // Move slider incrementally to the right
            }
            else if (joystickVector.x < -0.5f)
            {
                IncrementSlider(selectedSlider, -sliderStep); // Move slider incrementally to the left
            }
            if (joystickVector.y > 0.5f)
            {
                // Move Up
                SelectButton(currentSelected, MoveDirection.Up);
            }
            else if (joystickVector.y < -0.5f)
            {
                // Move Down
                SelectButton(currentSelected, MoveDirection.Down);
            }
        }
        else if (selectedScrollbar != null)
        {
            // If it's a scrollbar, skip single-select handling, let HandleContinuousScrollbar do its job
        }
        else
        {
            // If it's not a slider or scrollbar, handle the button selection
            if (joystickVector.y > 0.5f)
            {
                // Move Up
                SelectButton(currentSelected, MoveDirection.Up);
            }
            else if (joystickVector.y < -0.5f)
            {
                // Move Down
                SelectButton(currentSelected, MoveDirection.Down);
            }

            if (joystickVector.x > 0.5f)
            {
                // Move Right
                SelectButton(currentSelected, MoveDirection.Right);
            }
            else if (joystickVector.x < -0.5f)
            {
                // Move Left
                SelectButton(currentSelected, MoveDirection.Left);
            }
        }
    }

    // Handles continuous scrolling for a scrollbar
    private void HandleContinuousScrollbar(Scrollbar scrollbar, Vector2 joystickVector)
    {
        if (joystickVector.y > 0.1f) // Up direction
        {
            IncrementScrollbar(scrollbar, scrollSpeed * joystickVector.y); // Scroll up continuously

            // Check if we reached the top of the scrollbar
            if (Mathf.Approximately(scrollbar.value, 1f))
            {
                // Move to the next selectable item upwards if the scrollbar is at the top
                SelectButton(scrollbar.gameObject, MoveDirection.Up);
            }
        }
        else if (joystickVector.y < -0.1f) // Down direction
        {
            IncrementScrollbar(scrollbar, scrollSpeed * joystickVector.y); // Scroll down continuously

            // Check if we reached the bottom of the scrollbar
            if (Mathf.Approximately(scrollbar.value, 0f))
            {
                // Move to the next selectable item downwards if the scrollbar is at the bottom
                SelectButton(scrollbar.gameObject, MoveDirection.Down);
            }
        }
    }


    // Handles moving the selection based on the direction for buttons
    private void SelectButton(GameObject currentButton, MoveDirection direction)
    {
        // Get the Selectable component of the currently selected button
        Selectable selectable = currentButton.GetComponent<Selectable>();

        // Find the next selectable UI element in the specified direction
        if (selectable != null)
        {
            Selectable nextSelectable = null;

            switch (direction)
            {
                case MoveDirection.Up:
                    nextSelectable = selectable.FindSelectableOnUp();
                    break;
                case MoveDirection.Down:
                    nextSelectable = selectable.FindSelectableOnDown();
                    break;
                case MoveDirection.Left:
                    nextSelectable = selectable.FindSelectableOnLeft();
                    break;
                case MoveDirection.Right:
                    nextSelectable = selectable.FindSelectableOnRight();
                    break;
            }

            // If there is a next selectable UI element, select it
            if (nextSelectable != null)
            {
                eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
                // Invoke the event, passing the new selected GameObject and direction
                OnSelectionMoved?.Invoke(nextSelectable.gameObject, direction);
            }
        }
    }

    // Increment or decrement the slider
    private void IncrementSlider(Slider slider, float step)
    {
        slider.value = Mathf.Clamp(slider.value + step, slider.minValue, slider.maxValue);
    }

    // Increment or decrement the scrollbar
    private void IncrementScrollbar(Scrollbar scrollbar, float step)
    {
        scrollbar.value = Mathf.Clamp(scrollbar.value + step, 0f, 1f);
    }

    public void DestinationButton(Button destinationButton)
    {
        destinationButton.Select();
    }
}

// Enum to handle Move Directions
public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}

// Component for single-select mode toggle
public class SingleSelectModeComponent : MonoBehaviour
{
    public bool singleSelectMode = true; // Default to single select mode
}