using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablePauseMenu : MonoBehaviour
{
    public GameObject enablePauseMenu;  // Az egész objektum, nem csak komponens

    void Start()
    {
        enablePauseMenu.SetActive(false);  // A GameObjectet inaktívvá tesszük
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // A GameObjectet aktiváljuk vagy deaktiváljuk
            enablePauseMenu.SetActive(!enablePauseMenu.activeSelf);
        }
    }
}
