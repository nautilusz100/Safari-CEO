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

    /*
    public event EventHandler LoadGame;
    public event EventHandler ExitMenu;
    */

    void Start()
    {
        uIDocument = GetComponent<UIDocument>();
        if (uIDocument == null)
        {
            Debug.LogError("UIDocument component not found.");
            return;
        }

        startButton = uIDocument.rootVisualElement.Q<Button>("MenuStartButton");
        if (startButton == null)
        {
            Debug.LogError("Start Button not found.");
            return;
        }
        startButton.clickable.clicked += OnStartButtonClick;

        loadButton = uIDocument.rootVisualElement.Q<Button>("MenuLoadButton");
        if (loadButton == null)
        {
            Debug.LogError("Load Button not found.");
            return;
        }
        loadButton.clickable.clicked += OnLoadButtonClick;

        exitButton = uIDocument.rootVisualElement.Q<Button>("MenuExitButton");
        if (exitButton == null)
        {
            Debug.LogError("Exit Button not found.");
            return;
        }
        exitButton.clickable.clicked += OnExitButtonClick;

        easyButton = uIDocument.rootVisualElement.Q<Button>("MenuEasyButton");
        if (easyButton == null)
        {
            Debug.LogError("Easy Button not found.");
            return;
        }
        easyButton.clickable.clicked += OnEasyButtonClick;

        mediumButton = uIDocument.rootVisualElement.Q<Button>("MenuMediumButton");
        if (mediumButton == null)
        {
            Debug.LogError("Medium Button not found.");
            return;
        }
        mediumButton.clickable.clicked += OnMediumButtonClick;

        hardButton = uIDocument.rootVisualElement.Q<Button>("MenuHardButton");
        if (hardButton == null)
        {
            Debug.LogError("Hard Button not found.");
            return;
        }
        hardButton.clickable.clicked += OnHardButtonClick;
    }



    private void OnStartButtonClick()
    {

        if (selectedDifficulty == Difficulty.None)
        {
            Debug.Log("No difficulty selected!");
        }
        else
        {
            GameSettings.SelectedDifficulty = selectedDifficulty;
            SceneManager.LoadScene("Game");
        }

    }
    private void OnLoadButtonClick()
    {
#if UNITY_EDITOR
        Debug.Log("OnLoadButtonClick called");
        string path = EditorUtility.OpenFilePanel("Select a file", "", "json");
        Debug.Log("Path after OpenFilePanel: " + path);  // Logoljuk a path-ot

        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Selected file: " + path);
            LoadSettings.LoadPath = path;
            SceneManager.LoadScene("Game"); // Betöltjük a Game jelenetet
        }
        else
        {
            Debug.LogWarning("No file selected or path is empty");
        }
#else
    StartCoroutine(ShowLoadDialog());
#endif
    }


    private IEnumerator ShowLoadDialog()
    {
        SimpleFileBrowser.FileBrowser.SetFilters(true, new SimpleFileBrowser.FileBrowser.Filter("Save Files", ".saf"), new SimpleFileBrowser.FileBrowser.Filter("All Files", "*"));

        SimpleFileBrowser.FileBrowser.ShowLoadDialog((paths) =>
        {
            if (paths.Length > 0)
            {
                Debug.Log("Selected file: " + paths[0]);
                LoadSettings.LoadPath = paths[0];
                SceneManager.LoadScene("Game"); // Betöltjük a Game jelenetet
            }
        }, () => Debug.Log("File selection canceled"), SimpleFileBrowser.FileBrowser.PickMode.Files);

        yield return null;
    }

    private void OnExitButtonClick()
    {
        Debug.Log("Quit");
        //ExitMenu?.Invoke(this, EventArgs.Empty);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

    }

    private void OnEasyButtonClick()
    {
        selectedDifficulty = Difficulty.Easy;
        SetSelectedButton(easyButton);
    }

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