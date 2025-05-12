using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using System.Collections;
using Unity.VisualScripting;
using System.Net.NetworkInformation;
using System.IO;


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
        saveButton.clickable.clicked += OnSaveButtonClick;

        exitButton = uIDocument.rootVisualElement.Q<Button>("exitButton");
        if (exitButton == null)
        {
            Debug.LogError("exitButton Button not found.");
            return;
        }
        exitButton.clickable.clicked += OnExitButtonClick;
    }

    private void OnExitButtonClick()
    {
        ExitMenu?.Invoke(this, EventArgs.Empty);
        SceneManager.LoadScene("MainMenu");
    }


    private void OnSaveButtonClick()
    {
#if UNITY_EDITOR
        string path = EditorUtility.SaveFilePanel("Save Game", "", "savegame.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = GameManager.Instance.SaveGame();
            File.WriteAllText(path, json);
        }
#else
        StartCoroutine(ShowSaveDialog());
#endif
    }

    private IEnumerator ShowSaveDialog()
    {
        SimpleFileBrowser.FileBrowser.SetFilters(false, new SimpleFileBrowser.FileBrowser.Filter("Save Files", ".json"));
        SimpleFileBrowser.FileBrowser.SetDefaultFilter(".json");

        SimpleFileBrowser.FileBrowser.ShowSaveDialog((paths) =>
        {
            if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                string json = GameManager.Instance.SaveGame();
                File.WriteAllText(paths[0], json);
            }
        },
        () => Debug.Log("File save canceled"),
        SimpleFileBrowser.FileBrowser.PickMode.Files, false, null, "savegame.saf");

        yield return null;
    }







}

