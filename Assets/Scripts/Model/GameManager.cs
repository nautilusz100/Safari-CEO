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
using Assets.Scripts.Model.Map;
using System.IO;
using System.Linq;
//https://www.flaticon.com/free-icons/next icons credit - Flaticon


/// <summary>
/// GameManager is a singleton class that manages the game state, including the map, animals, visitors, and game speed.
/// </summary>

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    private SafariMap currentMap;
    public SafariMap safariMapPrefab;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI visitorCount;
    public TextMeshProUGUI jeepCountText;
    public TMP_InputField parkName;
    [SerializeField] private GameObject fox_prefab;
    [SerializeField] private GameObject lion_prefab;
    [SerializeField] private GameObject zebra_prefab;
    [SerializeField] private GameObject giraffe_prefab;
    [SerializeField] private gameUIButtonsManager gameUIButtonsManager;

    // Jeep & Tourist related information
    private float visitorInterval = 5f; // Time in seconds between each visitor
    public float visitorTimer = 0f; // Timer to track the interval
    public int jeepCount = 0;
    public int totalJeepCount = 0;

    public float satisfaction = 0;
    
    //difficulty seettings
    [SerializeField] private Difficulty gameDifficulty;

    //winning conditions
    private int howManyVisitorsNeeded;
    private int howManyAnimalsNeededCarnivorous;
    private int howManyAnimalsNeededHerbivore;
    private int howManyDaysNeeded;
    private int howMuchMoneyNeeded;
    [SerializeField] private GameObject endGameScreen;
    public bool HasWon { get; private set; }
    private bool HasLost { get; set; }

    //current conditions
    public int Visitors { get; set; }
    public int CurrentCarnivorousCount { get; set; }
    public int CurrentHerbivoresCount { get; set; }
    private int hoursPassed;
    private int money;




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


    [SerializeField] private GameObject uiGameObject;
    private Label moneyLabel;
    private UnityEngine.UIElements.Button dateButton;
    private UnityEngine.UIElements.Button speedButton;
    [SerializeField] private Texture2D normalTimeArrow;
    [SerializeField] private Texture2D doubleTimeArrow;
    [SerializeField] private Texture2D tripleTimeArrow;
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
    public bool testMode = false;
    /// UI elements
    public int TimePassed
    {
        get => hoursPassed;
        set
        {
            hoursPassed = value;
            if (dateButton != null)
            {
                int yearsPassed = hoursPassed / 8640;
                int monthsPassed =( hoursPassed % 8640)/ 720; // 30 days * 24 hours
                int daysPassed = ((hoursPassed % 8640) % 720) / 24;
                int hours = ((hoursPassed % 8640) % 720) % 24;

                dateButton.text = "Y: "+ yearsPassed.ToString("D2") +
                    " M: " + monthsPassed.ToString("D2") +
                    " D: " + daysPassed.ToString("D2") +
                    " H: " + hours.ToString("D2");
            }
        }
    }
    public enum GameSpeed
    {
        Normal = 1,
        Double = 6,
        Triple = 12
    }
    public GameSpeed CurrentGameSpeed { get; set; } = GameSpeed.Normal;

    // UI elements
    public GameObject navMesh;
    private NavMeshSurface navMeshSurface;

    public Tile.ShopType IsBuilding { get; set; } = Tile.ShopType.Animal;

    private float timer; // Real-world timer in seconds
    [SerializeField] private float secondsPerGameHour = 5f; // 1 second = 1 game hour (adjustable)
    private float timeAccumulator = 0f;
    public float ScaledDeltaTime { get; private set; }

    //for saving
    public List<GameObject> Animals { get; set; } = new List<GameObject>();
    public bool isLoadedGame = false;
    public SaveData loadedSave;

    

    // Singleton GameManager
    void Start()
    {

        if (testMode) return;

        if (LoadSettings.IsLoadRequested)
        {
            LoadedStart();
            LoadSettings.LoadPath = null; // Kiürítjük, hogy ne töltsön újra véletlenül
        }
        else
            NormalStart();

    }

    /// <summary>
    /// Initializes the game manager, sets up the map, nav mesh, and UI elements.
    /// </summary>
    void NormalStart()
    {
        if (testMode) return; //For testing classes that refer to gamemanager
        scoreText.text = "$" + EntryFee.ToString();
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // If the game is loaded, we don't want to create a new map
        if (currentMap != null)
        {
            Destroy(currentMap.gameObject);
        }

        currentMap = safariMapPrefab;
        navMeshSurface = navMesh.GetComponent<NavMeshSurface>();
        // Map generation
        currentMap.CreateMap();
        // NavMesh generation
        navMeshSurface.BuildNavMesh();

        // UI initialization
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
        // Set the initial difficulty
        SetDifficulty();

        speedButton.clicked += ChangeSpeed;
        UpdateSpeedButton();
        //set game speed
        CurrentGameSpeed = GameSpeed.Normal;
        // set start date
        TimePassed = 0;

        CurrentCarnivorousCount = 0;
        CurrentHerbivoresCount = 0;

        HasWon = false;
        HasLost = false;

        // Set the initial money and entry fee
        EntryFee = 50;
        scoreText.text = "$" + EntryFee.ToString();
        visitorTimer = visitorInterval;
        Visitors = 0;
        UpdateVisitorCount();
    }
    /// <summary>
    /// Loads the game from a saved file.
    /// </summary>
    private void LoadedStart()
    {
        NormalStart();
        string path = LoadSettings.LoadPath;

        string json = File.ReadAllText(path);
        loadedSave = JsonUtility.FromJson<SaveData>(json);
        isLoadedGame = true;



        // Map data load
        currentMap.LoadTiles(loadedSave.tiles);
        LoadAnimals(loadedSave.animals);

        Invoke(nameof(ApplyLoadedGameData), 0.1f); // késleltetett hívás, hogy betöltsön a UI

    }

    /// <summary>
    /// Applies the loaded game data to the game manager.
    /// </summary>
    public void ApplyLoadedGameData()
    {
        SaveData save = loadedSave;
        // GameManager értékek beállítása
        Money = save.gameManager.money;
        jeepCount = save.gameManager.jeepCount;
        TimePassed = save.gameManager.time;


        // Set the game name
        if (gameUIButtonsManager != null)
        {
            gameUIButtonsManager.UpdateParkNamePreview(save.gameManager.parkName);
        }

        // Set the game difficulty
        switch (save.gameManager.difficulty)
        {
            case 0:
                gameDifficulty = Difficulty.Easy;
                break;
            case 1:
                gameDifficulty = Difficulty.Medium;
                break;
            case 2:
                gameDifficulty = Difficulty.Hard;
                break;
            default:
                gameDifficulty = Difficulty.None;
                break;
        }
        // Set the game settings from the loaded game
        EntryFee = save.gameManager.entryFee;
        satisfaction = save.gameManager.satisfaction;
        Visitors = save.gameManager.visitorCount;
        CurrentCarnivorousCount = save.gameManager.howManyCarnivores;
        CurrentHerbivoresCount = save.gameManager.howManyHerbivores;



        // UI update
        scoreText.text = "$" + EntryFee.ToString();
        visitorTimer = visitorInterval;
        UpdateJeepCount();
        UpdateVisitorCount();
    }

    private void SetDifficulty()
    {
        // Set the difficulty based on the selected difficulty level
        switch (gameDifficulty)
        {
            case Difficulty.Easy:
                Money = 1000;
                howManyVisitorsNeeded = 25;
                howManyAnimalsNeededCarnivorous = 20;
                howManyAnimalsNeededHerbivore = 20;
                howManyDaysNeeded = 90;
                howMuchMoneyNeeded = 2000;
                break;
            case Difficulty.Medium:
                Money = 750;
                howManyVisitorsNeeded = 50;
                howManyAnimalsNeededCarnivorous = 30;
                howManyAnimalsNeededHerbivore = 30;
                howManyDaysNeeded = 180;
                howMuchMoneyNeeded = 3000;
                break;
            case Difficulty.Hard:
                Money = 500;
                howManyVisitorsNeeded = 75;
                howManyAnimalsNeededCarnivorous = 50;
                howManyAnimalsNeededHerbivore = 50;
                howManyDaysNeeded = 360;
                howMuchMoneyNeeded = 4000;
                break;
        }
    }
    /// <summary>
    /// Notifies the game manager that food on a tile has been depleted.
    /// </summary>
    /// <param name="pos"></param>
    public virtual void  NotifyTileFoodDepleted(Vector2Int pos)
    {
        currentMap.ReplaceTileWithPlains(pos);
    }
    /// <summary>
    /// Checks the winning conditions of the game.
    /// </summary>
    private void WinningConditionCheck()
    {
        if (HasWon) return; // ha már nyertél, nem ellenőrzünk többször

        bool enoughVisitors = Visitors >= howManyVisitorsNeeded;
        bool enoughCarnivores = CurrentCarnivorousCount >= howManyAnimalsNeededCarnivorous;
        bool enoughHerbivores = CurrentHerbivoresCount >= howManyAnimalsNeededHerbivore;
        bool enoughDaysPassed = (TimePassed / 24) >= howManyDaysNeeded; // órából napra váltunk
        bool enoughMoney = Money >= howMuchMoneyNeeded;

        if (enoughVisitors && enoughCarnivores && enoughHerbivores && enoughDaysPassed && enoughMoney)
        {
            Debug.Log("You Win!");
            HasWon = true;
            if (endGameScreen != null)
            {
                endGameScreen.SetActive(true);
            }
        }
        if (Money < 0 || (CurrentCarnivorousCount < 0 && CurrentHerbivoresCount < 0)) //ha elfogyott a pénz vagy az összes állat elpusztult
        {
            HasLost = true;
            endGameScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Updates the game state every frame.
    /// </summary>
    void Update()
    {
        if (testMode) return;
        ScaledDeltaTime = (Time.deltaTime * (int)CurrentGameSpeed) / secondsPerGameHour;
        timeAccumulator += ScaledDeltaTime; // gyorsítás szorzás
        // Update the timer
        if (timeAccumulator >= 1)
        {
            int fullHoursPassed = Mathf.FloorToInt(timeAccumulator);
            TimePassed += fullHoursPassed; // 1 órát növelünk
            timeAccumulator -= fullHoursPassed;
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
                        // Option: Change tile nature
                        Vector2 tilePosition = hit.collider.gameObject.transform.position;
                        Tile tile = hit.transform.gameObject.GetComponent<Tile>();
                        if (tile != null)
                        {
                            if (tile.IsLocked) return;
                        currentMap.ChangeTileToRoad(tilePosition);
                        }
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
                    Tile tile = hit.transform.gameObject.GetComponent<Tile>();
                    if (tile != null)
                    {
                        if (tile.IsLocked) return;
                    }
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
                            return;
                    }

                    if (Money < price)
                    {
                        return;
                    }
                    Money -= price;

                    if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
                    {

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
            // Left click event
            jeepCount++;
            UpdateJeepCount();
            totalJeepCount++;
            IsBuilding = Tile.ShopType.None;
            Money = Money - jeepPrice;
        }

        if(visitorTimer > 0f)
        {
            // Decrease the timer
            visitorTimer -= ScaledDeltaTime;
        }
        else
        {
            // Reset the timer
            visitorTimer = visitorInterval;
            AttemptVisitorSpawn();
        }

        WinningConditionCheck(); // Check for winning conditions
    }
    /// <summary>
    /// Attempts to spawn a visitor based on the current game state.
    /// </summary>
    private void AttemptVisitorSpawn()
    {
        float baseSpawnChance = 0.5f;
        int maxFee = 100; // Maximum entry fee
        float feePenalty = Mathf.Clamp01(EntryFee / maxFee); 
        float reviewBonus = satisfaction == 0 ? 1 : satisfaction / 5f;
        float finalSpawnChance = baseSpawnChance * (1f - feePenalty) * reviewBonus;

        // Adjust spawn chance based on game difficulty
        if (UnityEngine.Random.value < finalSpawnChance && jeepCount > 0)
        {
            int tourists = UnityEngine.Random.Range(1, 5);
            Money += EntryFee * tourists;
            Visitors += tourists;
            UpdateVisitorCount();
            SpawnJeep(tourists);
        }
    }
    /// <summary>
    /// Spawns a jeep with a specified number of tourists.
    /// </summary>
    /// <param name="tourists"></param>
    private void SpawnJeep(int tourists)
    {
        // Spawn a new Jeep
        GameObject jeep = Instantiate(currentMap.prefab_jeep, new Vector3(39f,39.5f,0f) , Quaternion.identity );
        jeep.GetComponent<Jeep>().SetManager(this);
        jeep.GetComponent<Jeep>().tourists = tourists;
        jeepCount--;
        UpdateJeepCount();
        jeep.GetComponent<Jeep>().id = totalJeepCount - jeepCount;
    }
    /// <summary>
    /// Notifies the game manager that a jeep has returned home.
    /// </summary>
    /// <param name="animalCount"></param>
    /// <param name="differentAnimals"></param>
    public void JeepIsHome(int animalCount, int differentAnimals)
    {
        // Review calculation
        int review = CalculateReview(animalCount, differentAnimals);
        if (satisfaction == 0)
        {
            satisfaction = review;
        }
        else
        {
            satisfaction = (satisfaction + review) / 2;
        }
        jeepCount++;
        UpdateJeepCount();

    }
    /// <summary>
    /// Calculates the review score based on the number of animals seen and the game difficulty.
    /// </summary>
    /// <param name="animalCount"></param>
    /// <param name="differentAnimals"></param>
    /// <returns></returns>
    private int CalculateReview(int animalCount, int differentAnimals)
    {
        int difficultyAdjustment = 0;

        // Adjust the animal count based on the number of different animals seen
        switch (differentAnimals)
        {
            case 1:
                animalCount = (int)(animalCount * 0.5);
                break;
            case 2:
                break;
            case 3:
                animalCount = animalCount * 2;
                break;
            case 4:
                animalCount = animalCount * 3;
                break;
            default:
                break;
        }
        // Adjust the review score based on the game difficulty
        switch (gameDifficulty)
        {
            case Difficulty.Easy:
                difficultyAdjustment = 0;
                break;
            case Difficulty.Medium:
                difficultyAdjustment = 3;
                break;
        case Difficulty.Hard:
            difficultyAdjustment = 5;
            break;
        }
        // Calculate the review score based on the number of animals seen
        if (animalCount < (1+difficultyAdjustment)) return 1;
        if (animalCount < (3+difficultyAdjustment)) return 2;
        if (animalCount < (5+difficultyAdjustment)) return 3;
        if (animalCount < (7+difficultyAdjustment)) return 4;
        if (animalCount > (7+difficultyAdjustment)) return 5;
        return 0;
    }

    public void EnableRoadBuilding()
    {
        IsBuilding = Tile.ShopType.Road;
    }
    public void DisableRoadBuilding()
    {
        IsBuilding = Tile.ShopType.None;
    }
    public void PriceIncrease()
    { 
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
    public void UpdateJeepCount()
    {
        jeepCountText.text = jeepCount.ToString();
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
    /// <summary>
    ///  Changes the game speed when the speed button is clicked.
    /// </summary>
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
    /// <summary>
    /// Updates the speed button icon based on the current game speed.
    /// </summary>
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
    /// <summary>
    /// Saves the game data to a JSON string.
    /// </summary>
    /// <returns></returns>
    public string SaveGame()
    {
        SaveData save = new SaveData();

        // GameManager adatainak mentése
        save.gameManager = new GameManagerData
        {
            money = Money,
            jeepCount = jeepCount,
            time = TimePassed,
            difficulty = (int)gameDifficulty,
            howManyCarnivores = CurrentCarnivorousCount,
            howManyHerbivores = CurrentHerbivoresCount,
            parkName = parkName.text,
            entryFee = EntryFee,
            satisfaction = satisfaction,
            visitorCount = Visitors
        };

        // Map mentése
        save.tiles = new List<TileData>();

        for (int y = 0; y < currentMap.tile_grid.Count; y++)
        {
            for (int x = 0; x < currentMap.tile_grid[y].Count; x++)
            {
                GameObject tileObj = currentMap.tile_grid[y][x];
                Tile tileScript = tileObj.GetComponent<Tile>();

                save.tiles.Add(new TileData
                {
                    position = tileObj.transform.position,
                    type = (int)tileScript.Type,
                    foodAmount = tileScript.FoodAmount
                });
            }
        }


        save.animals = Animals.Select(animal =>
        {
            Herbivore herb = animal.GetComponent<Herbivore>();
            Carnivorous carn = animal.GetComponent<Carnivorous>();

            string animalType = animal.tag; // Pl. "Lion", "Fox" stb.

            if (herb != null)
            {
                return new AnimalData
                {
                    animalType = animalType,
                    position = animal.transform.position,
                    age = herb.age,
                };
            }
            else if (carn != null)
            {
                return new AnimalData
                {
                    animalType = animalType,
                    position = animal.transform.position,
                    age = carn.age,
                };
            }
            else
            {
                return null;
            }
        }).Where(a => a != null).ToList();


        return JsonUtility.ToJson(save, true);
    }

    /// <summary>
    /// Loads the animals from a list of AnimalData.
    /// </summary>
    /// <param name="animals"></param>
    private void LoadAnimals(List<AnimalData> animals)
    {
        foreach (AnimalData animalData in animals)
        {
            GameObject animalPrefab = null;
            switch (animalData.animalType)
            {
                case "Fox":
                    animalPrefab = fox_prefab;
                    break;
                case "Lion":
                    animalPrefab = lion_prefab;
                    break;
                case "Giraffe":
                    animalPrefab = giraffe_prefab;
                    break;
                case "Zebra":
                    animalPrefab = zebra_prefab;
                    break;
                default:
                    continue; // Skip this animal if type is unknown
            }
            GameObject newAnimal = Instantiate(animalPrefab, animalData.position, Quaternion.identity);
            Animals.Add(newAnimal);
            if (newAnimal.TryGetComponent<Herbivore>(out Herbivore herbivore))
            {
                herbivore.age = animalData.age;
            }
            else if (newAnimal.TryGetComponent<Carnivorous>(out Carnivorous carnivorous))
            {
                carnivorous.age = animalData.age;
            }
        }
    }

}
