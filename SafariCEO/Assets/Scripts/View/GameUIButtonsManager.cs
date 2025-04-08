using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class gameUIButtonsManager : MonoBehaviour
{
    private UIDocument uIDocument;
    private Button natureButton;
    private Button animalButton;
    private Button jeepAndRoadButton;
    private Button parkStatButton;

    private ShopTypes currentShopType = ShopTypes.NONE;
    private List<Transform> shopElementPanels;

    [SerializeField]private UnityEngine.Sprite bushImage;
    [SerializeField]private UnityEngine.Sprite treeImage;
    [SerializeField]private UnityEngine.Sprite flowerImage;
    [SerializeField]private UnityEngine.Sprite foxImage;
    [SerializeField]private UnityEngine.Sprite lionImage;
    [SerializeField]private UnityEngine.Sprite giraffeImage;
    [SerializeField]private UnityEngine.Sprite zebraImage;
    [SerializeField]private UnityEngine.Sprite roadImage;
    [SerializeField]private UnityEngine.Sprite jeepImage;
    
    public GameObject enableShop;
    public GameObject enableParkStat;
    public GameManager gameManager;

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

        CreateShopElementPanelList();

    }

    private void CreateShopElementPanelList()
    {
        shopElementPanels = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            shopElementPanels.Add(enableShop.transform.GetChild(i).GetChild(0));
        }
    }

    private void OnParkStatButtonClick()
    {
        enableParkStat.SetActive(!enableParkStat.activeSelf);
        ParkStatClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ShopDisplayManager(ShopTypes shopType)
    {
        if (currentShopType == shopType)
        {
            enableShop.SetActive(false);
            currentShopType = ShopTypes.NONE;
        }
        else
        {
            currentShopType = shopType;
            enableShop.SetActive(true);
            SetShopElements(shopType);
        }
    }

    private void SetShopElements(ShopTypes currentShop)
    {

        switch (currentShop)
        {
            case ShopTypes.NATURE:

                SetShopDetails(0, "Flowers", "$10", flowerImage);
                SetShopDetails(1, "Bushes", "$20", bushImage);
                SetShopDetails(2, "Trees", "$30", treeImage);
                shopElementPanels[3].gameObject.SetActive(false);

                break;
            case ShopTypes.ANIMALS:
                SetShopDetails(0, "Fox", "$10", foxImage);
                SetShopDetails(1, "Lion", "$20", lionImage);
                SetShopDetails(2, "Giraffe", "$30", giraffeImage);
                SetShopDetails(3, "Zebra", "$40", zebraImage);
                break;
            case ShopTypes.JEEP:
                SetShopDetails(0, "Road", "$10", roadImage);
                SetShopDetails(1, "Jeep", "$20", jeepImage);
                shopElementPanels[2].gameObject.SetActive(false);
                shopElementPanels[3].gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }
    private void SetShopDetails(int panelNumber, string panelName, string panelPrice, UnityEngine.Sprite shopSprite)
    {
        shopElementPanels[panelNumber].gameObject.SetActive(true);
        shopElementPanels[panelNumber].Find("ShopElementName").GetComponent<TextMeshProUGUI>().text = panelName;
        shopElementPanels[panelNumber].Find("ShopElementPrice").GetComponent<TextMeshProUGUI>().text = panelPrice;
        shopElementPanels[panelNumber].Find("ShopElementImagePanel").Find("ShopElementImage").GetComponent<UnityEngine.UI.Image>().sprite = shopSprite;
    }

    private void OnNautreButtonClick()
    {
        ShopDisplayManager(ShopTypes.NATURE);
        NatureShopClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnAnimalButtonClick()
    {
        ShopDisplayManager(ShopTypes.ANIMALS);
        AnimalShopClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnJeepButtonClick()
    {
        ShopDisplayManager(ShopTypes.JEEP);
        JeepShopClicked?.Invoke(this, EventArgs.Empty);
    }

    public void SelectElement(int button)
    {
        switch (currentShopType)
        {
            case ShopTypes.NATURE:
                if (button == 0)
                {
                    gameManager.IsBuilding = Tile.TileType.Flowerbed;
                }
                else if (button == 1)
                {
                    gameManager.IsBuilding = Tile.TileType.Bush;
                }
                else if (button == 2)
                {
                    gameManager.IsBuilding = Tile.TileType.Tree;
                }
                else
                {
                    gameManager.IsBuilding = Tile.TileType.None;
                }
                break;
            case ShopTypes.ANIMALS:
                gameManager.IsBuilding = Tile.TileType.None;
                break;
            case ShopTypes.JEEP:
                if (button == 0)
                {
                    gameManager.IsBuilding = Tile.TileType.Road;
                }
                else
                {
                    gameManager.IsBuilding = Tile.TileType.None;
                }
                break;
            default:
                break;
        }
    }
}
