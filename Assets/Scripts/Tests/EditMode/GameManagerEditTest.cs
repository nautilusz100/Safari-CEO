using NUnit.Framework;
using Assets.Scripts;
using System.Reflection;
using UnityEngine;
using TMPro;
using System; 
using NavMeshPlus.Components;
using Assets.Scripts.Model.Map;
using UnityEngine.UIElements;
public class GameManagerEditModeTests
{
    private GameManager gameManager;
    private GameObject gameManagerObject;

    [SetUp]
    public void SetUp()
    {
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();

        gameManager.scoreText = new GameObject().AddComponent<TextMeshProUGUI>();
        gameManager.visitorCount = new GameObject().AddComponent<TextMeshProUGUI>();
        GameObject navMesh = new GameObject("NavMesh");
        navMesh.AddComponent<NavMeshSurface>();
        gameManager.navMesh = navMesh;
        gameManager.safariMapPrefab = new GameObject().AddComponent<DummySafariMap>();

        // Create UI GameObject and UIDocument
        var uiGO = new GameObject("UI");
        var uiDoc = uiGO.AddComponent<UIDocument>();
        uiDoc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>(); // Required

        // Build fake UI hierarchy
        var root = new VisualElement();
        var moneyLabel = new Label { name = "MoneyLabel" };
        var dateButton = new UnityEngine.UIElements.Button { name = "dateButton" };
        var speedButton = new UnityEngine.UIElements.Button { name = "speedButton" };

        root.Add(moneyLabel);
        root.Add(dateButton);
        root.Add(speedButton);

        // Inject root into UIDocument using reflection
        var rootField = typeof(UIDocument).GetField("m_RootVisualElement", BindingFlags.NonPublic | BindingFlags.Instance);
        rootField?.SetValue(uiDoc, root);

        // Assign uiGameObject to GameManager using reflection
        var uiGameObjectField = typeof(GameManager).GetField("uiGameObject", BindingFlags.NonPublic | BindingFlags.Instance);
        uiGameObjectField?.SetValue(gameManager, uiGO);


        GameSettings.SelectedDifficulty = Difficulty.Easy; // Set a default difficulty for the tests    
    }
    
    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(gameManagerObject);
    }

    [Test]
    public void GetAnimalPrice_ReturnsCorrectPrice()
    {
        var lionPriceField = typeof(GameManager).GetField("lionPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int lionPrice = (int)lionPriceField.GetValue(gameManager);

        var foxPriceField = typeof(GameManager).GetField("foxPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int foxPrice = (int)foxPriceField.GetValue(gameManager);

        var giraffePriceField = typeof(GameManager).GetField("giraffePrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int giraffePrice = (int)giraffePriceField.GetValue(gameManager);

        var zebraPriceField = typeof(GameManager).GetField("zebraPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int zebraPrice = (int)zebraPriceField.GetValue(gameManager);

        Assert.AreEqual(lionPrice, gameManager.GetAnimalPrice(Draggable.AnimalType.Lion));
        Assert.AreEqual(foxPrice, gameManager.GetAnimalPrice(Draggable.AnimalType.Fox));
        Assert.AreEqual(giraffePrice, gameManager.GetAnimalPrice(Draggable.AnimalType.Giraffe));
        Assert.AreEqual(zebraPrice, gameManager.GetAnimalPrice(Draggable.AnimalType.Zebra));
    }

    private void InvokeStart(GameManager gm)
    {
        var method = typeof(GameManager).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "Couldn't find Start() method.");
        method.Invoke(gm, null);
    }


    [Test]
    public void Start_SetsValuesCorrectly_ForEasyDifficulty()
    {
        GameSettings.SelectedDifficulty = Difficulty.Easy;

        InvokeStart(gameManager);

        Assert.AreEqual(1000, gameManager.Money);
        AssertPrivateField(gameManager, "howManyVisitorsNeeded", 25);
        AssertPrivateField(gameManager, "howManyAnimalsNeededCarnivorous", 20);
        AssertPrivateField(gameManager, "howManyAnimalsNeededHerbivore", 20);
        AssertPrivateField(gameManager, "howManyDaysNeeded", 90);
        AssertPrivateField(gameManager, "howMuchMoneyNeeded", 2000);
    }

    [Test]
    public void Start_SetsValuesCorrectly_ForMediumDifficulty()
    {
        GameSettings.SelectedDifficulty = Difficulty.Medium;

        InvokeStart(gameManager);

        Assert.AreEqual(750, gameManager.Money);
        AssertPrivateField(gameManager, "howManyVisitorsNeeded", 50);
        AssertPrivateField(gameManager, "howManyAnimalsNeededCarnivorous", 30);
        AssertPrivateField(gameManager, "howManyAnimalsNeededHerbivore", 30);
        AssertPrivateField(gameManager, "howManyDaysNeeded", 180);
        AssertPrivateField(gameManager, "howMuchMoneyNeeded", 3000);
    }

    [Test]
    public void Start_SetsValuesCorrectly_ForHardDifficulty()
    {
        GameSettings.SelectedDifficulty = Difficulty.Hard;

        InvokeStart(gameManager);

        Assert.AreEqual(500, gameManager.Money);
        AssertPrivateField(gameManager, "howManyVisitorsNeeded", 75);
        AssertPrivateField(gameManager, "howManyAnimalsNeededCarnivorous", 50);
        AssertPrivateField(gameManager, "howManyAnimalsNeededHerbivore", 50);
        AssertPrivateField(gameManager, "howManyDaysNeeded", 360);
        AssertPrivateField(gameManager, "howMuchMoneyNeeded", 4000);
    }

    private void AssertPrivateField<T>(object obj, string fieldName, T expected)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field '{fieldName}' not found.");
        var actual = field.GetValue(obj);
        Assert.AreEqual(expected, actual, $"Field '{fieldName}' was expected to be {expected}, but was {actual}.");
    }

    [TestCase(2, 1, Difficulty.Easy, ExpectedResult = 2)]
    [TestCase(3, 3, Difficulty.Medium, ExpectedResult = 3)]
    [TestCase(5, 4, Difficulty.Hard, ExpectedResult = 5)]
    [TestCase(0, 1, Difficulty.Hard, ExpectedResult = 1)]
    [TestCase(10, 2, Difficulty.Easy, ExpectedResult = 5)]
    public int Test_CalculateReview(int animalCount, int differentAnimals, Difficulty difficulty)
    {
        // Set private field `gameDifficulty`
        typeof(GameManager)
            .GetField("gameDifficulty", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameManager, difficulty);

        // Call private method `CalculateReview`
        MethodInfo method = typeof(GameManager)
            .GetMethod("CalculateReview", BindingFlags.NonPublic | BindingFlags.Instance);

        object result = method.Invoke(gameManager, new object[] { animalCount, differentAnimals });

        return (int)result;
    }

    [Test]
    public void ToggleRoadBuilding_Test()
    {
        Assert.AreEqual(Tile.ShopType.Animal, gameManager.IsBuilding);
        gameManager.EnableRoadBuilding();
        Assert.AreEqual(Tile.ShopType.Road, gameManager.IsBuilding);
        gameManager.DisableRoadBuilding();
        Assert.AreEqual(Tile.ShopType.None, gameManager.IsBuilding);
    }

    /*
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
     
     */
    [Test]
    public void EntryFeeAdjustment_Test()
    {
        InvokeStart(gameManager);
        Assert.AreEqual(50, gameManager.EntryFee);
        Assert.AreEqual("$" + gameManager.EntryFee.ToString(),gameManager.scoreText.text);
        gameManager.PriceIncrease();
        Assert.AreEqual(51, gameManager.EntryFee);
        Assert.AreEqual("$" + gameManager.EntryFee.ToString(), gameManager.scoreText.text);
        gameManager.PriceDecrease();
        Assert.AreEqual(50, gameManager.EntryFee);
        Assert.AreEqual("$" + gameManager.EntryFee.ToString(), gameManager.scoreText.text);
    }

    [Test]
    public void GameSpeedChange_Test()
    {
        var method = typeof(GameManager).GetMethod("ChangeSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.AreEqual(GameManager.GameSpeed.Normal, gameManager.CurrentGameSpeed);
        method?.Invoke(gameManager, null);
        Assert.AreEqual(GameManager.GameSpeed.Double, gameManager.CurrentGameSpeed);
        method?.Invoke(gameManager, null);
        Assert.AreEqual(GameManager.GameSpeed.Triple, gameManager.CurrentGameSpeed);
        method?.Invoke(gameManager, null);
        Assert.AreEqual(GameManager.GameSpeed.Normal, gameManager.CurrentGameSpeed);
    }
}

public class DummySafariMap : SafariMap
{
    public override void CreateMap()
    {
        // Do nothing — it's a stub
    }
}
