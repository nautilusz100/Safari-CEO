using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class gameUIButtonsManager : MonoBehaviour
{
    private UIDocument uIDocument;
    private Button natureButton;
    private Button animalButton;
    private Button jeepAndRoadButton;
    private Button parkStatButton;

    public GameObject enableShop;
    public GameObject enableParkStat;

    public event EventHandler NatureShopClicked;
    public event EventHandler AnimalShopClicked;
    public event EventHandler JeepShopClicked;
    public event EventHandler ParkStatClicked;
    void Start()
    {
        uIDocument = GetComponent<UIDocument>();
        if (uIDocument == null)
        {
            Debug.LogError("UIDocument component not found.");
            return;
        }

        natureButton = uIDocument.rootVisualElement.Q<Button>("natureButton");
        if (natureButton == null)
        {
            Debug.LogError("nautureButton Button not found.");
            return;
        }
        natureButton.clickable.clicked += OnNautreButtonClick;

        animalButton = uIDocument.rootVisualElement.Q<Button>("animalButton");
        if (animalButton == null)
        {
            Debug.LogError("animalButton Button not found.");
            return;
        }
        animalButton.clickable.clicked += OnAnimalButtonClick;

        jeepAndRoadButton = uIDocument.rootVisualElement.Q<Button>("roadAndJeepButton");
        if (jeepAndRoadButton == null)
        {
            Debug.LogError("jeepAndRoadButton Button not found.");
            return;
        }
        jeepAndRoadButton.clickable.clicked += OnJeepButtonClick;

        parkStatButton = uIDocument.rootVisualElement.Q<Button>("parkButton");
        if (parkStatButton == null)
        {
            Debug.LogError("parkStatButton Button not found.");
            return;
        }
        parkStatButton.clickable.clicked += OnParkStatButtonClick;
    }

    private void OnParkStatButtonClick()
    {
        enableParkStat.SetActive(!enableParkStat.activeSelf);
        ParkStatClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnNautreButtonClick()
    {
        enableShop.SetActive(!enableShop.activeSelf);
        NatureShopClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnAnimalButtonClick()
    {
        enableShop.SetActive(!enableShop.activeSelf);
        AnimalShopClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnJeepButtonClick()
    {
        enableShop.SetActive(!enableShop.activeSelf);
        JeepShopClicked?.Invoke(this, EventArgs.Empty);
    }
}
