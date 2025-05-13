using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// This class represents the main menu of the game.
/// </summary>

public class MainMenu : MonoBehaviour
{
    private UIDocument uIDocument;
    private Button startButton;
    private Button loadButton;
    private Button exitButton;
    private Button easyButton;
    private Button mediumButton;
    private Button hardButton;
    private Difficulty selectedDifficulty = Difficulty.None;


    void Start()
    {
        uIDocument = GetComponent<UIDocument>();
        if (uIDocument == null)
        {
            return;
        }

        startButton = uIDocument.rootVisualElement.Q<Button>("MenuStartButton");
        if (startButton == null)
        {
            return;
        }
        startButton.clickable.clicked += OnStartButtonClick;

        loadButton = uIDocument.rootVisualElement.Q<Button>("MenuLoadButton");
        if (loadButton == null)
        {
            return;
        }
        loadButton.clickable.clicked += OnLoadButtonClick;

        exitButton = uIDocument.rootVisualElement.Q<Button>("MenuExitButton");
        if (exitButton == null)
        {

            return;
        }
        exitButton.clickable.clicked += OnExitButtonClick;

        easyButton = uIDocument.rootVisualElement.Q<Button>("MenuEasyButton");
        if (easyButton == null)
        {
            return;
        }
        easyButton.clickable.clicked += OnEasyButtonClick;

        mediumButton = uIDocument.rootVisualElement.Q<Button>("MenuMediumButton");
        if (mediumButton == null)
        {
            return;
        }
        mediumButton.clickable.clicked += OnMediumButtonClick;

        hardButton = uIDocument.rootVisualElement.Q<Button>("MenuHardButton");
        if (hardButton == null)
        {
            return;
        }
        hardButton.clickable.clicked += OnHardButtonClick;
    }


    /// <summary>
    /// Start the game with the selected difficulty.
    /// </summary>
    private void OnStartButtonClick()
    {

        if (selectedDifficulty == Difficulty.None)
        {
        }
        else
        {
            GameSettings.SelectedDifficulty = selectedDifficulty;
            SceneManager.LoadScene("Game");
        }

    }
    /// <summary>
    /// Load a game from a file.
    /// </summary>>
    private void OnLoadButtonClick()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Select a file", "", "json");

        if (!string.IsNullOrEmpty(path))
        {
            LoadSettings.LoadPath = path;
            SceneManager.LoadScene("Game"); // Betöltjük a Game jelenetet
        }
#else
    StartCoroutine(ShowLoadDialog());
#endif
    }
    /// <summary>
    /// Load a game from a file using SimpleFileBrowser.
    /// </summary>

    private IEnumerator ShowLoadDialog()
    {
        SimpleFileBrowser.FileBrowser.SetFilters(true, new SimpleFileBrowser.FileBrowser.Filter("Save Files", ".saf"), new SimpleFileBrowser.FileBrowser.Filter("All Files", "*"));

        SimpleFileBrowser.FileBrowser.ShowLoadDialog((paths) =>
        {
            if (paths.Length > 0)
            {
                LoadSettings.LoadPath = paths[0];
                SceneManager.LoadScene("Game"); // Betöltjük a Game jelenetet
            }
        }, () => Debug.Log("File selection canceled"), SimpleFileBrowser.FileBrowser.PickMode.Files);

        yield return null;
    }

    /// <summary>
    /// Exit the game.
    /// </summary>
    private void OnExitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

    }
    /// <summary>
    /// Set the selected difficulty to Easy.
    /// </summary>
    private void OnEasyButtonClick()
    {
        selectedDifficulty = Difficulty.Easy;
        SetSelectedButton(easyButton);
    }
    /// <summary>
    /// Set the selected difficulty to Medium.
    /// </summary>
    private void OnMediumButtonClick()
    {
        selectedDifficulty = Difficulty.Medium;
        SetSelectedButton(mediumButton);
    }

    private void OnHardButtonClick()
    {
        selectedDifficulty = Difficulty.Hard;
        SetSelectedButton(hardButton);
    }
    /// <summary>
    /// Set the selected button and update the UI accordingly.
    /// </summary>
    private void SetSelectedButton(Button selectedButton)
    {
        easyButton.RemoveFromClassList("difficultybuttonSelected");
        easyButton.RemoveFromClassList("diffcultybuttonBlurred");
        mediumButton.RemoveFromClassList("difficultybuttonSelected");
        mediumButton.RemoveFromClassList("diffcultybuttonBlurred");
        hardButton.RemoveFromClassList("difficultybuttonSelected");
        hardButton.RemoveFromClassList("diffcultybuttonBlurred");

        selectedButton.AddToClassList("difficultybuttonSelected");

        if (selectedButton != easyButton)
        {
            easyButton.AddToClassList("diffcultybuttonBlurred");
        }
        if (selectedButton != mediumButton)
        {
            mediumButton.AddToClassList("diffcultybuttonBlurred");
        }
        if (selectedButton != hardButton)
        {
            hardButton.AddToClassList("diffcultybuttonBlurred");
        }
    }
}