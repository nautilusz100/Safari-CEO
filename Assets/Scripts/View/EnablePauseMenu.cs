using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnablePauseMenu class for managing the visibility of the pause menu.
/// <summary>
public class EnablePauseMenu : MonoBehaviour
{
    public GameObject enablePauseMenu;

    void Start()
    {
    }

    void Update()
    {
        // Toggle pause menu visibility when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            enablePauseMenu.SetActive(!enablePauseMenu.activeSelf);
        }
    }
}
