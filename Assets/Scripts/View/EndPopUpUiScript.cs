using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EndPopUpUiScript : MonoBehaviour
{
    // Reference to the UI Document component attached to this GameObject
    private UIDocument uIDocumentEndPopUp;

    // UI element showing park statistics (e.g. name)
    [SerializeField] private TextMeshProUGUI uiParkStatsText;

    // Reference to the quit button in the UI
    private Button exitButton;

    // UI label showing game result ("You Win" or "You Lost")
    private Label endLabel;

    // UI label showing the name of the park
    private Label parkName;

    void Start()
    {
        // Get UIDocument component
        uIDocumentEndPopUp = GetComponent<UIDocument>();
        if (uIDocumentEndPopUp == null)
        {
            return; // Exit if UIDocument is missing
        }

        // Locate the "QuitButton" in the UI hierarchy
        exitButton = uIDocumentEndPopUp.rootVisualElement.Q<Button>("QuitButton");
        if (exitButton == null)
        {
            return; // Exit if the button is not found
        }

        // Register click handler for the quit button
        exitButton.clickable.clicked += OnExitButtonClick;

        // Locate the end result label ("You Win"/"You Lost")
        endLabel = uIDocumentEndPopUp.rootVisualElement.Q<Label>("EndLabel");
        if (endLabel == null)
        {
            Debug.LogError("EndLabel Label not found.");
            return;
        }

        // Locate the label displaying the park name
        parkName = uIDocumentEndPopUp.rootVisualElement.Q<Label>("ParkName");
        if (parkName == null)
        {
            Debug.LogError("ParkName Label not found.");
            return;
        }

        // Display result based on GameManager state
        endLabel.text = GameManager.Instance.HasWon ? "You Win" : "You Lost";

        // Set park name with prefix
        parkName.text = "As CEO of " + uiParkStatsText.text;
    }

    // Handle quit button click: invoke exit event and load main menu scene
    private void OnExitButtonClick()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
