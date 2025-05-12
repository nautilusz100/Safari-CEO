using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Model.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class gameUIButtonsManager : MonoBehaviour
{
    // UI and control references
    private UIDocument uIDocument;
    private Button natureButton;
    private Button animalButton;
    private Button jeepAndRoadButton;
    private Button parkStatButton;

    // Input field for park name
    [SerializeField] public TMP_InputField parkNameInputField;

    // Currently selected shop type
    private ShopTypes currentShopType = ShopTypes.NONE;
    public ShopTypes CurrentShopType { get { return currentShopType; } }

    // List of shop UI panels
    private List<Transform> shopElementPanels;

    // Sprites for shop items
    [SerializeField] private UnityEngine.Sprite bushImage;
    [SerializeField] private UnityEngine.Sprite treeImage;
    [SerializeField] private UnityEngine.Sprite flowerImage;
    [SerializeField] private UnityEngine.Sprite foxImage;
    [SerializeField] private UnityEngine.Sprite lionImage;
    [SerializeField] private UnityEngine.Sprite giraffeImage;
    [SerializeField] private UnityEngine.Sprite zebraImage;
    [SerializeField] private UnityEngine.Sprite roadImage;
    [SerializeField] private UnityEngine.Sprite jeepImage;

    // Sprites for park satisfaction levels
    [SerializeField] private UnityEngine.Sprite awfulIcon;
    [SerializeField] private UnityEngine.Sprite badIcon;
    [SerializeField] private UnityEngine.Sprite okayIcon;
    [SerializeField] private UnityEngine.Sprite goodIcon;
    [SerializeField] private UnityEngine.Sprite perfectIcon;

    // GameObjects to control shop and park stat panel visibility
    public GameObject enableShop;
    public GameObject enableParkStat;

    // Reference to GameManager for data access
    public GameManager gameManager;

    // Events for external systems to listen to UI interactions
    public event EventHandler NatureShopClicked;
    public event EventHandler AnimalShopClicked;
    public event EventHandler JeepShopClicked;
    public event EventHandler ParkStatClicked;

    // Allows skipping logic in test scenarios
    bool testMode = false;
    public void TestMode()
    {
        testMode = true;
    }

    // Initialization logic
    void Start()
    {
        if (testMode) { return; }

        uIDocument = GetComponent<UIDocument>();
        if (uIDocument == null)
        {
            Debug.LogError("UIDocument component not found.");
            return;
        }

        // Hook up all buttons from the UI document
        natureButton = uIDocument.rootVisualElement.Q<Button>("natureButton");
        if (natureButton == null) { return; }
        natureButton.clickable.clicked += OnNautreButtonClick;

        animalButton = uIDocument.rootVisualElement.Q<Button>("animalButton");
        if (animalButton == null) { return; }
        animalButton.clickable.clicked += OnAnimalButtonClick;

        jeepAndRoadButton = uIDocument.rootVisualElement.Q<Button>("roadAndJeepButton");
        if (jeepAndRoadButton == null) { return; }
        jeepAndRoadButton.clickable.clicked += OnJeepButtonClick;

        parkStatButton = uIDocument.rootVisualElement.Q<Button>("parkButton");
        if (parkStatButton == null) { return; }
        parkStatButton.clickable.clicked += OnParkStatButtonClick;

        // Populate panel list to control shop UI content
        CreateShopElementPanelList();

        // Update preview button text on name input
        parkNameInputField.onValueChanged.AddListener(UpdateParkNamePreview);
    }

    // Update satisfaction icon every frame if stats panel is open
    private void Update()
    {
        if (testMode) return;
        if (enableParkStat.activeSelf)
        {
            UpdateSatisfactionIcon();
        }
    }

    // Gather child panels to populate with shop items
    private void CreateShopElementPanelList()
    {
        shopElementPanels = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            shopElementPanels.Add(enableShop.transform.GetChild(i).GetChild(0));
        }
    }

    // Toggle park stats panel and update icon
    private void OnParkStatButtonClick()
    {
        enableParkStat.SetActive(!enableParkStat.activeSelf);
        UpdateSatisfactionIcon();
        ParkStatClicked?.Invoke(this, EventArgs.Empty);
    }

    // Determine which satisfaction icon to display
    private void UpdateSatisfactionIcon()
    {
        UnityEngine.Sprite icon = awfulIcon;
        float satisfaction = gameManager.satisfaction;

        if (satisfaction < 2) icon = awfulIcon;
        else if (satisfaction < 3) icon = badIcon;
        else if (satisfaction < 4) icon = okayIcon;
        else if (satisfaction < 4.5) icon = goodIcon;
        else if (satisfaction <= 5) icon = perfectIcon;

        enableParkStat.transform.Find("ParkSatisfactionPanel").Find("Image").GetComponent<UnityEngine.UI.Image>().sprite = icon;
    }

    // Live-update park name button with input text
    public void UpdateParkNamePreview(string value)
    {
        if (value == null || value == "")
        {
            parkStatButton.text = "Park Name";
            return;
        }
        parkStatButton.text = value;
    }

    // Open or close the shop panel based on button type
    private void ShopDisplayManager(ShopTypes shopType)
    {
        if (currentShopType == shopType)
        {
            enableShop.SetActive(false);
            currentShopType = ShopTypes.NONE;
            gameManager.IsBuilding = Tile.ShopType.None;
        }
        else
        {
            currentShopType = shopType;
            enableShop.SetActive(true);
            SetShopElements(shopType);
        }
    }

    // Set shop items and icons based on shop type
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

    // Apply name, price, and image to the correct panel
    private void SetShopDetails(int panelNumber, string panelName, string panelPrice, UnityEngine.Sprite shopSprite)
    {
        shopElementPanels[panelNumber].gameObject.SetActive(true);
        shopElementPanels[panelNumber].Find("ShopElementName").GetComponent<TextMeshProUGUI>().text = panelName;
        shopElementPanels[panelNumber].Find("ShopElementPrice").GetComponent<TextMeshProUGUI>().text = panelPrice;
        shopElementPanels[panelNumber].Find("ShopElementImagePanel").Find("ShopElementImage").GetComponent<UnityEngine.UI.Image>().sprite = shopSprite;
    }

    // Button handlers for each shop type
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

    // Handle selection of individual shop items
    public void SelectElement(int button)
    {
        switch (currentShopType)
        {
            case ShopTypes.NATURE:
                if (button == 0)
                {
                    gameManager.IsBuilding = Tile.ShopType.Flowerbed;
                }
                else if (button == 1)
                {
                    gameManager.IsBuilding = Tile.ShopType.Bush;
                }
                else if (button == 2)
                {
                    gameManager.IsBuilding = Tile.ShopType.Tree;
                }
                else
                {
                    gameManager.IsBuilding = Tile.ShopType.None;
                }
                break;

            case ShopTypes.ANIMALS:
                gameManager.IsBuilding = Tile.ShopType.Animal;
                break;

            case ShopTypes.JEEP:
                if (button == 0)
                {
                    gameManager.IsBuilding = Tile.ShopType.Road;
                }
                else
                {
                    gameManager.IsBuilding = Tile.ShopType.Jeep;
                    Debug.Log("Jeep selected");
                }
                break;

            default:
                break;
        }
    }
}