using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Net;
using UnityEngine.UIElements;
using static Draggable;
//https://www.flaticon.com/free-icons/next icons credit - Flaticon

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private SafariMap currentMap;
    public SafariMap safariMapPrefab;
    [SerializeField]private int jeepCount = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI visitorCount;

    //difficulty seettings
    [SerializeField] private Difficulty gameDifficulty;

    //winning conditions
    private int howManyAnimalsNeededCarnivorous;
    private int howManyAnimalsNeededHerbivore;
    private int howManyDaysNeeded;
    //private int howMuchMoneyNeeded;

    //shop prices
    private int roadPrice = 10;
    private int jeepPrice = 20;
    private int foxPrice = 10;
    private int lionPrice = 20;
    private int giraffePrice = 30;
    private int zebraPrice = 40;
    private int flowerPrice = 10;
    private int bushPrice = 20;
    private int treePrice = 30;




    //public GameObject animalPrefab;
    public int EntryFee { get; set; }
    public int Visitors { get; set; }

    [SerializeField] private GameObject uiGameObject;
    private Label moneyLabel;
    private UnityEngine.UIElements.Button dateButton;
    private UnityEngine.UIElements.Button speedButton;
    [SerializeField] private Texture2D normalTimeArrow;
    [SerializeField] private Texture2D doubleTimeArrow;
    [SerializeField] private Texture2D tripleTimeArrow;
    private int money;
    public int Money
    {
        get => money;
        set
        {
            money = value;
            if (moneyLabel != null)
            {
                moneyLabel.text = money + "$";
            }
        }
    }
    private int hoursPassed;
    public int TimePassed
    {
        get => hoursPassed;
        set
        {
            hoursPassed = value;
            if (dateButton != null)
            {
                int monthsPassed = hoursPassed / 720; // 30 days * 24 hours
                int daysPassed = (hoursPassed % 720) / 24;
                int hours = (hoursPassed % 720) % 24;

                dateButton.text =
                    "M: " + monthsPassed.ToString("D2") +
                    " D: " + daysPassed.ToString("D2") +
                    " H: " + hours.ToString("D2");
            }
        }
    }
    public enum GameSpeed
    {
        Normal = 1,
        Double = 2,
        Triple = 3
    }
    public GameSpeed CurrentGameSpeed { get; private set; } = GameSpeed.Normal;


    public GameObject navMesh;
    private NavMeshSurface navMeshSurface;

    public Tile.ShopType IsBuilding { get; set; } = Tile.ShopType.Animal;

    private float timer; // Real-world timer in seconds
    [SerializeField] private float secondsPerGameHour = 1f; // 1 second = 1 game hour (adjustable)
    private float timeAccumulator = 0f;

    // Singleton GameManager
    void Start()
    {

        EntryFee = 100;
        Visitors = 0;
        scoreText.text = "$" + EntryFee.ToString();
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Ha van már térkép, akkor töröljük
        if (currentMap != null)
        {
            Destroy(currentMap.gameObject);
        }

        currentMap = safariMapPrefab;
        navMeshSurface = navMesh.GetComponent<NavMeshSurface>();
        // Térkép generálása
        currentMap.CreateMap();
        // NavMesh generálása
        navMeshSurface.BuildNavMesh();

        var uiDocument = uiGameObject.GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        moneyLabel = root.Q<Label>("MoneyLabel");
        dateButton = root.Q<UnityEngine.UIElements.Button>("dateButton");
        speedButton = root.Q<UnityEngine.UIElements.Button>("speedButton");


        //Game difficulty
        gameDifficulty = GameSettings.SelectedDifficulty;
        if (gameDifficulty == Difficulty.None)
        {
            gameDifficulty = Difficulty.Easy; // Default difficulty
        }
        Debug.Log("Difficulty from static: " + gameDifficulty);
        // Set the initial difficulty
        SetDifficulty();

        speedButton.clicked += ChangeSpeed;
        UpdateSpeedButton();
        //set game speed
        CurrentGameSpeed = GameSpeed.Normal;
        // set start date
        TimePassed = 0;
    }

    private void SetDifficulty()
    {
        switch (gameDifficulty)
        {
            case Difficulty.Easy:
                Money = 1000;
                howManyAnimalsNeededCarnivorous = 20;
                howManyAnimalsNeededHerbivore = 20;
                howManyDaysNeeded = 180;
                break;
            case Difficulty.Medium:
                Money = 750;
                howManyAnimalsNeededCarnivorous = 30;
                howManyAnimalsNeededHerbivore = 30;
                howManyDaysNeeded = 270;
                break;
            case Difficulty.Hard:
                Money = 500;
                howManyAnimalsNeededCarnivorous = 50;
                howManyAnimalsNeededHerbivore = 50;
                howManyDaysNeeded = 360;
                break;
        }
    }
    internal void NotifyTileFoodDepleted(Vector2Int pos)
    {
        currentMap.ReplaceTileWithPlains(pos);
    }
    void Update()
    {

        timeAccumulator += Time.deltaTime * (int)CurrentGameSpeed; // gyorsítás szorzás
        // Update the timer
        if (timeAccumulator >= 1f) // 1 másodperc eltelt
        {
            TimePassed += 1; // 1 órát növelünk
            timeAccumulator -= 1f;
        }

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (IsBuilding == Tile.ShopType.Road)
            {
                // Left click event
                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    // Raycast from camera position
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                    if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles") && Money >= roadPrice)
                    {
                        // Debug message for the clicked object
                        Debug.Log("Clicked object: " + hit.collider.gameObject.name);

                        Vector2 tilePosition = hit.collider.gameObject.transform.position;
                        currentMap.ChangeTileToRoad(tilePosition);

                        Money = Money -roadPrice;
                        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
                    }
                }
            }
            else if ((int)IsBuilding < 3)
            {
                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    // Raycast from camera position
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                    int price = 0;

                    switch (IsBuilding)
                    {
                        case Tile.ShopType.Flowerbed:
                            price = flowerPrice;
                            break;
                        case Tile.ShopType.Bush:
                            price = bushPrice;
                            break;
                        case Tile.ShopType.Tree:
                            price = treePrice;
                            break;
                        default:
                            Debug.Log("Invalid building type");
                            return;
                    }

                    if (Money < price)
                    {
                        Debug.Log("Not enough money for " + IsBuilding);
                        return;
                    }
                    Money -= price;

                    if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
                    {
                        // Debug message for the clicked object
                        Debug.Log("Clicked object: " + hit.collider.gameObject.name);

                        // Option: Change tile nature
                        Vector2 tilePosition = hit.collider.gameObject.transform.position;
                        currentMap.ChangeTileNature(tilePosition, IsBuilding);
                        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);


                    }
                }
            }
        }
        else if (IsBuilding == Tile.ShopType.Jeep && Money >= jeepPrice)
        {
            jeepCount++;
            IsBuilding = Tile.ShopType.None;
            Money = Money - jeepPrice;
        }

        AttemptJeepSpawn();

    }

    private void AttemptJeepSpawn()
    {
        if(jeepCount > 0)
        {
            // Spawn a new Jeep
            GameObject jeep = Instantiate(currentMap.prefab_jeep, new Vector3(39f,39.5f,0f) , Quaternion.identity );
            jeepCount--;
        }
    }

    // Útépítési mód engedélyezése
    public void EnableRoadBuilding()
    {
        IsBuilding = Tile.ShopType.Road;
    }

    // Útépítési mód letiltása
    public void DisableRoadBuilding()
    {
        IsBuilding = Tile.ShopType.None;
    }
    public void PriceIncrease()
    { 
        Debug.Log("PriceIncrease");
        EntryFee++;
        scoreText.text = "$" + EntryFee.ToString();
    }
    public void PriceDecrease()
    {
        EntryFee--;
        scoreText.text = "$" + EntryFee.ToString();
    }

    public void UpdateVisitorCount()
    {
        visitorCount.text = Visitors.ToString();
    }
    public int GetAnimalPrice(AnimalType type)
    {
        switch (type)
        {
            case AnimalType.Fox:
                return foxPrice;
            case AnimalType.Lion:
                return lionPrice;
            case AnimalType.Giraffe:
                return giraffePrice;
            case AnimalType.Zebra:
                return zebraPrice;
            default:
                return int.MaxValue; // unknown type
        }
    }
    private void ChangeSpeed()
    {
        switch (CurrentGameSpeed)
        {
            case GameSpeed.Normal:
                CurrentGameSpeed = GameSpeed.Double;
                break;
            case GameSpeed.Double:
                CurrentGameSpeed = GameSpeed.Triple;
                break;
            case GameSpeed.Triple:
                CurrentGameSpeed = GameSpeed.Normal;
                break;
        }
        UpdateSpeedButton();
    }
    private void UpdateSpeedButton()
    {
        if (speedButton == null) return;

        switch (CurrentGameSpeed)
        {
            case GameSpeed.Normal:

                speedButton.style.backgroundImage = normalTimeArrow;
                speedButton.style.unitySliceScale = 0;
                break;
            case GameSpeed.Double:
                speedButton.style.backgroundImage = doubleTimeArrow;
                speedButton.style.unitySliceScale  = 0;
                break;
            case GameSpeed.Triple:
                speedButton.style.backgroundImage = tripleTimeArrow;
                speedButton.style.unitySliceScale = 6;
                break;
        }
    }



}
