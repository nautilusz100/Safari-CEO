using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using System.Collections;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenu : MonoBehaviour
{
    private UIDocument uIDocument;
    private Button saveButton;
    private Button exitButton;
    public event EventHandler SaveGame;
    public event EventHandler ExitMenu;


    void Start()
    {
        uIDocument = GetComponent<UIDocument>();
        if (uIDocument == null)
        {
            Debug.LogError("UIDocument component not found.");
            return;
        }

        saveButton = uIDocument.rootVisualElement.Q<Button>("saveButton");
        if (saveButton == null)
        {
            Debug.LogError("saveButton Button not found.");
            return;
        }
        saveButton.clickable.clicked += OnSaveButtonnClick;

        exitButton = uIDocument.rootVisualElement.Q<Button>("exitButton");
        if (exitButton == null)
        {
            Debug.LogError("exitButton Button not found.");
            return;
        }
        exitButton.clickable.clicked += OnExitButtonClick;
    }

    private void OnDestroy()
    {
        if (saveButton != null)
        {
            saveButton.clickable.clicked -= OnSaveButtonnClick;
        }
        if (exitButton != null)
        {
            exitButton.clickable.clicked -= OnExitButtonClick;
        }
    }

    private void OnSaveButtonnClick()
    {
        #if UNITY_EDITOR
                string path = EditorUtility.SaveFilePanel("Save Game", "", "savegame.saf", "saf");
                if (!string.IsNullOrEmpty(path))
                {
                    Debug.Log("Saving to file: " + path);
                    SaveGame?.Invoke(this, EventArgs.Empty);
                }
        #else
            StartCoroutine(ShowSaveDialog());
        #endif
       

    }
    private void OnExitButtonClick()
    {
        ExitMenu?.Invoke(this, EventArgs.Empty);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator ShowSaveDialog()
    {
        SimpleFileBrowser.FileBrowser.SetFilters(false, new SimpleFileBrowser.FileBrowser.Filter("Save Files", ".saf"));
        SimpleFileBrowser.FileBrowser.SetDefaultFilter(".saf");

        SimpleFileBrowser.FileBrowser.ShowSaveDialog((path) =>
        {
            if (!string.IsNullOrEmpty(path[0]))
            {
                Debug.Log("Saving to file: " + path[0]);
                SaveGame?.Invoke(this, EventArgs.Empty);
            }
        },
        () => Debug.Log("File save canceled"),
        SimpleFileBrowser.FileBrowser.PickMode.Files, false, null, "savegame.saf");

        yield return null;
    }



   
}
