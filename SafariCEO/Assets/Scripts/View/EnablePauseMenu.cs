using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablePauseMenu : MonoBehaviour
{
    public GameObject enablePauseMenu;

    void Start()
    {
        //enablePauseMenu.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            enablePauseMenu.SetActive(!enablePauseMenu.activeSelf);
        }
    }
}
