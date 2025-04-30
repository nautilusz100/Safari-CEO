using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EndPopUpUiScript : MonoBehaviour
{
    private UIDocument uIDocumentEndPopUp;
    [SerializeField] private TextMeshProUGUI uiParkStatsText;
    private Button exitButton;
    private Label endLabel;
    private Label parkName;
    public event EventHandler ExitMenu;


    void Start()
    {
        uIDocumentEndPopUp = GetComponent<UIDocument>();
        if (uIDocumentEndPopUp == null)
        {
            Debug.LogError("UIDocument component not found.");
            return;
        }


        exitButton = uIDocumentEndPopUp.rootVisualElement.Q<Button>("QuitButton");
        if (exitButton == null)
        {
            Debug.LogError("exitButton Button not found.");
            return;
        }
        exitButton.clickable.clicked += OnExitButtonClick;


        endLabel = uIDocumentEndPopUp.rootVisualElement.Q<Label>("EndLabel");
        if (endLabel == null)
        {
            Debug.LogError("EndLabel Label not found.");
            return;
        }
        parkName = uIDocumentEndPopUp.rootVisualElement.Q<Label>("ParkName");
        if (parkName == null)
        {
            Debug.LogError("ParkName Label not found.");
            return;
        }
        endLabel.text = GameManager.Instance.HasWon ? "You Win" : "You Lost";

        parkName.text = "As CEO of " + uiParkStatsText.text;
    }

    private void OnExitButtonClick()
    {
        ExitMenu?.Invoke(this, EventArgs.Empty);
        SceneManager.LoadScene("MainMenu");
    }
}
